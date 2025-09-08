using Microsoft.EntityFrameworkCore;
using rest_api.Data;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class ClanServiceTest
{
    private readonly WarframeTrackerDbContext dbContext;
    private readonly ClanService clanService;

    public ClanServiceTest()
    {
        dbContext = new WarframeTrackerDbContextTest();

        clanService = new ClanService(dbContext);
    }

    [Fact]
    public async Task TestCreateClan()
    {
        var player = new Player
        {
            username = "TestPlayer",
            mastery_rank = 10
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "TestClan";
        var clan = await clanService.CreateClanAsync(player, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(player.id, clan.leader_id);
        Assert.True(clan.players.Contains(player));
        Assert.Single(clan.players);
    }

    [Fact]
    public async Task TestCreateClanConflictingName()
    {
        var player = new Player
        {
            username = "TestPlayer2",
            mastery_rank = 15
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "UniqueClan";
        var clan1 = await clanService.CreateClanAsync(player, clanName);
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var clan2 = await clanService.CreateClanAsync(player, clanName);
        });
        Assert.Equal("Clan with the same name already exists.", exception.Message);
        Assert.Equal(1, dbContext.clans.Count());
        Assert.Equal(clanName, dbContext.clans.First().name);
        Assert.Equal(player.id, dbContext.clans.First().leader_id);
        Assert.Single(dbContext.clans.First().players);
    }

    [Fact]
    public async Task TestUpdateClan()
    {
        var player = new Player
        {
            username = "TestPlayer3",
            mastery_rank = 20
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName = "UpdateClan";
        var clan = await clanService.CreateClanAsync(player, clanName);

        Assert.NotNull(clan);
        Assert.Equal(clanName, clan.name);
        Assert.Equal(player.id, clan.leader_id);
        Assert.True(clan.players.Contains(player));
        Assert.Single(clan.players);

        int clanId = clan.id;

        var newClanName = "UpdatedClanName";
        clan.name = newClanName;
        await clanService.UpdateClanAsync(clan);

        var updatedClan = dbContext.clans.FirstOrDefault(c => c.id == clanId);
        Assert.NotNull(updatedClan);
        Assert.Equal(newClanName, updatedClan.name);
    }

    [Fact]
    public async Task TestUpdateClanConflictingName()
    {
        var player = new Player
        {
            username = "TestPlayer4",
            mastery_rank = 25
        };

        dbContext.players.Add(player);
        dbContext.SaveChanges();

        var clanName1 = "FirstClan";
        var clan1 = await clanService.CreateClanAsync(player, clanName1);

        var clanName2 = "SecondClan";
        var clan2 = await clanService.CreateClanAsync(player, clanName2);

        Assert.NotNull(clan1);
        Assert.NotNull(clan2);
        Assert.Equal(clanName1, clan1.name);
        Assert.Equal(clanName2, clan2.name);

        clan2.name = clanName1; // Attempt to rename second clan to the name of the first clan

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await clanService.UpdateClanAsync(clan2);
        });
        Assert.Equal("Clan with the same name already exists.", exception.Message);

        await dbContext.Entry(clan2).ReloadAsync();

        var unchangedClan2 = await dbContext.clans.FirstOrDefaultAsync(c => c.id == clan2.id);
        Assert.NotNull(unchangedClan2);
        Assert.Equal(clanName2, unchangedClan2.name); // Name should remain unchanged
    }
}