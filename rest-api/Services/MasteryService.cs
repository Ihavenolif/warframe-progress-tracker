using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;

namespace rest_api.Services;

public interface IMasteryService
{
    public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerIdAsync(int playerId);
}

public class MasteryService : IMasteryService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public MasteryService(WarframeTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerIdAsync(int playerId)
    {
        throw new NotImplementedException();
    }
}