using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using rest_api.DTO;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;
using rest_api_testing.Fixtures;

namespace rest_api_testing.ServiceTests;

public class MasteryServiceSnapshotTest
{
    [Fact]
    public void MasterySnapshotRemovesRowsAbsentFromLatestImport()
    {
        Player_items_mastery retained = MasteryItem("/Items/Retained", 100);
        Player_items_mastery stale = MasteryItem("/Items/Stale", 200);

        List<Player_items_mastery> result = MasteryService.FindStaleMasteryEntries(
            [retained],
            [retained, stale]);

        Assert.Same(stale, Assert.Single(result));
    }

    [Fact]
    public void RepeatedMasterySnapshotIsIdempotent()
    {
        Player_items_mastery item = MasteryItem("/Items/Retained", 100);

        List<Player_items_mastery> result = MasteryService.FindStaleMasteryEntries([item], [item]);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(0, true, 0)]
    [InlineData(1, false, 1000)]
    [InlineData(1, true, 2000)]
    public void MissionXpRequiresNormalCompletion(int completionCount, bool spComplete, int expectedXp)
    {
        int result = MasteryService.CalculateMissionMasteryXp(completionCount, spComplete, 1000);

        Assert.Equal(expectedXp, result);
    }

    [Fact]
    public async Task ImportRemovesStaleMasteryAndRepeatedSnapshotIsIdempotent()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "snapshot-player", TotalMasteryXp = 1 };
        Item retained = Item("/Items/Retained");
        Item stale = Item("/Items/Stale");
        context.AddRange(player, retained, stale);
        await context.SaveChangesAsync();
        context.player_items_masteries.Add(MasteryItem(stale.unique_name, 450000, player.id));
        context.missions.Add(new Mission
        {
            UniqueName = "/Missions/Incomplete",
            Name = "Incomplete",
            MasteryXp = 1000
        });
        await context.SaveChangesAsync();
        MasteryService service = CreateService(context, retained.unique_name, stale.unique_name);
        string snapshot = """
            {
              "XPInfo": [{ "ItemType": "/Items/Retained", "XP": 450000 }],
              "Recipes": [],
              "MiscItems": [],
              "PlayerLevel": 10,
              "PlayerSkills": {},
              "Missions": [{ "Tag": "/Missions/Incomplete", "Completes": 0, "Tier": 1 }]
            }
            """;

        MasteryImportReceiptDto firstReceipt = await service.UpdatePlayerMasteryAsync(player, snapshot);
        int progressCount = await context.mastery_progress_entries.CountAsync();
        MasteryImportReceiptDto repeatedReceipt = await service.UpdatePlayerMasteryAsync(player, snapshot);

        Player_items_mastery mastery = Assert.Single(await context.player_items_masteries.ToListAsync());
        Assert.Equal(retained.unique_name, mastery.unique_name);
        Assert.Equal(3000, player.TotalMasteryXp);
        Assert.Equal(progressCount, await context.mastery_progress_entries.CountAsync());
        Assert.Single(await context.mission_completions.ToListAsync());
        Assert.True(firstReceipt.Changed);
        Assert.False(repeatedReceipt.Changed);
        Assert.Equal(2, await context.mastery_import_receipts.CountAsync());
    }

    [Fact]
    public async Task FullImportPersistsCurrentStateAndCalculatesAllMasteryXpSources()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "full-import-player" };
        AddImportCatalog(context, player);
        await context.SaveChangesAsync();
        MasteryService service = CreateService(
            context,
            MasteryImportFixture.WeaponUniqueName,
            MasteryImportFixture.WarframeUniqueName,
            MasteryImportFixture.StaleUniqueName);

        MasteryImportReceiptDto receipt = await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.FullImport);

        Assert.Equal(20, player.mastery_rank);
        Assert.Equal(10, player.duviri_skills);
        Assert.Equal(15, player.railjack_skills);
        Assert.Equal(49000, player.TotalMasteryXp);
        Assert.Equal(2, await context.player_items_masteries.CountAsync());
        Assert.Equal(2, await context.mission_completions.CountAsync());
        Assert.Empty(await context.mastery_progress_entries.ToListAsync());
        Assert.True(receipt.Changed);
        Assert.Equal(MasteryImportFixture.SourceVersion, receipt.SourceVersion);
        Assert.Equal(4, receipt.ProcessedCount);
        Assert.Equal(0, receipt.SkippedCount);
        Assert.Equal(20, receipt.ResultingMasteryRank);
        Assert.Equal(49000, receipt.ResultingTotalMasteryXp);
    }

    [Fact]
    public async Task ZeroCountKnownRelicIsProcessedWithoutWarning()
    {
        await using var context = new WarframeTrackerDbContextTest();
        const string uniqueName = "/Lotus/Types/Game/Projections/TestPrimeBronze";
        Player player = new() { username = "zero-relic-player" };
        Item item = Item(uniqueName);
        Relic relic = new() { Name = "Test Relic", Era = RelicEra.Lith };
        relic.Variants.Add(new RelicVariant
        {
            UniqueName = uniqueName,
            Item = item,
            Refinement = RelicRefinement.Intact
        });
        context.AddRange(player, item, relic);
        await context.SaveChangesAsync();
        MasteryService service = CreateService(context, uniqueName);

        MasteryImportReceiptDto receipt = await service.UpdatePlayerMasteryAsync(player, $$"""
            {
              "XPInfo": [],
              "Recipes": [],
              "MiscItems": [{ "ItemType": "{{uniqueName}}", "ItemCount": 0 }],
              "PlayerLevel": 0,
              "PlayerSkills": {},
              "Missions": []
            }
            """);

        Assert.Equal(1, receipt.ProcessedCount);
        Assert.Equal(0, receipt.SkippedCount);
    }

    [Fact]
    public async Task ImportHistoryRecordsOnlyProgressAndPersistsCorrections()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "history-player" };
        AddImportCatalog(context, player);
        await context.SaveChangesAsync();
        MasteryService service = CreateService(
            context,
            MasteryImportFixture.WeaponUniqueName,
            MasteryImportFixture.WarframeUniqueName,
            MasteryImportFixture.StaleUniqueName);

        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.FirstImport);
        Assert.Equal(4300, player.TotalMasteryXp);
        Assert.Empty(await context.mastery_progress_entries.ToListAsync());

        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.RepeatedImport);
        Assert.Empty(await context.mastery_progress_entries.ToListAsync());

        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.ProgressiveImport);
        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.UnchangedImport);

        MasteryProgressEntry progress = await context.mastery_progress_entries
            .Include(entry => entry.LeveledItems)
            .Include(entry => entry.Missions)
            .SingleAsync();
        Assert.Equal(4300, progress.PreviousTotalMasteryXp);
        Assert.Equal(8900, progress.CurrentTotalMasteryXp);
        Assert.Equal(4600, progress.MasteryXpGained);
        Assert.Equal(600, progress.LeveledItems.Sum(item => item.MasteryXpGained));
        MasteryProgressMission mission = Assert.Single(progress.Missions);
        Assert.False(mission.PreviousSPComplete);
        Assert.True(mission.CurrentSPComplete);
        Assert.Equal(1000, mission.MasteryXpGained);

        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.CorrectedImport);

        Assert.Equal(1100, player.TotalMasteryXp);
        Assert.Single(await context.mastery_progress_entries.ToListAsync());
        Assert.Equal(500, await context.player_items_masteries
            .Where(item => item.unique_name == MasteryImportFixture.WeaponUniqueName)
            .Select(item => item.xp_gained)
            .SingleAsync());

        List<MasteryImportReceipt> receipts = await context.mastery_import_receipts.OrderBy(receipt => receipt.Id).ToListAsync();
        Assert.Equal(5, receipts.Count);
        Assert.True(receipts[0].Changed);
        Assert.False(receipts[1].Changed);
        Assert.True(receipts[2].Changed);
        Assert.False(receipts[3].Changed);
        Assert.True(receipts[4].Changed);

        MasteryImportReceiptDto? latestReceipt = await service.GetLatestImportReceiptAsync(player);
        Assert.NotNull(latestReceipt);
        Assert.Equal(receipts[4].Id, latestReceipt.Id);
    }

    [Fact]
    public async Task FailedReconciliationRollsBackMasteryAndInventoryDeletes()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "rollback-player", mastery_rank = 5, TotalMasteryXp = 3000 };
        Item stale = Item("/Items/Stale");
        context.AddRange(player, stale);
        await context.SaveChangesAsync();
        context.player_items_masteries.Add(MasteryItem(stale.unique_name, 450000, player.id));
        context.player_items.Add(new Player_item
        {
            unique_name = stale.unique_name,
            player_id = player.id,
            item_count = 2
        });
        await context.SaveChangesAsync();
        MasteryService service = CreateService(context, stale.unique_name);
        context.ThrowAfterNextSave = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdatePlayerMasteryAsync(player, """
            {
              "XPInfo": [],
              "Recipes": [],
              "MiscItems": [],
              "PlayerLevel": 20,
              "PlayerSkills": {},
              "Missions": []
            }
            """));

        context.ChangeTracker.Clear();
        Assert.Single(await context.player_items_masteries.ToListAsync());
        Assert.Single(await context.player_items.ToListAsync());
        Player storedPlayer = await context.players.SingleAsync();
        Assert.Equal(5, storedPlayer.mastery_rank);
        Assert.Equal(3000, storedPlayer.TotalMasteryXp);
        Assert.Empty(await context.mastery_import_receipts.ToListAsync());
    }

    private static Player_items_mastery MasteryItem(string uniqueName, int xpGained) => new()
    {
        unique_name = uniqueName,
        player_id = 7,
        xp_gained = xpGained
    };

    private static Player_items_mastery MasteryItem(string uniqueName, int xpGained, int playerId) => new()
    {
        unique_name = uniqueName,
        player_id = playerId,
        xp_gained = xpGained
    };

    private static Item Item(string uniqueName) => new()
    {
        unique_name = uniqueName,
        name = uniqueName,
        type = "Weapon",
        item_class = "LongGuns",
        xp_required = 450000
    };

    internal static void AddImportCatalog(WarframeTrackerDbContextTest context, Player player)
    {
        context.AddRange(
            player,
            new Item
            {
                unique_name = MasteryImportFixture.WeaponUniqueName,
                name = "Test Weapon",
                type = "Weapon",
                item_class = "LongGuns",
                xp_required = 450000
            },
            new Item
            {
                unique_name = MasteryImportFixture.WarframeUniqueName,
                name = "Test Warframe",
                type = "Warframe",
                item_class = "Warframes",
                xp_required = 900000
            },
            Item(MasteryImportFixture.StaleUniqueName),
            new Mission
            {
                UniqueName = MasteryImportFixture.MissionUniqueName,
                Name = "Test Mission",
                Planet = "Earth",
                MasteryXp = 1000
            },
            new Mission
            {
                UniqueName = MasteryImportFixture.SecondMissionUniqueName,
                Name = "Second Mission",
                Planet = "Venus",
                MasteryXp = 500
            });
    }

    private static MasteryService CreateService(WarframeTrackerDbContextTest context, params string[] itemUniqueNames)
    {
        return new MasteryService(
            context,
            new TestItemService(itemUniqueNames),
            null!,
            NullLogger<MasteryService>.Instance);
    }

    private sealed class TestItemService(IEnumerable<string> itemUniqueNames) : IItemService
    {
        public Task<IEnumerable<string>> GetItemUniqueNamesAsync() => Task.FromResult(itemUniqueNames);
        public Task<IEnumerable<string>> GetRecipeUniqueNamesAsync() => Task.FromResult<IEnumerable<string>>([]);
        public Task<IEnumerable<Item>> GetItemsAsync() => throw new NotImplementedException();
        public Task<IEnumerable<Recipe>> GetRecipesAsync() => throw new NotImplementedException();
        public Task<Item> GetItemByUniqueNameAsync() => throw new NotImplementedException();
        public Task<Recipe> GetRecipeByUniqueNameAsync() => throw new NotImplementedException();
        public Task<bool> UpdateItemDatabaseAsync() => throw new NotImplementedException();
    }
}
