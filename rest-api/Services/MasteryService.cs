using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;
using System.Text.Json.Nodes;
using SQLitePCL;


namespace rest_api.Services;

public interface IMasteryService
{
    /// <summary>
    /// Update the player's mastery data based on the provided JSON data.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="jsonData"></param>
    /// <throws cref="ArgumentException">Invalid JSON data</throws>
    /// <throws cref="System.Text.Json.JsonReaderException">Invalid JSON format</throws>
    /// <returns></returns>
    public Task UpdatePlayerMasteryAsync(Player player, string jsonData);
    public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player);
    public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByClanAsync(Clan clan);
    public Task<List<DashboardProgressEntryDTO>> GetLatestProgressEntriesAsync(Player player);
    public Task<List<DashboardProgressDayDTO>> GetDailyProgressAsync(Player player, int days);
}

public class MasteryService : IMasteryService
{
    private readonly WarframeTrackerDbContext _dbContext;
    private readonly IItemService _itemService;
    private readonly IClanService _clanService;

    public MasteryService(WarframeTrackerDbContext dbContext, IItemService itemService, IClanService clanService)
    {
        _dbContext = dbContext;
        _itemService = itemService;
        _clanService = clanService;
    }

    private JsonNode validateMasteryItem(JsonNode item)
    {
        if (item == null) throw new ArgumentException("Invalid JSON data: Invalid XPInfo entry");
        if (item["ItemType"] == null || item["XP"] == null) throw new ArgumentException("Invalid JSON data: Invalid XPInfo entry");
        if (item["ItemType"]!.GetValue<string>() == null) throw new ArgumentException("Invalid JSON data: Invalid XPInfo entry");
        if (item["XP"]!.GetValue<int>() < 0) throw new ArgumentException("Invalid JSON data: Invalid XPInfo entry");
        return item;
    }

    private JsonNode validateMiscItem(JsonNode item)
    {
        if (item == null) throw new ArgumentException("Invalid JSON data: Invalid MiscItems entry");
        if (item["ItemType"] == null || item["ItemCount"] == null) throw new ArgumentException("Invalid JSON data: Invalid MiscItems entry");
        if (item["ItemType"]!.GetValue<string>() == null) throw new ArgumentException("Invalid JSON data: Invalid MiscItems entry");
        if (item["ItemCount"]!.GetValue<int>() < 0) throw new ArgumentException("Invalid JSON data: Invalid MiscItems entry");
        return item;
    }

    private JsonNode validateMissionItems(JsonNode item)
    {
        if (item == null) throw new ArgumentException("Invalid JSON data: Invalid Missions entry");
        if (item["Completes"] == null) throw new ArgumentException("Invalid JSON data: Invalid Missions entry");
        if (item["Tag"]!.GetValue<string>() == null) throw new ArgumentException("Invalid JSON data: Invalid Missions entry");
        return item;
    }

    public static int GetItemRank(int xpRequired, int xpGained)
    {
        int[] warframeThresholds = [
            0, 1000, 4000, 9000, 16000, 25000, 36000, 49000, 64000, 81000,
            100000, 121000, 144000, 169000, 196000, 225000, 256000, 289000,
            324000, 361000, 400000, 441000, 484000, 529000, 576000, 625000,
            676000, 729000, 784000, 841000, 900000, 961000, 1024000, 1089000,
            1156000, 1225000, 1296000, 1369000, 1444000, 1521000, 1600000
        ];

        int[] weaponThresholds = [
            0, 500, 2000, 4500, 8000, 12500, 18000, 24500, 32000, 40500,
            50000, 60500, 72000, 84500, 98000, 112500, 128000, 144500,
            162000, 180500, 200000, 220500, 242000, 264500, 288000, 312500,
            338000, 364500, 392000, 420500, 450000, 480500, 512000, 544500,
            578000, 612500, 648000, 684500, 722000, 760500, 800000
        ];

        int[] thresholds = xpRequired == 1600000 || xpRequired == 900000 ? warframeThresholds :
            xpRequired == 800000 || xpRequired == 450000 ? weaponThresholds :
            throw new InvalidOperationException("Unknown item xp_required value");
        int maxRank = xpRequired == 1600000 || xpRequired == 800000 ? 40 : 30;

        for (int rank = 1; rank <= maxRank; rank++)
        {
            if (xpGained < thresholds[rank]) return rank - 1;
        }

        return maxRank;
    }

