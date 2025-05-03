using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Services;

public interface IPlayerService
{
    public Task<Player?> FindPlayerByUsernameAsync(string username);

}

public class PlayerService : IPlayerService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public PlayerService(WarframeTrackerDbContext context)
    {
        this._dbContext = context;
    }

    public async Task<Player?> FindPlayerByUsernameAsync(string username)
    {
        return await _dbContext.players.FirstOrDefaultAsync(u => u.username == username);
    }
}