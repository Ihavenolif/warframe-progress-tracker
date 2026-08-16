using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using rest_api.Data;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Fixtures;
using Testcontainers.PostgreSql;

namespace rest_api_testing.Database;

public sealed class MasteryPostgreSqlIntegrationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16.10-bookworm")
        .WithDatabase("mastery_tests")
        .WithUsername("mastery_tests")
        .WithPassword("mastery-tests-password")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task MigrationsAndBulkImportReconcileSnapshotOnPostgreSql()
    {
        var options = new DbContextOptionsBuilder<WarframeTrackerDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var context = new WarframeTrackerDbContext(options);
        await context.Database.MigrateAsync();
        Player player = new() { username = "postgres-player", TotalMasteryXp = 1 };
        AddImportCatalog(context, player);
        await context.SaveChangesAsync();
        context.player_items_masteries.Add(new Player_items_mastery
        {
            player_id = player.id,
            unique_name = MasteryImportFixture.StaleUniqueName,
            xp_gained = 450000
        });
        await context.SaveChangesAsync();
        MasteryService service = new(
            context,
            new DatabaseItemService(context),
            null!,
            NullLogger<MasteryService>.Instance);

        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.FullImport);
        await service.UpdatePlayerMasteryAsync(player, MasteryImportFixture.FullImport);

        Assert.Equal(49000, player.TotalMasteryXp);
        Assert.Equal(2, await context.player_items_masteries.CountAsync());
        Assert.DoesNotContain(
            await context.player_items_masteries.Select(item => item.unique_name).ToListAsync(),
            uniqueName => uniqueName == MasteryImportFixture.StaleUniqueName);
        Assert.Equal(2, await context.mission_completions.CountAsync());
        Assert.Single(await context.mastery_progress_entries.ToListAsync());
        List<MasteryImportReceipt> receipts = await context.mastery_import_receipts.OrderBy(receipt => receipt.Id).ToListAsync();
        Assert.Equal(2, receipts.Count);
        Assert.True(receipts[0].Changed);
        Assert.False(receipts[1].Changed);
        Assert.NotEmpty(await context.Database.GetAppliedMigrationsAsync());
    }

    private static void AddImportCatalog(WarframeTrackerDbContext context, Player player)
    {
        context.AddRange(
            player,
            Item(MasteryImportFixture.WeaponUniqueName, "Test Weapon", "Weapon", 450000),
            Item(MasteryImportFixture.WarframeUniqueName, "Test Warframe", "Warframe", 900000),
            Item(MasteryImportFixture.StaleUniqueName, "Stale", "Weapon", 450000),
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

    private static Item Item(string uniqueName, string name, string type, int xpRequired) => new()
    {
        unique_name = uniqueName,
        name = name,
        type = type,
        item_class = type,
        xp_required = xpRequired
    };

    private sealed class DatabaseItemService(WarframeTrackerDbContext context) : IItemService
    {
        public async Task<IEnumerable<string>> GetItemUniqueNamesAsync() =>
            await context.items.Select(item => item.unique_name).ToListAsync();

        public async Task<IEnumerable<string>> GetRecipeUniqueNamesAsync() =>
            await context.recipes.Select(recipe => recipe.unique_name).ToListAsync();

        public Task<IEnumerable<Item>> GetItemsAsync() => throw new NotSupportedException();
        public Task<IEnumerable<Recipe>> GetRecipesAsync() => throw new NotSupportedException();
        public Task<Item> GetItemByUniqueNameAsync() => throw new NotSupportedException();
        public Task<Recipe> GetRecipeByUniqueNameAsync() => throw new NotSupportedException();
        public Task<bool> UpdateItemDatabaseAsync() => throw new NotSupportedException();
    }
}
