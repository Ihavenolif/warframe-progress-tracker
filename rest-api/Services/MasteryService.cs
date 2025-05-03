using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;

namespace rest_api.Services;

public interface IMasteryService
{
    public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player);
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
}