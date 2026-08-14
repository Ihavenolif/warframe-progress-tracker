using rest_api.DTOs.Relics;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class RelicServiceTest
{
    [Fact]
    public async Task ReadUsesOnlyCurrentPlayerOwnership()
    {
        await using var context = new WarframeTrackerDbContextTest();
        var (first, second, relic) = await SeedCatalogAsync(context);
        context.player_items.AddRange(
            new Player_item { player_id = first.id, unique_name = relic.Variants.First().UniqueName, item_count = 2 },
            new Player_item { player_id = second.id, unique_name = relic.Variants.First().UniqueName, item_count = 9 });
        await context.SaveChangesAsync();
        var service = new RelicService(context);

        var result = await service.GetRelicAsync(first.id, relic.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalOwned);
        Assert.Equal(2, result.Variants.Single(value => value.Refinement == "Intact").Quantity);
    }

    [Fact]
    public async Task ReadAppliesSearchFiltersAndPagination()
    {
        await using var context = new WarframeTrackerDbContextTest();
        var (player, _, relic) = await SeedCatalogAsync(context);
        context.player_items.Add(new Player_item
        {
            player_id = player.id,
            unique_name = relic.Variants.Single(value => value.Refinement == RelicRefinement.Radiant).UniqueName,
            item_count = 3
        });
        await AddRelicAsync(context, "Axi Z9", RelicEra.Axi, "/Relics/AxiZ9Intact", "Other Prime Part");
        var splitMatch = await AddRelicAsync(context, "Axi A1", RelicEra.Axi, "/Relics/AxiA1Intact", "Braton Prime Stock");
        splitMatch.Rewards.Add(new RelicReward
        {
            RewardUniqueName = "/Rewards/AkstilettoPrimeBarrel",
            Rarity = RelicRewardRarity.Uncommon,
            ItemCount = 1,
            Reward = new Item
            {
                unique_name = "/Rewards/AkstilettoPrimeBarrel",
                name = "Akstiletto Prime Barrel",
                type = "PrimePart",
                item_class = "MiscItems"
            }
        });
        await context.SaveChangesAsync();
        var service = new RelicService(context);

        var filtered = await service.GetRelicsAsync(player.id, new RelicQueryDTO
        {
            Search = "bra bar",
            Era = "lith",
            Owned = "owned"
        });
        var page = await service.GetRelicsAsync(player.id, new RelicQueryDTO { Page = 1, PageSize = 1, Sort = "name" });
        var eraSorted = await service.GetRelicsAsync(player.id, new RelicQueryDTO { Sort = "era" });

        Assert.Single(filtered.Items);
        Assert.Equal(relic.Id, filtered.Items[0].Id);
        Assert.Equal(3, filtered.Items[0].TotalOwned);
        Assert.Single(page.Items);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(["Lith", "Axi", "Axi"], eraSorted.Items.Select(item => item.Era));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetRelicsAsync(player.id,
            new RelicQueryDTO { Page = int.MaxValue, PageSize = 100 }));
    }

    private static async Task<(Player First, Player Second, Relic Relic)> SeedCatalogAsync(WarframeTrackerDbContextTest context)
    {
        var first = new Player { username = $"first-{Guid.NewGuid()}" };
        var second = new Player { username = $"second-{Guid.NewGuid()}" };
        context.players.AddRange(first, second);
        var relic = await AddRelicAsync(context, "Lith A1", RelicEra.Lith, "/Relics/LithA1Intact", "Braton Prime Barrel");
        relic.Variants.Add(new RelicVariant
        {
            UniqueName = "/Relics/LithA1Radiant",
            Refinement = RelicRefinement.Radiant,
            Item = new Item
            {
                unique_name = "/Relics/LithA1Radiant",
                name = "Lith A1 Radiant",
                type = "Relic",
                item_class = "MiscItems"
            }
        });
        await context.SaveChangesAsync();
        return (first, second, relic);
    }

    private static async Task<Relic> AddRelicAsync(
        WarframeTrackerDbContextTest context,
        string name,
        RelicEra era,
        string variantUniqueName,
        string rewardName)
    {
        var rewardUniqueName = $"/Rewards/{name.Replace(" ", string.Empty)}";
        var relic = new Relic
        {
            Name = name,
            Era = era,
            Variants =
            {
                new RelicVariant
                {
                    UniqueName = variantUniqueName,
                    Refinement = RelicRefinement.Intact,
                    Item = new Item
                    {
                        unique_name = variantUniqueName,
                        name = $"{name} Intact",
                        type = "Relic",
                        item_class = "MiscItems"
                    }
                }
            },
            Rewards =
            {
                new RelicReward
                {
                    RewardUniqueName = rewardUniqueName,
                    Rarity = RelicRewardRarity.Rare,
                    ItemCount = 1,
                    Reward = new Item
                    {
                        unique_name = rewardUniqueName,
                        name = rewardName,
                        type = "PrimePart",
                        item_class = "MiscItems"
                    }
                }
            }
        };
        context.relics.Add(relic);
        await context.SaveChangesAsync();
        return relic;
    }
}
