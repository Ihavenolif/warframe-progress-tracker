using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;
using System.Text.Json;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;
using System.Text.Json.Nodes;
using EFCore.BulkExtensions;


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
    /// <returns></returns>
    public Task UpdatePlayerMasteryAsync(Player player, string jsonData);
}

public class MasteryService : IMasteryService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public MasteryService(WarframeTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player)
    {
        var result = _dbContext.Database
            .SqlQuery<MasteryItemDTO>(
                @$"SELECT coalesce(p.username, {player.username}) as username, pim.xp_gained as xpGained, item.xp_required as xpRequired, item.name as itemName, item.type as itemType, item.item_class as itemClass
                FROM player p
                JOIN player_items_mastery pim ON p.id = pim.player_id
                RIGHT JOIN item USING (unique_name)
                WHERE p.id = {player.id} OR p.id IS NULL
                AND item.xp_required IS NOT NULL");

        return await result.ToListAsync();

    }

    // TODO: Fuckton of validation
    public async Task UpdatePlayerMasteryAsync(Player player, string jsonData)
    {
        JsonNode root = JsonNode.Parse(jsonData) ?? throw new ArgumentException("Invalid JSON data");

        List<string> allRecipes = await _dbContext.recipes.Select(r => r.unique_name).ToListAsync();
        List<string> allItems = await _dbContext.items.Select(i => i.unique_name).ToListAsync();

        JsonArray xpInfo = root["XPInfo"]!.AsArray() ?? throw new ArgumentException("Invalid JSON data: Missing XPInfo");
        JsonArray recipes = root["Recipes"]!.AsArray() ?? throw new ArgumentException("Invalid JSON data: Missing Recipes");
        JsonArray miscItems = root["MiscItems"]!.AsArray() ?? throw new ArgumentException("Invalid JSON data: Missing MiscItems");

        List<Player_items_mastery> masteryItems = [.. xpInfo
            .Where(x => allItems.Contains((x ?? throw new ArgumentException("Invalid JSON data: Invalid XPInfo entry"))["ItemType"]!.GetValue<string>()))
            .Select(x => new Player_items_mastery
            {
                unique_name = (x ?? throw new ArgumentException("Invalid JSON data: Invalid XPInfo entry"))["ItemType"]!.GetValue<string>(),
                player_id = player.id,
                xp_gained = x["XP"]!.GetValue<int>()
            })];
        List<Player_item> recipeItems = [.. recipes
            .Where(x => allRecipes.Contains((x ?? throw new ArgumentException("Invalid JSON data: Invalid Recipes entry"))["ItemType"]!.GetValue<string>()))
            .Select(x => new Player_item
            {
                unique_name = (x ?? throw new ArgumentException("Invalid JSON data: Invalid Recipes entry"))["ItemType"]!.GetValue<string>(),
                player_id = player.id,
                item_count = x["ItemCount"]!.GetValue<int>()
            })];
        List<Player_item> miscItemEntries = [.. miscItems
            .Where(x => allItems.Contains((x ?? throw new ArgumentException("Invalid JSON data: Invalid MiscItems entry"))["ItemType"]!.GetValue<string>()))
            .Select(x => new Player_item
            {
                unique_name = (x ?? throw new ArgumentException("Invalid JSON data: Invalid MiscItems entry"))["ItemType"]!.GetValue<string>(),
                player_id = player.id,
                item_count = x["ItemCount"]!.GetValue<int>()
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