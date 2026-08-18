using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rest_api.Controllers;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ControllerTests;

public class PlayerControllerAuthorizationTest
{
    [Fact]
    public async Task NonAdminListingContainsSelfAndClanMembersOnly()
    {
        var fixture = await CreateFixtureAsync();
        var controller = fixture.CreateController();

        var result = await controller.GetPlayers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var players = Assert.IsType<List<Player>>(ok.Value);
        Assert.Equal(["ClanMember", "Viewer"], players.Select(player => player.username).Order());
    }

    [Fact]
    public async Task AdminListingContainsAllPlayers()
    {
        var fixture = await CreateFixtureAsync(isAdmin: true);
        var controller = fixture.CreateController();

        var result = await controller.GetPlayers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var players = Assert.IsType<List<Player>>(ok.Value);
        Assert.Equal(["ClanMember", "Unrelated", "Viewer"], players.Select(player => player.username).Order());
    }

    private static async Task<TestFixture> CreateFixtureAsync(bool isAdmin = false)
    {
        var dbContext = new WarframeTrackerDbContextTest();
        var viewer = new Player("Viewer");
        var clanMember = new Player("ClanMember");
        var unrelated = new Player("Unrelated");
        var clan = new Clan
        {
            name = "TestClan",
            leader = viewer,
            players = { viewer, clanMember }
        };
        var user = new Registered_user("Account", "hash") { player = viewer };
        dbContext.AddRange(viewer, clanMember, unrelated, clan, user);
        await dbContext.SaveChangesAsync();

        return new TestFixture(dbContext, isAdmin);
    }

    private sealed class TestFixture(WarframeTrackerDbContextTest dbContext, bool isAdmin)
    {
        public PlayerController CreateController()
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, "Account") };
            if (isAdmin) claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

            return new PlayerController(new PlayerService(dbContext), new UserService(dbContext))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                    }
                }
            };
        }
    }
}
