using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;
using System.Text.Json;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;
using System.Text.Json.Nodes;


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
                @$"SELECT {player.username} as username, xp_gained as xpGained, xp_required as xpRequired, name as itemName, type as itemType, item_class as itemClass
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
}