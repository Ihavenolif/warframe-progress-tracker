using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;
using rest_api.Services;
using Testcontainers.PostgreSql;

namespace RestApiPostgreSqlBenchmarks;

[MemoryDiagnoser]
public class ClanMasteryPostgreSqlBenchmarks
{
    private static readonly string ProfileDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

    private PostgreSqlContainer _postgres = null!;
    private WarframeTrackerDbContext _dbContext = null!;
    private MasteryService _masteryService = null!;
    private Clan _clan = null!;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:16.10-bookworm")
            .WithDatabase("benchmark")
            .WithUsername("benchmark")
            .WithPassword("benchmark-password")
            .Build();
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<WarframeTrackerDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _dbContext = new WarframeTrackerDbContext(options);
        await _dbContext.Database.MigrateAsync();

        var profileFiles = Directory.EnumerateFiles(ProfileDirectory, "out_*.json")
            .OrderBy(Path.GetFileName, StringComparer.Ordinal)
            .ToArray();
        if (profileFiles.Length != 3)
        {
            throw new InvalidOperationException(
                $"Expected exactly three out_*.json files, found {profileFiles.Length}.");
        }

        var profiles = new List<string>(profileFiles.Length);
        foreach (var profileFile in profileFiles)
        {
            var rawJson = await File.ReadAllTextAsync(profileFile);
            profiles.Add(NormalizeInventoryPayload(rawJson));
        }

        await SeedCatalogAsync(profiles);

        var userService = new UserService(_dbContext);
        var clanService = new ClanService(_dbContext);
        var itemService = new BenchmarkItemService(_dbContext);
        _masteryService = new MasteryService(
            _dbContext,
            itemService,
            clanService,
            NullLogger<MasteryService>.Instance);

        var players = new List<Player>(3);
        for (var index = 0; index < profiles.Count; index++)
        {
            var user = await userService.CreateUserAsync(
                $"benchmark-user-{index + 1}",
                "benchmark-password");
            await userService.AddPlayerToUser(user, $"benchmark-player-{index + 1}");
            players.Add(user.player!);
        }

        _clan = await clanService.CreateClanAsync(players[0], "benchmark-clan")
            ?? throw new InvalidOperationException("Failed to create benchmark clan.");
        await clanService.AddPlayerToClanAsync(_clan, players[1]);
        await clanService.AddPlayerToClanAsync(_clan, players[2]);

        // Imports stay sequential because MasteryService shares one DbContext.
        for (var index = 0; index < profiles.Count; index++)
        {
            await _masteryService.UpdatePlayerMasteryAsync(players[index], profiles[index]);
        }

        await ValidateSeedAsync();
        _dbContext.ChangeTracker.Clear();
        _clan = await _dbContext.clans
            .Include(clan => clan.players)
            .SingleAsync(clan => clan.name == "benchmark-clan");

