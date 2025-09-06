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
    public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player);
    /// <summary>
    /// Update the player's mastery data based on the provided JSON data.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="jsonData"></param>
    /// <throws cref="ArgumentException">Invalid JSON data</throws>
    /// <throws cref="System.Text.Json.JsonReaderException">Invalid JSON format</throws>
    /// <returns></returns>
    public Task UpdatePlayerMasteryAsync(Player player, string jsonData);
    public Task<IEnumerable<MasteryItemWithComponentsDTO>> GetMasteryInfoWithComponentsByPlayerAsync(Player player);
    public Task<IEnumerable<MasteryItemNewDTO>> GetMasteryInfoByPlayerNewAsync(Player player);
}

public class MasteryService : IMasteryService
{
    private readonly WarframeTrackerDbContext _dbContext;
    private readonly IItemService _itemService;

    public MasteryService(WarframeTrackerDbContext dbContext, IItemService itemService)
    {
        _dbContext = dbContext;
        _itemService = itemService;
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

    public async Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player)
    {
        // THIS IS ASS!!! USERNAME SHOULD BE SANITIZED - SQL INJECTION POSSIBLE!!!!!!! - maybe? does efcore sanitize? idk
        var result = _dbContext.Database
            .SqlQuery<MasteryItemDTO>(
                @$"SELECT {player.username} as username, xp_gained as xpGained, xp_required as xpRequired, name as itemName, type as itemType, item_class as itemClass, unique_name as uniqueName
                from (
                    select * from item
                    where xp_required is not null
                ) left join (
                    select * from player_items_mastery
                    where player_items_mastery.player_id = {player.id}
                ) using (unique_name);");

        return await result.ToListAsync();
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

        using var transaction = _dbContext.Database.BeginTransaction();
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

            _dbContext.SaveChanges();
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<MasteryItemWithComponentsDTO>> GetMasteryInfoWithComponentsByPlayerAsync(Player player)
    {
        var items = (await GetMasteryInfoByPlayerAsync(player)).ToDictionary(item => item.uniqueName!, item => new MasteryItemWithComponentsDTO(item));

        // Monster query. also in testUnhinged.sql. TODO: Rename this file and put somewhere sensible.
        var missingItems = await _dbContext.Database
            .SqlQuery<UnownedItemDTO>(
                @$"with missing_items as (
                    select * from item where xp_required is not null and not exists( 
                        select 1 from player_items_mastery 
                        where player_items_mastery.player_id = {player.id}
                        and item.unique_name = player_items_mastery.unique_name 
                    )
                ), recipe_items as (
                    select missing_items.*, recipe_item.name as recipe_name, recipe_item.unique_name as recipe_item_unique_name from missing_items
                    left join recipe on recipe.result_item = missing_items.unique_name
                    left join item recipe_item on recipe.unique_name = recipe_item.unique_name
                ), player_items_filtered as (
                    select * from player_items
                    where player_items.player_id = {player.id}
                ), player_recipe_items as (
                    select recipe_items.*, coalesce(player_items_filtered.item_count, 0) as recipe_owned_count from recipe_items
                    left join player_items_filtered 
                    on recipe_items.recipe_item_unique_name = player_items_filtered.unique_name
                ), player_recipes_with_ingredients as (
                    select player_recipe_items.unique_name, json_agg(
                        json_build_object(
                            'name', item_component.name,
                            'uniqueName', item_component.unique_name,
                            'countOwned', coalesce(owned_ingredient.item_count, 0),
                            'countRequired', recipe_ingredients.ingredient_count,
                            'isCraftable', recipe_for_ingredient.unique_name is not null,
                            'recipeOwned', owned_recipe_for_ingredient.item_count is not null and owned_recipe_for_ingredient.item_count > 0
                        )
                    ) filter (where item_component.unique_name is not null) as components_json from player_recipe_items
                    left join recipe_ingredients on player_recipe_items.recipe_item_unique_name = recipe_ingredients.recipe_name
                    left join item item_component on recipe_ingredients.item_ingredient = item_component.unique_name
                    left join player_items_filtered owned_ingredient on item_component.unique_name = owned_ingredient.unique_name
                    left join recipe recipe_for_ingredient on item_component.unique_name = recipe_for_ingredient.result_item
                    left join player_items_filtered owned_recipe_for_ingredient on owned_recipe_for_ingredient.unique_name = recipe_for_ingredient.unique_name
                    group by player_recipe_items.unique_name

                )
                select * from player_recipes_with_ingredients;").ToListAsync();

        foreach (var item in missingItems)
        {
            items[item.unique_name!].components = item.components;
        }

        return items.Values;

        throw new NotImplementedException();
    }

    private async Task<Dictionary<string, MasteryItemNewDTO>> GetRawItems()
    {
        var items = await _dbContext.Database.SqlQuery<MasteryItemNewDTO>(@$"SELECT 
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

    public async Task<IEnumerable<MasteryItemNewDTO>> GetMasteryInfoByPlayerNewAsync(Player player)
    {

        var rawItems = await GetRawItems();
        var playerData = await GetPlayerData(player);

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
}