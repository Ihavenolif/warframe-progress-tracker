using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class MasteryServiceDashboardTest
{
    [Fact]
    public async Task SummaryAggregatesCurrentPlayerProgressWithoutIncludingIneligibleItems()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new()
        {
            username = "summary-player",
            mastery_rank = 33,
            TotalMasteryXp = 123456
        };
        Player otherPlayer = new() { username = "other-summary-player" };
        Item mastered = Item("/Items/Mastered", "Weapon", 450000);
        Item started = Item("/Items/Started", "Weapon", 450000);
        Item craftReady = Item("/Items/CraftReady", "Warframe", 900000);
        Item blueprint = Item("/Items/CraftReadyBlueprint", "Recipe");
        Item component = Item("/Items/Component", "Resource");
        Item ineligible = Item("/Items/Ineligible", "Weapon");
        context.AddRange(player, otherPlayer, mastered, started, craftReady, blueprint, component, ineligible);
        context.recipes.Add(new Recipe
        {
            unique_name = blueprint.unique_name,
            result_item = craftReady.unique_name,
            recipe_ingredients =
            {
                new Recipe_ingredient
                {
                    item_ingredient = component.unique_name,
                    ingredient_count = 2
                }
            }
        });
        context.missions.AddRange(
            new Mission { UniqueName = "/Missions/One", MasteryXp = 1000 },
            new Mission { UniqueName = "/Missions/Two", MasteryXp = 1000 },
            new Mission { UniqueName = "/Missions/Ineligible", MasteryXp = 0 });
        await context.SaveChangesAsync();

        context.player_items_masteries.AddRange(
            new Player_items_mastery
            {
                player_id = player.id,
                unique_name = mastered.unique_name,
                xp_gained = mastered.xp_required!.Value
            },
            new Player_items_mastery
            {
                player_id = player.id,
                unique_name = started.unique_name,
                xp_gained = 0
            },
            new Player_items_mastery
            {
                player_id = otherPlayer.id,
                unique_name = craftReady.unique_name,
                xp_gained = craftReady.xp_required!.Value
            });
        context.player_items.AddRange(
            new Player_item { player_id = player.id, unique_name = blueprint.unique_name, item_count = 1 },
            new Player_item { player_id = player.id, unique_name = component.unique_name, item_count = 2 });
        context.mission_completions.AddRange(
            new MissionCompletion
            {
                PlayerId = player.id,
                UniqueName = "/Missions/One",
                CompletionCount = 1,
                SPComplete = true
            },
            new MissionCompletion
            {
                PlayerId = player.id,
                UniqueName = "/Missions/Ineligible",
                CompletionCount = 1,
                SPComplete = true
            });
        DateTime today = DateTime.UtcNow.Date;
        context.mastery_progress_entries.AddRange(
            ProgressEntry(player, today.AddHours(1), 100),
            ProgressEntry(player, today.AddDays(-10), 300),
            ProgressEntry(player, today.AddDays(-30), 900),
            ProgressEntry(otherPlayer, today, 800));
        context.mastery_import_receipts.AddRange(
            new MasteryImportReceipt
            {
                PlayerId = player.id,
                ImportedAt = today.AddDays(-2),
                ResultingMasteryRank = 32
            },
            new MasteryImportReceipt
            {
                PlayerId = player.id,
                ImportedAt = today.AddDays(-1),
                ResultingMasteryRank = 33,
                ResultingTotalMasteryXp = player.TotalMasteryXp
            });
        await context.SaveChangesAsync();

        var summary = await CreateService(context).GetDashboardSummaryAsync(player);

        Assert.Equal(33, summary.MasteryRank);
        Assert.Equal(123456, summary.TotalMasteryXp);
        Assert.Equal(33, summary.LatestImport?.ResultingMasteryRank);
        Assert.Equal(100, summary.MasteryXpGained7Days);
        Assert.Equal(400, summary.MasteryXpGained30Days);
        Assert.Equal(3, summary.Items.Total);
        Assert.Equal(1, summary.Items.Mastered);
        Assert.Equal(1, summary.Items.Started);
        Assert.Equal(1, summary.Items.Unowned);
        Assert.Equal(1, summary.Items.CraftReady);
        Assert.Collection(summary.Categories,
            category =>
            {
                Assert.Equal("Warframe", category.Category);
                Assert.Equal(0, category.Mastered);
                Assert.Equal(1, category.Total);
            },
            category =>
            {
                Assert.Equal("Weapon", category.Category);
                Assert.Equal(1, category.Mastered);
                Assert.Equal(2, category.Total);
            });
        Assert.Equal(1, summary.Missions.NormalCompleted);
        Assert.Equal(2, summary.Missions.NormalTotal);
        Assert.Equal(1, summary.Missions.SteelPathCompleted);
        Assert.Equal(2, summary.Missions.SteelPathTotal);
    }

    [Fact]
    public async Task SummaryReturnsZeroStateForNewProfile()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "new-profile" };
        context.players.Add(player);
        await context.SaveChangesAsync();

        var summary = await CreateService(context).GetDashboardSummaryAsync(player);

        Assert.Equal(0, summary.MasteryRank);
        Assert.Equal(0, summary.TotalMasteryXp);
        Assert.Null(summary.LatestImport);
        Assert.Equal(0, summary.MasteryXpGained7Days);
        Assert.Equal(0, summary.MasteryXpGained30Days);
        Assert.Equal(0, summary.Items.Total);
        Assert.Empty(summary.Categories);
        Assert.Equal(0, summary.Missions.NormalTotal);
        Assert.Equal(0, summary.Missions.SteelPathTotal);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(31)]
    public async Task SummaryReturnsCombinedRank(int combinedRank)
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = $"rank-{combinedRank}", mastery_rank = combinedRank };
        context.players.Add(player);
        await context.SaveChangesAsync();

        var summary = await CreateService(context).GetDashboardSummaryAsync(player);

        Assert.Equal(combinedRank, summary.MasteryRank);
    }

    [Fact]
    public async Task LatestEntriesMapItemsAndMissionsInNewestFirstOrder()
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "dashboard-player" };
        Player otherPlayer = new() { username = "other-player" };
        Item item = new()
        {
            unique_name = "/Items/DashboardWeapon",
            name = "Catalog Name",
            type = "Weapon",
            item_class = "LongGuns",
            xp_required = 450000
        };
        Mission mission = new()
        {
            UniqueName = "/Missions/DashboardMission",
            Name = "Catalog Mission",
            Planet = "Earth",
            MasteryXp = 1000
        };
        context.AddRange(player, otherPlayer, item, mission);
        await context.SaveChangesAsync();

        DateTime start = DateTime.UtcNow.Date.AddDays(-2);
        for (int index = 0; index < 12; index++)
        {
            context.mastery_progress_entries.Add(ProgressEntry(player, start.AddHours(index), 1000 + index));
        }
        context.mastery_progress_entries.AddRange(
            ProgressEntry(otherPlayer, start.AddDays(1), 9000),
            new MasteryProgressEntry
            {
                Player = player,
                CreatedAt = start.AddDays(1),
                PreviousTotalMasteryXp = 0,
                CurrentTotalMasteryXp = 500,
                MasteryXpGained = 500
            });
        MasteryProgressEntry newest = ProgressEntry(player, start.AddDays(1), 5000);
        newest.LeveledItems.Add(new MasteryProgressItem
        {
            Item = item,
            Name = "Imported Name",
            PreviousXp = 500,
            CurrentXp = 4500,
            MasteryXpGained = 200
        });
        newest.Missions.Add(new MasteryProgressMission
        {
            Mission = mission,
            UniqueName = mission.UniqueName,
            Name = mission.Name,
            Planet = mission.Planet,
            PreviousCompletionCount = 0,
            CurrentCompletionCount = 1,
            PreviousSPComplete = false,
            CurrentSPComplete = true,
            MasteryXpGained = 2000
        });
        context.mastery_progress_entries.Add(newest);
        await context.SaveChangesAsync();

        MasteryService service = CreateService(context);
        var entries = await service.GetLatestProgressEntriesAsync(player);

        Assert.Equal(10, entries.Count);
        Assert.Equal(newest.Id, entries[0].id);
        Assert.Equal(5000, entries[0].masteryXpGained);
        var mappedItem = Assert.Single(entries[0].leveledItems);
        Assert.Equal(item.unique_name, mappedItem.uniqueName);
        Assert.Equal("Imported Name", mappedItem.name);
        Assert.Equal(1, mappedItem.previousRank);
        Assert.Equal(3, mappedItem.currentRank);
        var mappedMission = Assert.Single(entries[0].missions);
        Assert.True(mappedMission.completed);
        Assert.True(mappedMission.steelPathCompleted);
        Assert.Equal(2000, mappedMission.masteryXpGained);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    public async Task DailyProgressGroupsUtcDaysAndZeroFillsRange(int days)
    {
        await using var context = new WarframeTrackerDbContextTest();
        Player player = new() { username = "daily-player" };
        Player otherPlayer = new() { username = "other-daily-player" };
        context.AddRange(player, otherPlayer);
        await context.SaveChangesAsync();
        DateTime today = DateTime.UtcNow.Date;
        context.mastery_progress_entries.AddRange(
            ProgressEntry(player, today.AddHours(1), 100),
            ProgressEntry(player, today.AddHours(2), 250),
            ProgressEntry(player, today.AddDays(-1).AddHours(3), 75),
            ProgressEntry(player, today.AddDays(-days), 999),
            ProgressEntry(otherPlayer, today.AddHours(4), 888),
            new MasteryProgressEntry
            {
                Player = player,
                CreatedAt = today,
                PreviousTotalMasteryXp = 500,
                CurrentTotalMasteryXp = 400,
                MasteryXpGained = -100
            });
        await context.SaveChangesAsync();

        var result = await CreateService(context).GetDailyProgressAsync(player, days);

        Assert.Equal(days, result.Count);
        Assert.Equal(today.AddDays(-(days - 1)), result[0].date);
        Assert.Equal(today, result[^1].date);
        Assert.Equal(350, result[^1].masteryXpGained);
        Assert.Equal(75, result[^2].masteryXpGained);
        Assert.All(result.SkipLast(2), day => Assert.Equal(0, day.masteryXpGained));
    }

    private static MasteryProgressEntry ProgressEntry(Player player, DateTime createdAt, int gained) => new()
    {
        Player = player,
        CreatedAt = createdAt,
        PreviousTotalMasteryXp = 1000,
        CurrentTotalMasteryXp = 1000 + gained,
        MasteryXpGained = gained
    };

    private static Item Item(string uniqueName, string itemClass, int? xpRequired = null) => new()
    {
        unique_name = uniqueName,
        name = uniqueName,
        type = "Test",
        item_class = itemClass,
        xp_required = xpRequired
    };

    private static MasteryService CreateService(WarframeTrackerDbContextTest context) => new(
        context,
        null!,
        null!,
        NullLogger<MasteryService>.Instance);
}
