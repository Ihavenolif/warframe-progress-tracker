using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

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

        await service.UpdatePlayerMasteryAsync(player, snapshot);
        int progressCount = await context.mastery_progress_entries.CountAsync();
        await service.UpdatePlayerMasteryAsync(player, snapshot);

        Player_items_mastery mastery = Assert.Single(await context.player_items_masteries.ToListAsync());
        Assert.Equal(retained.unique_name, mastery.unique_name);
        Assert.Equal(3000, player.TotalMasteryXp);
        Assert.Equal(progressCount, await context.mastery_progress_entries.CountAsync());
        Assert.Single(await context.mission_completions.ToListAsync());
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