    private static int GetMasteryPointsPerRank(int xpRequired)
    {
        return xpRequired == 1600000 || xpRequired == 900000 ? 200 :
            xpRequired == 800000 || xpRequired == 450000 ? 100 :
            throw new InvalidOperationException("Unknown item xp_required value");
    }

    // TODO: Fuckton of validation
    // Also TODO: Write some tests
    public async Task UpdatePlayerMasteryAsync(Player player, string jsonData)
    {
        JsonNode root = JsonNode.Parse(jsonData) ?? throw new ArgumentException("Invalid JSON data");

        IEnumerable<string> allRecipes = await _itemService.GetRecipeUniqueNamesAsync();
        IEnumerable<string> allItems = await _itemService.GetItemUniqueNamesAsync();

        JsonArray xpInfo = (root["XPInfo"] ?? throw new ArgumentException("Invalid JSON data: Missing XPInfo")).AsArray() ?? throw new ArgumentException("Invalid JSON data: Invalid XPInfo");
        JsonArray recipes = (root["Recipes"] ?? throw new ArgumentException("Invalid JSON data: Missing Recipes")).AsArray() ?? throw new ArgumentException("Invalid JSON data: Invalid Recipes");
        JsonArray miscItems = (root["MiscItems"] ?? throw new ArgumentException("Invalid JSON data: Missing MiscItems")).AsArray() ?? throw new ArgumentException("Invalid JSON data: Invalid MiscItems");
        int masteryRank = (root["PlayerLevel"] ?? throw new ArgumentException("Invalid JSON data: Missing PlayerLevel")).GetValue<int>();

        List<Player_items_mastery> masteryItems = [.. xpInfo
            .Where(x => allItems.Contains(validateMasteryItem(x!)["ItemType"]!.GetValue<string>()))
            .Select(x => new Player_items_mastery
            {
                unique_name = validateMasteryItem(x!)["ItemType"]!.GetValue<string>(),
                player_id = player.id,
                xp_gained = x!["XP"]!.GetValue<int>()
            })];

        List<string> masteryItemUniqueNames = masteryItems.Select(item => item.unique_name).Distinct().ToList();
        var previousMasteryItems = await _dbContext.player_items_masteries
            .Where(item => item.player_id == player.id && masteryItemUniqueNames.Contains(item.unique_name))
            .ToDictionaryAsync(item => item.unique_name, item => item.xp_gained);
        var itemInfo = await _dbContext.items
            .Where(item => masteryItemUniqueNames.Contains(item.unique_name))
            .ToDictionaryAsync(item => item.unique_name);
        int previousTotalMasteryXp = player.TotalMasteryXp;

        List<MasteryProgressItem> leveledItems = [.. masteryItems
            .Select(item =>
            {
                var info = itemInfo[item.unique_name];
                int previousXp = previousMasteryItems.GetValueOrDefault(item.unique_name, 0);
                int previousRank = GetItemRank(info.xp_required!.Value, previousXp);
                int currentRank = GetItemRank(info.xp_required!.Value, item.xp_gained);
                return new MasteryProgressItem
                {
                    Item = info,
                    Name = info.name,
                    PreviousXp = previousXp,
                    CurrentXp = item.xp_gained,
                    MasteryXpGained = (currentRank - previousRank) * GetMasteryPointsPerRank(info.xp_required.Value)
                };
            })
            .Where(item => item.CurrentRank > item.PreviousRank)];
        List<Player_item> recipeItems = [.. recipes
            .Where(x => allRecipes.Contains(validateMiscItem(x!)["ItemType"]!.GetValue<string>()))
            .Select(x => new Player_item
            {
                unique_name = validateMiscItem(x!)["ItemType"]!.GetValue<string>(),
                player_id = player.id,
                item_count = x!["ItemCount"]!.GetValue<int>()
            })];
        List<Player_item> miscItemEntries = [.. miscItems
            .Where(x => allItems.Contains(validateMiscItem(x!)["ItemType"]!.GetValue<string>()))
            .Select(x => new Player_item
            {
                unique_name = validateMiscItem(x!)["ItemType"]!.GetValue<string>(),
                player_id = player.id,
                item_count = x!["ItemCount"]!.GetValue<int>()
            })];


        var missingItems = xpInfo
            .Where(x => !allItems.Contains(validateMasteryItem(x!)["ItemType"]!.GetValue<string>()))
            .Select(x => (validateMasteryItem(x!)["ItemType"]!.GetValue<string>(), x!["XP"]!.GetValue<int>()))
            .Distinct()
            .ToList();

        JsonNode allSkills = root["PlayerSkills"] ?? throw new ArgumentException("Invalid JSON data: Missing PlayerSkills");

        int duviriSkills = (allSkills["LPS_DRIFT_RIDING"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_DRIFT_COMBAT"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_DRIFT_OPPORTUNITY"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_DRIFT_ENDURANCE"]?.GetValue<int>() ?? 0);

        int railjackSkills = (allSkills["LPS_PILOTING"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_TACTICAL"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_GUNNERY"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_ENGINEERING"]?.GetValue<int>() ?? 0) +
                        (allSkills["LPS_COMMAND"]?.GetValue<int>() ?? 0);

        JsonArray missions = (root["Missions"] ?? throw new ArgumentException("Invalid JSON data: Missing Missions")).AsArray() ?? throw new ArgumentException("Invalid JSON data: Invalid Missions");

        var missionInfo = await _dbContext.missions
            .Select(m => new { m.UniqueName, m.Name, m.Planet, m.MasteryXp })
            .ToDictionaryAsync(m => m.UniqueName!);

        List<MissionCompletion> missionEntries = [.. missions
            .Select(x => new MissionCompletion
            {
                UniqueName = validateMissionItems(x!)["Tag"]!.GetValue<string>(),
                PlayerId = player.id,
                CompletionCount = x!["Completes"]!.GetValue<int>(),
                SPComplete = validateMissionItems(x!)["Tier"]?.GetValue<int>() == 1
            })
            .Where(x => missionInfo.ContainsKey(x.UniqueName!))];

        List<string> missionUniqueNames = missionEntries.Select(mission => mission.UniqueName!).Distinct().ToList();
        var previousMissionCompletions = await _dbContext.mission_completions
            .Where(mission => mission.PlayerId == player.id && missionUniqueNames.Contains(mission.UniqueName!))
            .ToDictionaryAsync(mission => mission.UniqueName!);

        List<MasteryProgressMission> missionProgress = [.. missionEntries
            .Select(mission =>
            {
                var info = missionInfo[mission.UniqueName!];
                previousMissionCompletions.TryGetValue(mission.UniqueName!, out MissionCompletion? previous);

                int previousCompletionCount = previous?.CompletionCount ?? 0;
                bool previousSPComplete = previous?.SPComplete ?? false;
                int masteryXpGained = 0;
                if (previousCompletionCount == 0 && mission.CompletionCount > 0)
                {
                    masteryXpGained += info.MasteryXp;
                }
                if (!previousSPComplete && mission.SPComplete)
                {
                    masteryXpGained += info.MasteryXp;
                }

                return new MasteryProgressMission
                {
                    UniqueName = mission.UniqueName!,
                    Name = info.Name,
                    Planet = info.Planet,
                    PreviousCompletionCount = previousCompletionCount,
                    CurrentCompletionCount = mission.CompletionCount,
                    PreviousSPComplete = previousSPComplete,
                    CurrentSPComplete = mission.SPComplete,
                    MasteryXpGained = masteryXpGained
                };
            })
            .Where(mission =>
                mission.PreviousCompletionCount == 0 && mission.CurrentCompletionCount > 0 ||
                !mission.PreviousSPComplete && mission.CurrentSPComplete)];

        using var transaction = _dbContext.Database.BeginTransaction();
        player.mastery_rank = masteryRank;
        player.duviri_skills = duviriSkills;
        player.railjack_skills = railjackSkills;
        try
        {
            await _dbContext.BulkInsertOrUpdateAsync(masteryItems, new BulkConfig
            {
                UpdateByProperties = new List<string> { "unique_name", "player_id" },
                PropertiesToInclude = new List<string> { "xp_gained" }
            });
            await _dbContext.BulkInsertOrUpdateAsync(recipeItems, new BulkConfig
            {
                UpdateByProperties = new List<string> { "unique_name", "player_id" },
                PropertiesToInclude = new List<string> { "item_count" }
            });
            await _dbContext.BulkInsertOrUpdateAsync(miscItemEntries, new BulkConfig
            {
                UpdateByProperties = new List<string> { "unique_name", "player_id" },
                PropertiesToInclude = new List<string> { "item_count" }
            });
            await _dbContext.BulkInsertOrUpdateAsync(missionEntries, new BulkConfig
            {
                UpdateByProperties = new List<string> { "UniqueName", "PlayerId" },
                PropertiesToInclude = new List<string> { "CompletionCount", "SPComplete" }
            });

            var masteryItemsForXp = await _dbContext.player_items_masteries
                .Where(pim => pim.player_id == player.id)
                .Include(pim => pim.item)
                .ToListAsync();
            int masteryXp = masteryItemsForXp.Sum(item => item.MasteryPoints);
            int missionXp = await _dbContext.mission_completions
                .Where(mc => mc.PlayerId == player.id)
                .Join(_dbContext.missions,
                    mc => mc.UniqueName,
                    m => m.UniqueName,
                    (mc, m) => new { mc.SPComplete, m.MasteryXp })
                .SumAsync(mc => mc.SPComplete ? mc.MasteryXp * 2 : mc.MasteryXp);


            int totalXp = masteryXp + missionXp + (duviriSkills * 1500) + (railjackSkills * 1500);

            player.TotalMasteryXp = totalXp;

            if (previousTotalMasteryXp > 0)
            {
                _dbContext.mastery_progress_entries.Add(new MasteryProgressEntry
                {
                    PlayerId = player.id,
                    CreatedAt = DateTime.Now,
                    PreviousTotalMasteryXp = previousTotalMasteryXp,
                    CurrentTotalMasteryXp = totalXp,
                    MasteryXpGained = totalXp - previousTotalMasteryXp,
                    LeveledItems = leveledItems,
                    Missions = missionProgress
                });
            }

            _dbContext.SaveChanges();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }

    private async Task<Dictionary<string, MasteryItemDTO>> GetRawItems()
    {
        var items = await _dbContext.Database.SqlQuery<MasteryItemDTO>(@$"SELECT 
            name as itemName,
            type as itemType,
            item_class as itemClass,
            unique_name as uniqueName,
            recipe_name as recipeName,
            recipe_unique_name as recipeUniqueName,
            xp_required as xpRequired
            FROM xp_items_with_recipes_and_components
            group by name, type, item_class, unique_name, recipe_name, recipe_unique_name, xp_required")
            .ToDictionaryAsync(item => item.uniqueName!, item => item);

        return items;
    }

    private class PlayerData
    {
        public string? unique_name { get; set; }
        public int? xp_gained { get; set; }
        public bool? blueprint_owned { get; set; }
        public string? components_json { get; set; }
    }

    private async Task<List<PlayerData>> GetPlayerData(Player player)
    {
        var playerData = await _dbContext.Database.SqlQuery<PlayerData>(@$"
            select 
                xp_items_with_recipes_and_components.unique_name, 
                player_items_mastery.xp_gained as xp_gained,
                bp_ownership.item_count is not null and bp_ownership.item_count > 0 as blueprint_owned,
                json_agg(
                    json_build_object(
                        'name', xp_items_with_recipes_and_components.component_name,
                        'uniqueName', xp_items_with_recipes_and_components.component_unique_name,
                        'countOwned', COALESCE(component_ownership.item_count, 0),
                        'countRequired', xp_items_with_recipes_and_components.ingredient_count,
                        'isCraftable', xp_items_with_recipes_and_components.component_bp_unique_name is not null,
                        'blueprintOwned', component_bp_ownership.item_count is not null and component_bp_ownership.item_count > 0
                    )
                ) filter (where player_items_mastery.xp_gained is null and xp_items_with_recipes_and_components.component_unique_name is not null) as components_json 
            from xp_items_with_recipes_and_components
            full join (
                select * from player_items_mastery where player_id = {player.id} --parameter player_id
            ) player_items_mastery on xp_items_with_recipes_and_components.unique_name = player_items_mastery.unique_name
            left join (
                select * from player_items where player_id = {player.id} --parameter player_id
            ) bp_ownership on xp_items_with_recipes_and_components.recipe_unique_name = bp_ownership.unique_name and player_items_mastery.xp_gained is null 
            left join (
                select * from player_items where player_id = {player.id} --parameter player_id
            ) component_ownership on xp_items_with_recipes_and_components.component_unique_name = component_ownership.unique_name and player_items_mastery.xp_gained is null
            left join (
                select * from player_items where player_id = {player.id} --parameter player_id
            ) component_bp_ownership on xp_items_with_recipes_and_components.component_bp_unique_name = component_bp_ownership.unique_name and player_items_mastery.xp_gained is null
            group by xp_items_with_recipes_and_components.unique_name,
                player_items_mastery.xp_gained,
                bp_ownership.item_count;
        ").ToListAsync();

        return playerData;
    }

    public async Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player)
    {

        Dictionary<string, MasteryItemDTO> rawItems = await GetRawItems();
        List<PlayerData> playerData = await GetPlayerData(player);

        foreach (var item in playerData)
        {
            rawItems[item.unique_name!].players[player.username] = new PlayerMasteryItemDTO
            {
                xpGained = item.xp_gained,
                blueprintOwned = item.blueprint_owned,
                components_json = item.components_json
            };
        }


        return rawItems.Values;
    }

    public async Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByClanAsync(Clan clan)
    {
        Dictionary<string, MasteryItemDTO> rawItems = await GetRawItems();

        foreach (Player player in clan.players)
        {
            List<PlayerData> playerData = await GetPlayerData(player);

            foreach (var item in playerData)
            {
                rawItems[item.unique_name!].players[player.username] = new PlayerMasteryItemDTO
                {
                    xpGained = item.xp_gained,
                    blueprintOwned = item.blueprint_owned,
                    components_json = item.components_json
                };
            }
        }

        return rawItems.Values;
    }

    public async Task<List<DashboardProgressEntryDTO>> GetLatestProgressEntriesAsync(Player player)
    {
        List<MasteryProgressEntry> latestEntries = await _dbContext.mastery_progress_entries
            .Where(entry => entry.PlayerId == player.id)
            .OrderByDescending(entry => entry.CreatedAt)
            .Take(10)
            .Include(entry => entry.LeveledItems)
                .ThenInclude(item => item.Item)
            .Include(entry => entry.Missions)
            .ToListAsync();

        return [.. latestEntries.Select(entry => new DashboardProgressEntryDTO
        {
            id = entry.Id,
            createdAt = entry.CreatedAt,
            previousTotalMasteryXp = entry.PreviousTotalMasteryXp,
            currentTotalMasteryXp = entry.CurrentTotalMasteryXp,
            masteryXpGained = entry.PreviousTotalMasteryXp == 0
                ? 0
                : entry.CurrentTotalMasteryXp - entry.PreviousTotalMasteryXp,
            leveledItems = entry.PreviousTotalMasteryXp == 0 ? [] : [.. entry.LeveledItems.Select(item => new DashboardProgressItemDTO
            {
                uniqueName = item.Item.unique_name,
                name = item.Name ?? item.Item.name,
                previousRank = item.PreviousRank,
                currentRank = item.CurrentRank,
                previousXp = item.PreviousXp,
                currentXp = item.CurrentXp,
                masteryXpGained = item.MasteryXpGained
            })],
            missions = entry.PreviousTotalMasteryXp == 0 ? [] : [.. entry.Missions.Select(mission => new DashboardProgressMissionDTO
            {
                uniqueName = mission.UniqueName,
                name = mission.Name,
                planet = mission.Planet,
                completed = mission.PreviousCompletionCount == 0 && mission.CurrentCompletionCount > 0,
                steelPathCompleted = !mission.PreviousSPComplete && mission.CurrentSPComplete,
                masteryXpGained = mission.MasteryXpGained
            })]
        })];
    }

    public async Task<List<DashboardProgressDayDTO>> GetDailyProgressAsync(Player player, int days)
    {
        DateTime startDate = DateTime.Today.AddDays(-(days - 1));

        var dailyProgress = await _dbContext.mastery_progress_entries
            .Where(entry => entry.PlayerId == player.id && entry.CreatedAt.Date >= startDate)
            .GroupBy(entry => entry.CreatedAt.Date)
            .Select(group => new DashboardProgressDayDTO
            {
                date = group.Key,
                masteryXpGained = group.Sum(entry => entry.PreviousTotalMasteryXp == 0
                    ? 0
                    : entry.CurrentTotalMasteryXp - entry.PreviousTotalMasteryXp)
            })
            .ToListAsync();

        Dictionary<DateTime, int> dailyProgressByDate = dailyProgress.ToDictionary(day => day.date.Date, day => day.masteryXpGained);

        return [.. Enumerable.Range(0, days).Select(offset =>
            {
                DateTime date = startDate.AddDays(offset);
                return new DashboardProgressDayDTO
                {
                    date = date,
                    masteryXpGained = dailyProgressByDate.GetValueOrDefault(date, 0)
                };
            })];
    }
}
