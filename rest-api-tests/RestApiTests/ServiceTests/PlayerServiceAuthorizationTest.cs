using rest_api.Data;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ServiceTests;

public class PlayerServiceAuthorizationTest
{
    private readonly WarframeTrackerDbContext dbContext;
    private readonly PlayerService playerService;

    public PlayerServiceAuthorizationTest()
    {
        dbContext = new WarframeTrackerDbContextTest();
        playerService = new PlayerService(dbContext);
    }

    [Fact]
    public async Task NonAdminCanAccessSelfAndClanMembersOnly()
    {
        var viewer = new Player("Viewer");
        var clanMember = new Player("ClanMember");
        var unrelated = new Player("Unrelated");
        var clan = new Clan
        {
            name = "TestClan",
            leader = viewer,
            players = { viewer, clanMember }
        };
        dbContext.AddRange(viewer, clanMember, unrelated, clan);
        await dbContext.SaveChangesAsync();

        var accessiblePlayers = await playerService.GetAccessiblePlayersAsync(viewer.id, false);

        Assert.Equal(["ClanMember", "Viewer"], accessiblePlayers.Select(player => player.username).Order());
        Assert.NotNull(await playerService.FindAccessiblePlayerByUsernameAsync(viewer.username, viewer.id, false));
        Assert.NotNull(await playerService.FindAccessiblePlayerByUsernameAsync(clanMember.username, viewer.id, false));
        Assert.Null(await playerService.FindAccessiblePlayerByUsernameAsync(unrelated.username, viewer.id, false));
        Assert.Null(await playerService.FindAccessiblePlayerByUsernameAsync("Missing", viewer.id, false));
    }

    [Fact]
    public async Task AdminCanAccessAllPlayersWithoutLinkedPlayer()
    {
        dbContext.AddRange(new Player("First"), new Player("Second"));
        await dbContext.SaveChangesAsync();

        var accessiblePlayers = await playerService.GetAccessiblePlayersAsync(null, true);

        Assert.Equal(["First", "Second"], accessiblePlayers.Select(player => player.username).Order());
        Assert.NotNull(await playerService.FindAccessiblePlayerByUsernameAsync("Second", null, true));
    }

    [Fact]
    public async Task UnlinkedNonAdminCannotAccessPlayers()
    {
        dbContext.Add(new Player("Target"));
        await dbContext.SaveChangesAsync();

        Assert.Empty(await playerService.GetAccessiblePlayersAsync(null, false));
        Assert.Null(await playerService.FindAccessiblePlayerByUsernameAsync("Target", null, false));
    }
}
