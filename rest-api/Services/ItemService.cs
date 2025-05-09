using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Services;

public interface IItemService{
    public Task<IEnumerable<Item>> GetItemsAsync();
    public Task<IEnumerable<Recipe>> GetRecipesAsync();

    public Task<Item> GetItemByUniqueNameAsync();
    public Task<Recipe> GetRecipeByUniqueNameAsync();

    public Task<IEnumerable<string>> GetItemUniqueNamesAsync();
    public Task<IEnumerable<string>> GetRecipeUniqueNamesAsync();
}

public class ItemService : IItemService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public ItemService(WarframeTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
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
}