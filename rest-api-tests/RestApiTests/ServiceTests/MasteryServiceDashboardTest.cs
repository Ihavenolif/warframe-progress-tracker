using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class MasteryServiceDashboardTest
{
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

    private static MasteryService CreateService(WarframeTrackerDbContextTest context) => new(
        context,
        null!,
        null!,
        NullLogger<MasteryService>.Instance);
}
