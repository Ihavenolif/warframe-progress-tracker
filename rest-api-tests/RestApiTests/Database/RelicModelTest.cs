using Microsoft.EntityFrameworkCore;
using rest_api.Models;

namespace rest_api_testing.Dababase;

public class RelicModelTest
{
    [Fact]
    public async Task StoresRewardsAndMultipleAliasesForSameRefinement()
    {
        await using var context = new WarframeTrackerDbContextTest();

        var reward = new Item
        {
            unique_name = "/Lotus/Types/Recipes/Weapons/TestPrimeBlueprint",
            name = "Test Prime Blueprint",
            type = "Recipe",
            item_class = "MiscItems"
        };
        var intactA = new Item
        {
            unique_name = "/Lotus/Types/Game/Projections/TestPrimeABronze",
            name = "Lith T1 Intact",
            type = "Relic",
            item_class = "MiscItems"
        };
        var intactB = new Item
        {
            unique_name = "/Lotus/Types/Game/Projections/TestPrimeBBronze",
            name = "Lith T1 Intact",
            type = "Relic",
            item_class = "MiscItems"
        };
        var relic = new Relic
        {
            Name = "Lith T1",
            Era = RelicEra.Lith,
            Variants =
            {
                new RelicVariant { Item = intactA, Refinement = RelicRefinement.Intact },
                new RelicVariant { Item = intactB, Refinement = RelicRefinement.Intact }
            },
            Rewards =
            {
                new RelicReward
                {
                    Reward = reward,
                    Rarity = RelicRewardRarity.Rare,
                    ItemCount = 1
                }
            }
        };

        context.relics.Add(relic);
        await context.SaveChangesAsync();

        var stored = await context.relics
            .Include(value => value.Variants)
            .Include(value => value.Rewards)
            .SingleAsync();

        Assert.Equal(RelicEra.Lith, stored.Era);
        Assert.Equal(2, stored.Variants.Count);
        Assert.All(stored.Variants, variant => Assert.Equal(RelicRefinement.Intact, variant.Refinement));
        Assert.Single(stored.Rewards);
        Assert.Equal(RelicRewardRarity.Rare, stored.Rewards.Single().Rarity);
    }
}
