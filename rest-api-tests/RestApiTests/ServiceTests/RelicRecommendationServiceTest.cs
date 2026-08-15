using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class RelicRecommendationServiceTest
{
    [Fact]
    public async Task RecommendationsListEveryOwnerAndRankByRawNeed()
    {
        await using var context = new WarframeTrackerDbContextTest();
        var player = new Player { username = "squad-member" };
        var secondPlayer = new Player { username = "second-owner" };
        context.players.AddRange(player, secondPlayer);
        var rootBlueprint = Item("/RootBp", "Test Prime Blueprint");
        var root = Item("/Root", "Test Prime", 450000);
        var bladeBlueprint = Item("/BladeBp", "Test Prime Blade Blueprint");
        var blade = Item("/Blade", "Test Prime Blade");
        context.items.AddRange(rootBlueprint, root, bladeBlueprint, blade);
        context.recipes.AddRange(
            new Recipe
            {
                unique_name = rootBlueprint.unique_name,
                result_item = root.unique_name,
                recipe_ingredients =
                {
                    new Recipe_ingredient { item_ingredient = blade.unique_name, ingredient_count = 2 }
                }
            },
            new Recipe { unique_name = bladeBlueprint.unique_name, result_item = blade.unique_name });

        var bladeRelic = Relic("Lith A1", "/Relic/A1Intact", bladeBlueprint, RelicRewardRarity.Uncommon);
        bladeRelic.Variants.Add(new RelicVariant
        {
            UniqueName = "/Relic/A1Radiant",
            Refinement = RelicRefinement.Radiant,
            Item = Item("/Relic/A1Radiant", "Lith A1 Radiant")
        });
        var blueprintRelic = Relic("Lith B1", "/Relic/B1Intact", rootBlueprint, RelicRewardRarity.Rare);
        context.relics.AddRange(bladeRelic, blueprintRelic);
        await context.SaveChangesAsync();
        context.player_items.AddRange(
            new Player_item { player_id = player.id, unique_name = "/Relic/A1Intact", item_count = 1 },
            new Player_item { player_id = player.id, unique_name = "/Relic/A1Radiant", item_count = 2 },
            new Player_item { player_id = player.id, unique_name = "/Relic/B1Intact", item_count = 20 },
            new Player_item { player_id = secondPlayer.id, unique_name = "/Relic/A1Intact", item_count = 4 });
        context.player_items_masteries.Add(new Player_items_mastery
        {
            player_id = secondPlayer.id,
            unique_name = root.unique_name,
            xp_gained = 0
        });
        await context.SaveChangesAsync();

        var result = await new RelicRecommendationService(context)
            .GetRecommendationsAsync([player.id, secondPlayer.id], 10);

        Assert.Equal(["Lith A1", "Lith B1"], result.Recommendations.Select(value => value.Name));
        Assert.Equal(2, result.Recommendations[0].Score);
        Assert.Equal(2, result.Recommendations[0].Owners.Count);
        var firstOwner = result.Recommendations[0].Owners.Single(owner => owner.PlayerId == player.id);
        Assert.Equal(3, firstOwner.TotalCount);
        Assert.Equal(1, firstOwner.Refinements.Intact);
        Assert.Equal(2, firstOwner.Refinements.Radiant);
        var secondOwner = result.Recommendations[0].Owners.Single(owner => owner.PlayerId == secondPlayer.id);
        Assert.Equal(4, secondOwner.TotalCount);
        Assert.Equal(4, secondOwner.Refinements.Intact);
        Assert.Equal(2, result.Recommendations[0].UsefulRewards.Single().Players.Single().MissingCount);
        Assert.Equal(["Test Prime"], result.Recommendations[0].UsefulRewards.Single().Players.Single().RequiredFor);
        Assert.Equal(1, result.Recommendations[1].Score);
    }

    [Fact]
    public async Task PlayerWithoutProfileDataIsRejected()
    {
        await using var context = new WarframeTrackerDbContextTest();
        var player = new Player { username = "not-imported" };
        context.players.Add(player);
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<MissingProfileDataException>(() =>
            new RelicRecommendationService(context).GetRecommendationsAsync([player.id], 10));

        Assert.Contains(player.username, exception.Message);
    }

    [Fact]
    public async Task PrimePrefixGearIsIncluded()
    {
        await using var context = new WarframeTrackerDbContextTest();
        var player = new Player { username = "prefix-owner" };
        context.players.Add(player);
        var blueprint = Item("/PrefixBp", "Prime Laser Rifle Blueprint");
        var gear = Item("/Prefix", "Prime Laser Rifle", 450000);
        context.items.AddRange(blueprint, gear);
        context.recipes.Add(new Recipe { unique_name = blueprint.unique_name, result_item = gear.unique_name });
        var relic = Relic("Meso P1", "/Relic/P1Intact", blueprint, RelicRewardRarity.Rare);
        relic.Era = RelicEra.Meso;
        context.relics.Add(relic);
        await context.SaveChangesAsync();
        context.player_items.Add(new Player_item
        {
            player_id = player.id,
            unique_name = "/Relic/P1Intact",
            item_count = 1
        });
        await context.SaveChangesAsync();

        var result = await new RelicRecommendationService(context)
            .GetRecommendationsAsync([player.id], 10);

        Assert.Single(result.Recommendations);
        Assert.Equal(1, result.Recommendations[0].Score);
    }

    private static Item Item(string uniqueName, string name, int? xpRequired = null)
    {
        return new Item
        {
            unique_name = uniqueName,
            name = name,
            type = "PrimePart",
            item_class = "MiscItems",
            xp_required = xpRequired
        };
    }

    private static Relic Relic(
        string name,
        string variantUniqueName,
        Item reward,
        RelicRewardRarity rarity)
    {
        return new Relic
        {
            Name = name,
            Era = RelicEra.Lith,
            Variants =
            {
                new RelicVariant
                {
                    UniqueName = variantUniqueName,
                    Refinement = RelicRefinement.Intact,
                    Item = Item(variantUniqueName, $"{name} Intact")
                }
            },
            Rewards =
            {
                new RelicReward
                {
                    RewardUniqueName = reward.unique_name,
                    Reward = reward,
                    Rarity = rarity,
                    ItemCount = 1
                }
            }
        };
    }
}
