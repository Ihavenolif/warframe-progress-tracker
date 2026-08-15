using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.DTOs.RelicRecommendations;
using rest_api.Models;

namespace rest_api.Services;

public interface IRelicRecommendationService
{
    Task<RelicRecommendationResponseDto> GetRecommendationsAsync(IReadOnlyCollection<int> playerIds, int limit);
}

public class RelicRecommendationService : IRelicRecommendationService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public RelicRecommendationService(WarframeTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RelicRecommendationResponseDto> GetRecommendationsAsync(
        IReadOnlyCollection<int> playerIds,
        int limit)
    {
        var ids = ValidateRequest(playerIds, limit);
        var players = await LoadPlayersAsync(ids);
        var playerState = await LoadPlayerStateAsync(ids);
        EnsureProfilesExist(players, playerState);

        var ownershipRows = await LoadOwnershipAsync(ids);
        if (ownershipRows.Count == 0)
            return new RelicRecommendationResponseDto { Players = players };

        var relicIds = ownershipRows.Select(row => row.RelicId).Distinct().ToList();
        var rewardRows = await LoadRewardsAsync(relicIds);
        var recipes = await LoadRecipesAsync();
        var needsByPlayer = CalculateNeeds(players, playerState, rewardRows, recipes);
        var relicRows = await LoadRelicsAsync(relicIds);

        return new RelicRecommendationResponseDto
        {
            Players = players,
            Recommendations = BuildRecommendations(
                relicRows, rewardRows, ownershipRows, players, needsByPlayer, limit)
        };
    }

    private static List<int> ValidateRequest(IReadOnlyCollection<int> playerIds, int limit)
    {
        var ids = playerIds.Distinct().ToList();
        if (ids.Count is < 1 or > 4 || ids.Count != playerIds.Count)
            throw new ArgumentException("Select 1-4 distinct players.");
        if (limit is < 1 or > 50)
            throw new ArgumentException("Limit must be between 1 and 50.");
        return ids;
    }

    private async Task<List<RelicRecommendationPlayerDto>> LoadPlayersAsync(List<int> ids)
    {
        var players = await _dbContext.players.AsNoTracking()
            .Where(player => ids.Contains(player.id))
            .Select(player => new RelicRecommendationPlayerDto { Id = player.id, Name = player.username })
            .OrderBy(player => player.Name)
            .ToListAsync();
        if (players.Count != ids.Count)
            throw new ArgumentException("One or more selected players do not exist.");
        return players;
    }

    private async Task<PlayerState> LoadPlayerStateAsync(List<int> ids)
    {
        var inventoryRows = await _dbContext.player_items.AsNoTracking()
            .Where(item => ids.Contains(item.player_id) && item.item_count > 0)
            .Select(item => new InventoryRow(item.player_id, item.unique_name, item.item_count))
            .ToListAsync();
        var masteryRows = await _dbContext.player_items_masteries.AsNoTracking()
            .Where(item => ids.Contains(item.player_id))
            .Select(item => new MasteryRow(item.player_id, item.unique_name))
            .ToListAsync();
        return new PlayerState(inventoryRows, masteryRows);
    }

    private static void EnsureProfilesExist(
        List<RelicRecommendationPlayerDto> players,
        PlayerState playerState)
    {
        var playersWithProfiles = playerState.Inventory.Select(row => row.PlayerId)
            .Concat(playerState.Mastery.Select(row => row.PlayerId)).ToHashSet();
        var missingProfiles = players.Where(player => !playersWithProfiles.Contains(player.Id))
            .Select(player => player.Name).ToList();
        if (missingProfiles.Count > 0)
            throw new MissingProfileDataException(
                $"Import profile data for: {string.Join(", ", missingProfiles)}.");
    }

    private Task<List<OwnershipRow>> LoadOwnershipAsync(List<int> ids)
    {
        return (
            from variant in _dbContext.relic_variants.AsNoTracking()
            from playerItem in variant.Item.player_items
            where ids.Contains(playerItem.player_id) && playerItem.item_count > 0
            select new OwnershipRow(
                variant.RelicId,
                playerItem.player_id,
                variant.Refinement,
                playerItem.item_count)).ToListAsync();
    }

    private Task<List<RewardRow>> LoadRewardsAsync(List<int> relicIds)
    {
        return _dbContext.relic_rewards.AsNoTracking()
            .Where(reward => relicIds.Contains(reward.RelicId))
            .Select(reward => new RewardRow(
                reward.RelicId,
                reward.RewardUniqueName,
                reward.Reward.name,
                reward.Rarity))
            .ToListAsync();
    }

    private async Task<List<RecommendationRecipe>> LoadRecipesAsync()
    {
        var recipeRows = await _dbContext.recipes.AsNoTracking()
            .Select(recipe => new RecipeRow(
                recipe.unique_name,
                recipe.result_item,
                recipe.result_itemNavigation.name,
                recipe.result_itemNavigation.xp_required,
                recipe.recipe_ingredients.Select(ingredient =>
                    new IngredientRow(ingredient.item_ingredient, ingredient.ingredient_count)).ToList()))
            .ToListAsync();
        return recipeRows.Select(row => new RecommendationRecipe(
            row.BlueprintUniqueName,
            row.ResultUniqueName,
            row.ResultName ?? row.ResultUniqueName,
            IsPrimeName(row.ResultName),
            row.XpRequired.HasValue,
            row.Ingredients.Select(ingredient =>
                new RecommendationIngredient(ingredient.UniqueName, ingredient.Count)).ToList()))
            .ToList();
    }

    private static Dictionary<int, Dictionary<string, RecommendationNeed>> CalculateNeeds(
        List<RelicRecommendationPlayerDto> players,
        PlayerState playerState,
        List<RewardRow> rewardRows,
        List<RecommendationRecipe> recipes)
    {
        var rewardNames = rewardRows.Select(reward => reward.UniqueName).ToHashSet();
        var inventoryByPlayer = playerState.Inventory.ToLookup(row => row.PlayerId);
        var masteryByPlayer = playerState.Mastery.ToLookup(row => row.PlayerId);
        var needsByPlayer = new Dictionary<int, Dictionary<string, RecommendationNeed>>();
        foreach (var player in players)
        {
            var inventory = inventoryByPlayer[player.Id]
                .ToDictionary(row => row.UniqueName, row => row.Count);
            var mastered = masteryByPlayer[player.Id]
                .Select(row => row.UniqueName).ToHashSet();
            needsByPlayer[player.Id] = RelicNeedCalculator.Calculate(rewardNames, recipes, inventory, mastered);
        }
        return needsByPlayer;
    }

    private Task<List<RelicRow>> LoadRelicsAsync(List<int> relicIds)
    {
        return _dbContext.relics.AsNoTracking()
            .Where(relic => relicIds.Contains(relic.Id))
            .Select(relic => new RelicRow(relic.Id, relic.Name, relic.Era))
            .ToListAsync();
    }

    private static List<RelicRecommendationDto> BuildRecommendations(
        List<RelicRow> relicRows,
        List<RewardRow> rewardRows,
        List<OwnershipRow> ownershipRows,
        List<RelicRecommendationPlayerDto> players,
        Dictionary<int, Dictionary<string, RecommendationNeed>> needsByPlayer,
        int limit)
    {
        var playerNames = players.ToDictionary(player => player.Id, player => player.Name);
        var rewardsByRelic = rewardRows.ToLookup(row => row.RelicId);
        var ownershipByRelic = ownershipRows.ToLookup(row => row.RelicId);

        var recommendations = new List<RelicRecommendationDto>();
        foreach (var relic in relicRows)
        {
            var usefulRewards = BuildUsefulRewards(rewardsByRelic[relic.Id], players, needsByPlayer);
            if (usefulRewards.Count == 0) continue;
            recommendations.Add(new RelicRecommendationDto
            {
                RelicId = relic.Id,
                Name = relic.Name,
                Era = relic.Era.ToString(),
                Score = usefulRewards.Sum(reward => reward.NeedPoints),
                BenefitingPlayerCount = usefulRewards.SelectMany(reward => reward.Players)
                    .Select(need => need.PlayerId).Distinct().Count(),
                Owners = BuildOwners(ownershipByRelic[relic.Id], playerNames),
                UsefulRewards = usefulRewards.OrderBy(reward => Enum.Parse<RelicRewardRarity>(reward.Rarity))
                    .ThenBy(reward => reward.ItemName).ThenBy(reward => reward.ItemUniqueName).ToList()
            });
        }

        return recommendations.OrderByDescending(relic => relic.Score)
            .ThenByDescending(relic => relic.BenefitingPlayerCount)
            .ThenByDescending(relic => relic.UsefulRewards.Count)
            .ThenBy(relic => relic.Name)
            .ThenBy(relic => relic.RelicId)
            .Take(limit).ToList();
    }

    private static List<RelicRecommendationRewardDto> BuildUsefulRewards(
        IEnumerable<RewardRow> rewards,
        List<RelicRecommendationPlayerDto> players,
        Dictionary<int, Dictionary<string, RecommendationNeed>> needsByPlayer)
    {
        var usefulRewards = new List<RelicRecommendationRewardDto>();
        foreach (var reward in rewards)
        {
            var playerNeeds = BuildPlayerNeeds(reward.UniqueName, players, needsByPlayer);
            if (playerNeeds.Count == 0) continue;
            usefulRewards.Add(new RelicRecommendationRewardDto
            {
                ItemName = reward.Name,
                ItemUniqueName = reward.UniqueName,
                Rarity = reward.Rarity.ToString(),
                NeedPoints = playerNeeds.Sum(need => need.MissingCount),
                Players = playerNeeds
            });
        }
        return usefulRewards;
    }

    private static List<RelicRecommendationNeedDto> BuildPlayerNeeds(
        string rewardUniqueName,
        List<RelicRecommendationPlayerDto> players,
        Dictionary<int, Dictionary<string, RecommendationNeed>> needsByPlayer)
    {
        return players.Select(player =>
        {
            needsByPlayer[player.Id].TryGetValue(rewardUniqueName, out var need);
            return (Player: player, Need: need);
        }).Where(value => value.Need != null).Select(value => new RelicRecommendationNeedDto
        {
            PlayerId = value.Player.Id,
            PlayerName = value.Player.Name,
            MissingCount = value.Need!.Count,
            RequiredFor = value.Need.RequiredFor.OrderBy(name => name).ToList()
        }).ToList();
    }

    private static List<RelicRecommendationOwnerDto> BuildOwners(
        IEnumerable<OwnershipRow> ownershipRows,
        IReadOnlyDictionary<int, string> playerNames)
    {
        return ownershipRows.GroupBy(row => row.PlayerId).Select(group => new RelicRecommendationOwnerDto
        {
            PlayerId = group.Key,
            PlayerName = playerNames[group.Key],
            TotalCount = group.Sum(row => row.Count),
            Refinements = new RelicRefinementCountsDto
            {
                Intact = group.Where(row => row.Refinement == RelicRefinement.Intact).Sum(row => row.Count),
                Exceptional = group.Where(row => row.Refinement == RelicRefinement.Exceptional).Sum(row => row.Count),
                Flawless = group.Where(row => row.Refinement == RelicRefinement.Flawless).Sum(row => row.Count),
                Radiant = group.Where(row => row.Refinement == RelicRefinement.Radiant).Sum(row => row.Count)
            }
        }).OrderBy(owner => owner.PlayerName).ToList();
    }

    private sealed record PlayerState(List<InventoryRow> Inventory, List<MasteryRow> Mastery);
    private sealed record InventoryRow(int PlayerId, string UniqueName, int Count);
    private sealed record MasteryRow(int PlayerId, string UniqueName);
    private sealed record IngredientRow(string UniqueName, int Count);
    private sealed record RecipeRow(
        string BlueprintUniqueName,
        string ResultUniqueName,
        string? ResultName,
        int? XpRequired,
        List<IngredientRow> Ingredients);
    private sealed record RewardRow(int RelicId, string UniqueName, string? Name, RelicRewardRarity Rarity);
    private sealed record RelicRow(int Id, string Name, RelicEra Era);
    private sealed record OwnershipRow(int RelicId, int PlayerId, RelicRefinement Refinement, int Count);

    private static bool IsPrimeName(string? name)
    {
        return name?.StartsWith("Prime ", StringComparison.OrdinalIgnoreCase) == true
            || name?.Contains(" Prime", StringComparison.OrdinalIgnoreCase) == true;
    }
}

