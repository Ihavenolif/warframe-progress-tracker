using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Services;

public interface IItemService
{
    public Task<IEnumerable<Item>> GetItemsAsync();
    public Task<IEnumerable<Recipe>> GetRecipesAsync();

    public Task<Item> GetItemByUniqueNameAsync();
    public Task<Recipe> GetRecipeByUniqueNameAsync();

    public Task<IEnumerable<string>> GetItemUniqueNamesAsync();
    public Task<IEnumerable<string>> GetRecipeUniqueNamesAsync();

    public Task<bool> UpdateItemDatabaseAsync();
}

public class ItemService : IItemService
{
    private readonly WarframeTrackerDbContext _dbContext;
    private readonly ConfigurationService _config;

    public ItemService(WarframeTrackerDbContext dbContext, ConfigurationService config)
    {
        _dbContext = dbContext;
        _config = config;
    }
    public Task<Item> GetItemByUniqueNameAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        return await _dbContext.items.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetItemUniqueNamesAsync()
    {
        return await _dbContext.items.Select(i => i.unique_name).ToListAsync();
    }

    public Task<Recipe> GetRecipeByUniqueNameAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Recipe>> GetRecipesAsync()
    {
        return await _dbContext.recipes.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetRecipeUniqueNamesAsync()
    {
        return await _dbContext.recipes.Select(i => i.unique_name).ToListAsync();
    }

    public async Task<bool> UpdateItemDatabaseAsync()
    {
        string url = $"http://{_config.DataUpdateServerUrl}:5000/data-update";
        using HttpClient client = new HttpClient();
        var response = await client.PostAsync(url, null);

        await _dbContext.Database.ExecuteSqlRawAsync("REFRESH MATERIALIZED VIEW xp_items_with_recipes_and_components;");

        return response.IsSuccessStatusCode;
    }
}