        _ = (await _masteryService.GetMasteryInfoByClanAsync(_clan)).ToList();
    }

    [Benchmark]
    public async Task<List<MasteryItemDTO>> GetThreePlayerClanMasteryAsync()
    {
        return (await _masteryService.GetMasteryInfoByClanAsync(_clan)).ToList();
    }

    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }
    }

    private async Task SeedCatalogAsync(IEnumerable<string> profiles)
    {
        var items = new Dictionary<string, Item>(StringComparer.Ordinal);
        var recipes = new Dictionary<string, Recipe>(StringComparer.Ordinal);
        var missions = new Dictionary<string, Mission>(StringComparer.Ordinal);
        var projectionUniqueNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var profile in profiles)
        {
            using var document = JsonDocument.Parse(profile);
            var root = document.RootElement;

            foreach (var entry in root.GetProperty("XPInfo").EnumerateArray())
            {
                var uniqueName = entry.GetProperty("ItemType").GetString()!;
                var item = GetOrAddItem(items, uniqueName, "Mastery", "Benchmark mastery item");
                item.xp_required = 1600000;
            }

            foreach (var entry in root.GetProperty("Recipes").EnumerateArray())
            {
                var uniqueName = entry.GetProperty("ItemType").GetString()!;
                var blueprint = GetOrAddItem(items, uniqueName, "Recipe", "Benchmark blueprint");
                if (recipes.ContainsKey(uniqueName))
                {
                    continue;
                }

                var resultUniqueName = $"/Benchmark/RecipeResult/{recipes.Count}";
                var result = GetOrAddItem(items, resultUniqueName, "Result", "Benchmark recipe result");
                recipes.Add(uniqueName, new Recipe
                {
                    unique_name = uniqueName,
                    result_item = resultUniqueName,
                    unique_nameNavigation = blueprint,
                    result_itemNavigation = result
                });
            }

            foreach (var entry in root.GetProperty("MiscItems").EnumerateArray())
            {
                var uniqueName = entry.GetProperty("ItemType").GetString()!;
                GetOrAddItem(items, uniqueName, "Misc", "Benchmark inventory item");
                if (uniqueName.Contains("/Projections/", StringComparison.OrdinalIgnoreCase))
                {
                    projectionUniqueNames.Add(uniqueName);
                }
            }

            foreach (var entry in root.GetProperty("Missions").EnumerateArray())
            {
                var uniqueName = entry.GetProperty("Tag").GetString()!;
                missions.TryAdd(uniqueName, new Mission
                {
                    UniqueName = uniqueName,
                    Name = "Benchmark mission",
                    Planet = "Benchmark",
                    MasteryXp = 0,
                    Type = "Benchmark"
                });
            }
        }

        _dbContext.items.AddRange(items.Values);
        _dbContext.recipes.AddRange(recipes.Values);
        _dbContext.missions.AddRange(missions.Values);

        var relicIndex = 0;
        foreach (var uniqueName in projectionUniqueNames)
        {
            var relic = new Relic
            {
                Name = $"Benchmark Relic {++relicIndex}",
                Era = (RelicEra)((relicIndex - 1) % 4)
            };
            relic.Variants.Add(new RelicVariant
            {
                UniqueName = uniqueName,
                Refinement = GetRelicRefinement(uniqueName),
                Item = items[uniqueName]
            });
            _dbContext.relics.Add(relic);
        }

        await _dbContext.SaveChangesAsync();
        await _dbContext.Database.ExecuteSqlRawAsync(
            "REFRESH MATERIALIZED VIEW xp_items_with_recipes_and_components;");
        _dbContext.ChangeTracker.Clear();
    }

    private async Task ValidateSeedAsync()
    {
        if (await _dbContext.registered_users.CountAsync() != 3
            || await _dbContext.players.CountAsync() != 3)
        {
            throw new InvalidOperationException("Benchmark seed must contain three users and players.");
        }

        var memberCount = await _dbContext.Entry(_clan)
            .Collection(clan => clan.players)
            .Query()
            .CountAsync();
        if (memberCount != 3)
        {
            throw new InvalidOperationException("Benchmark clan must contain three players.");
        }

        var importedPlayerCount = await _dbContext.players
            .CountAsync(player => player.player_items_masteries.Any()
                || player.player_items.Any()
                || player.MissionsCompleted.Any());
        if (importedPlayerCount != 3)
        {
            throw new InvalidOperationException("Each benchmark player must have imported profile data.");
        }
    }

    private static Item GetOrAddItem(
        IDictionary<string, Item> items,
        string uniqueName,
        string type,
        string itemClass)
    {
        if (items.TryGetValue(uniqueName, out var item))
        {
            return item;
        }

        item = new Item
        {
            unique_name = uniqueName,
            name = Path.GetFileName(uniqueName),
            type = type,
            item_class = itemClass
        };
        items.Add(uniqueName, item);
        return item;
    }

    private static RelicRefinement GetRelicRefinement(string uniqueName)
    {
        if (uniqueName.EndsWith("Bronze", StringComparison.OrdinalIgnoreCase))
        {
            return RelicRefinement.Exceptional;
        }
        if (uniqueName.EndsWith("Silver", StringComparison.OrdinalIgnoreCase))
        {
            return RelicRefinement.Flawless;
        }
        if (uniqueName.EndsWith("Gold", StringComparison.OrdinalIgnoreCase))
        {
            return RelicRefinement.Radiant;
        }
        return RelicRefinement.Intact;
    }

    private static string NormalizeInventoryPayload(string rawJson)
    {
        using var document = JsonDocument.Parse(rawJson);
        var root = document.RootElement;
        if (root.TryGetProperty("XPInfo", out _))
        {
            return rawJson;
        }
        if (!root.TryGetProperty("InventoryJson", out var inventoryJson))
        {
            throw new InvalidDataException("Profile is missing XPInfo and InventoryJson.");
        }

        return inventoryJson.ValueKind switch
        {
            JsonValueKind.String => inventoryJson.GetString()
                ?? throw new InvalidDataException("InventoryJson is null."),
            JsonValueKind.Object => inventoryJson.GetRawText(),
            _ => throw new InvalidDataException("InventoryJson must be a string or object.")
        };
    }

    private sealed class BenchmarkItemService : IItemService
    {
        private readonly WarframeTrackerDbContext _dbContext;

        public BenchmarkItemService(WarframeTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<string>> GetItemUniqueNamesAsync()
        {
            return await _dbContext.items.Select(item => item.unique_name).ToListAsync();
        }

        public async Task<IEnumerable<string>> GetRecipeUniqueNamesAsync()
        {
            return await _dbContext.recipes.Select(recipe => recipe.unique_name).ToListAsync();
        }

        public Task<IEnumerable<Item>> GetItemsAsync() => throw new NotSupportedException();
        public Task<IEnumerable<Recipe>> GetRecipesAsync() => throw new NotSupportedException();
        public Task<Item> GetItemByUniqueNameAsync() => throw new NotSupportedException();
        public Task<Recipe> GetRecipeByUniqueNameAsync() => throw new NotSupportedException();
        public Task<bool> UpdateItemDatabaseAsync() => throw new NotSupportedException();
    }
}
