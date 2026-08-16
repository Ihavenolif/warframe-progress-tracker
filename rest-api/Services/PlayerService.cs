using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Services;

public interface IPlayerService
{
    public Task<Player?> FindPlayerByUsernameAsync(string username);
    public Task<Player?> FindAccessiblePlayerByUsernameAsync(string username, int? viewerPlayerId, bool isAdmin);
    public Task<List<Player>> GetAccessiblePlayersAsync(int? viewerPlayerId, bool isAdmin);
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

    public async Task<Player?> FindAccessiblePlayerByUsernameAsync(string username, int? viewerPlayerId, bool isAdmin)
    {
        return await GetAccessiblePlayers(viewerPlayerId, isAdmin)
            .FirstOrDefaultAsync(player => player.username == username);
    }

    public async Task<List<Player>> GetAccessiblePlayersAsync(int? viewerPlayerId, bool isAdmin)
    {
        return await GetAccessiblePlayers(viewerPlayerId, isAdmin).ToListAsync();
    }

    // Admins can view all profiles. Other users can view self and accepted clan members.
    private IQueryable<Player> GetAccessiblePlayers(int? viewerPlayerId, bool isAdmin)
    {
        IQueryable<Player> players = _dbContext.players.AsNoTracking();

        if (isAdmin) return players;
        if (viewerPlayerId == null) return players.Where(_ => false);

        int playerId = viewerPlayerId.Value;
        return players.Where(target =>
            target.id == playerId ||
            target.clans.Any(clan => clan.players.Any(member => member.id == playerId)));
    }
}
