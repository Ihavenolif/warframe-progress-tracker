using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.DTOs.Relics;
using rest_api.Models;

namespace rest_api.Services;

public interface IRelicService
{
    Task<RelicPageDTO> GetRelicsAsync(int playerId, RelicQueryDTO query);
    Task<RelicDTO?> GetRelicAsync(int playerId, int id);
}

public class RelicService : IRelicService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public RelicService(WarframeTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RelicPageDTO> GetRelicsAsync(int playerId, RelicQueryDTO query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        int offset;
        try
        {
            offset = checked((page - 1) * pageSize);
        }
        catch (OverflowException)
        {
            throw new ArgumentException("Page is too large.");
        }
        var owned = (query.Owned ?? "all").Trim().ToLowerInvariant();
        var sort = (query.Sort ?? "name").Trim().ToLowerInvariant();
        if (owned is not ("all" or "owned" or "unowned"))
            throw new ArgumentException("Owned must be all, owned, or unowned.");
        if (sort is not ("name" or "era" or "owned"))
            throw new ArgumentException("Sort must be name, era, or owned.");

        RelicEra? era = null;
        if (!string.IsNullOrWhiteSpace(query.Era))
            era = ParseEnum<RelicEra>(query.Era, "era");
        var relics = _dbContext.relics.AsNoTracking().AsQueryable();
        if (era.HasValue)
            relics = relics.Where(relic => relic.Era == era.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerms = query.Search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(term => term.ToLowerInvariant())
                .Distinct()
                .ToList();
            relics = relics.Where(relic => searchTerms.All(searchTerm => relic.Name.ToLower().Contains(searchTerm))
                || relic.Rewards.Any(reward => reward.Reward.name != null
                    && searchTerms.All(searchTerm => reward.Reward.name.ToLower().Contains(searchTerm))));
        }
        if (owned == "owned")
            relics = relics.Where(relic => relic.Variants.Any(variant => variant.Item.player_items
                .Any(item => item.player_id == playerId && item.item_count > 0)));
        else if (owned == "unowned")
            relics = relics.Where(relic => !relic.Variants.Any(variant => variant.Item.player_items
                .Any(item => item.player_id == playerId && item.item_count > 0)));

        var rows = relics.Select(relic => new RelicReadRow
        {
            Id = relic.Id,
            Name = relic.Name,
            Era = relic.Era,
            EraOrder = relic.Era == RelicEra.Lith ? 0
                : relic.Era == RelicEra.Meso ? 1
                : relic.Era == RelicEra.Neo ? 2
                : 3,
            TotalOwned = relic.Variants.SelectMany(variant => variant.Item.player_items)
                .Where(item => item.player_id == playerId)
                .Sum(item => (int?)item.item_count) ?? 0
        });
        rows = sort switch
        {
            "era" => rows.OrderBy(row => row.EraOrder).ThenBy(row => row.Name).ThenBy(row => row.Id),
            "owned" => rows.OrderByDescending(row => row.TotalOwned).ThenBy(row => row.Name).ThenBy(row => row.Id),
            _ => rows.OrderBy(row => row.Name).ThenBy(row => row.Id)
        };

        var totalCount = await rows.CountAsync();
        var selected = await rows.Skip(offset).Take(pageSize).ToListAsync();
        var items = selected.Select(row => new RelicDTO
        {
            Id = row.Id,
            Name = row.Name,
            Era = row.Era.ToString(),
            TotalOwned = row.TotalOwned
        }).ToList();
        await PopulateChildrenAsync(items, playerId);

        return new RelicPageDTO
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<RelicDTO?> GetRelicAsync(int playerId, int id)
    {
        var item = await _dbContext.relics.AsNoTracking()
            .Where(relic => relic.Id == id)
            .Select(relic => new RelicDTO
            {
                Id = relic.Id,
                Name = relic.Name,
                Era = relic.Era.ToString(),
                TotalOwned = relic.Variants.SelectMany(variant => variant.Item.player_items)
                    .Where(playerItem => playerItem.player_id == playerId)
                    .Sum(playerItem => (int?)playerItem.item_count) ?? 0
            }).SingleOrDefaultAsync();
        if (item != null)
            await PopulateChildrenAsync([item], playerId);
        return item;
    }

    private async Task PopulateChildrenAsync(List<RelicDTO> relics, int playerId)
    {
        var ids = relics.Select(relic => relic.Id).ToList();
        if (ids.Count == 0) return;

        var variants = await _dbContext.relic_variants.AsNoTracking()
            .Where(variant => ids.Contains(variant.RelicId))
            .Select(variant => new
            {
                variant.RelicId,
                Dto = new RelicVariantDTO
                {
                    UniqueName = variant.UniqueName,
                    Name = variant.Item.name ?? variant.UniqueName,
                    Refinement = variant.Refinement.ToString(),
                    Quantity = variant.Item.player_items.Where(item => item.player_id == playerId)
                        .Sum(item => (int?)item.item_count) ?? 0
                }
            }).ToListAsync();
        var rewards = await _dbContext.relic_rewards.AsNoTracking()
            .Where(reward => ids.Contains(reward.RelicId))
            .Select(reward => new
            {
                reward.RelicId,
                Dto = new RelicRewardDTO
                {
                    ItemName = reward.Reward.name,
                    UniqueName = reward.RewardUniqueName,
                    Rarity = reward.Rarity.ToString(),
                    ItemCount = reward.ItemCount
                }
            }).ToListAsync();

        foreach (var relic in relics)
        {
            relic.Variants = variants.Where(value => value.RelicId == relic.Id).Select(value => value.Dto)
                .OrderBy(value => ParseEnum<RelicRefinement>(value.Refinement, "refinement"))
                .ThenBy(value => value.UniqueName).ToList();
            relic.Rewards = rewards.Where(value => value.RelicId == relic.Id).Select(value => value.Dto)
                .OrderBy(value => ParseEnum<RelicRewardRarity>(value.Rarity, "rarity"))
                .ThenBy(value => value.ItemName).ThenBy(value => value.UniqueName).ToList();
        }
    }

    private static T ParseEnum<T>(string? value, string field) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value) || !Enum.TryParse<T>(value.Trim(), true, out var parsed) || !Enum.IsDefined(parsed))
            throw new ArgumentException($"Invalid {field}.");
        return parsed;
    }

    private class RelicReadRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public RelicEra Era { get; set; }
        public int EraOrder { get; set; }
        public int TotalOwned { get; set; }
    }
}