public class MissingProfileDataException : InvalidOperationException
{
    public MissingProfileDataException(string message) : base(message)
    {
    }
}

internal sealed record RecommendationIngredient(string UniqueName, int Count);

internal sealed record RecommendationRecipe(
    string BlueprintUniqueName,
    string ResultUniqueName,
    string ResultName,
    bool IsPrime,
    bool IsGear,
    IReadOnlyCollection<RecommendationIngredient> Ingredients);

internal sealed class RecommendationNeed
{
    public int Count { get; set; }
    public HashSet<string> RequiredFor { get; } = new();
}

internal static class RelicNeedCalculator
{
    public static Dictionary<string, RecommendationNeed> Calculate(
        IReadOnlySet<string> rewardUniqueNames,
        IReadOnlyCollection<RecommendationRecipe> recipes,
        IReadOnlyDictionary<string, int> inventory,
        IReadOnlySet<string> masteredItems)
    {
        var recipeByResult = recipes.Where(recipe => recipe.IsPrime)
            .GroupBy(recipe => recipe.ResultUniqueName)
            .ToDictionary(group => group.Key, group => group.First());
        var needs = new Dictionary<string, RecommendationNeed>();

        // Each possible gear build gets its own inventory allocation; sharing is prevented within that build.
        foreach (var target in recipes.Where(recipe => recipe.IsPrime && recipe.IsGear))
        {
            if (masteredItems.Contains(target.ResultUniqueName)) continue;
            var available = inventory.ToDictionary(pair => pair.Key, pair => pair.Value);
            if (Consume(available, target.ResultUniqueName, 1) == 1) continue;
            RequireRecipe(target, 1, target.ResultName, available, rewardUniqueNames,
                recipeByResult, needs, new HashSet<string>());
        }
        return needs;
    }

