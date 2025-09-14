using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;
using System.Text.Json;
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

        List<string> allMissionTags = await _dbContext.missions.Select(m => m.UniqueName!).ToListAsync();

        List<MissionCompletion> missionEntries = [.. missions
            .Select(x => new MissionCompletion
            {
                UniqueName = validateMissionItems(x!)["Tag"]!.GetValue<string>(),
                PlayerId = player.id,
                CompletionCount = x!["Completes"]!.GetValue<int>(),
                SPComplete = validateMissionItems(x!)["Tier"]?.GetValue<int>() == 1
            })
            .Where(x => allMissionTags.Contains(x.UniqueName!))];

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
                .ToArrayAsync();
            int masteryXp = masteryItemsForXp.Sum(item => item.MasteryPoints);
            int missionXp = await _dbContext.mission_completions
                .Where(mc => mc.PlayerId == player.id)
                .Join(_dbContext.missions,
                    mc => mc.UniqueName,
                    m => m.UniqueName,
                    (mc, m) => new { mc.SPComplete, m.MasteryXp })
                .SumAsync(mc => mc.SPComplete ? mc.MasteryXp * 2 : mc.MasteryXp);


            int totalXp = masteryXp + missionXp + duviriSkills * 1500 + railjackSkills * 1500;

            player.TotalMasteryXp = totalXp;

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
}