    private static void RequireRecipe(
        RecommendationRecipe recipe,
        int count,
        string targetName,
        Dictionary<string, int> available,
        IReadOnlySet<string> rewards,
        IReadOnlyDictionary<string, RecommendationRecipe> recipeByResult,
        Dictionary<string, RecommendationNeed> needs,
        HashSet<string> path)
    {
        if (!path.Add(recipe.BlueprintUniqueName)) return;
        RequireReward(recipe.BlueprintUniqueName, count, targetName, available, rewards, needs);
        foreach (var ingredient in recipe.Ingredients)
        {
            var required = checked(ingredient.Count * count);
            var remaining = required - Consume(available, ingredient.UniqueName, required);
            if (remaining == 0) continue;
            if (recipeByResult.TryGetValue(ingredient.UniqueName, out var ingredientRecipe))
                RequireRecipe(ingredientRecipe, remaining, targetName, available, rewards, recipeByResult, needs, path);
            else
                AddNeed(ingredient.UniqueName, remaining, targetName, rewards, needs);
        }
        path.Remove(recipe.BlueprintUniqueName);
    }

    private static void RequireReward(
        string uniqueName,
        int count,
        string targetName,
        Dictionary<string, int> available,
        IReadOnlySet<string> rewards,
        Dictionary<string, RecommendationNeed> needs)
    {
        var remaining = count - Consume(available, uniqueName, count);
        AddNeed(uniqueName, remaining, targetName, rewards, needs);
    }

    private static void AddNeed(
        string uniqueName,
        int count,
        string targetName,
        IReadOnlySet<string> rewards,
        Dictionary<string, RecommendationNeed> needs)
    {
        if (count <= 0 || !rewards.Contains(uniqueName)) return;
        if (!needs.TryGetValue(uniqueName, out var need))
        {
            need = new RecommendationNeed();
            needs[uniqueName] = need;
        }
        need.Count += count;
        need.RequiredFor.Add(targetName);
    }

    private static int Consume(Dictionary<string, int> available, string uniqueName, int count)
    {
        if (!available.TryGetValue(uniqueName, out var owned) || owned <= 0) return 0;
        var used = Math.Min(owned, count);
        available[uniqueName] = owned - used;
        return used;
    }
}
