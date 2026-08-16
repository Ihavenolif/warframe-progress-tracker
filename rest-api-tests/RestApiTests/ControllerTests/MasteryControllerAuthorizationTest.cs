using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rest_api.Controllers;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;
using rest_api.Services;
using rest_api_testing.Dababase;

namespace rest_api_testing.ControllerTests;

public class MasteryControllerAuthorizationTest
{
    [Theory]
    [InlineData("Viewer")]
    [InlineData("ClanMember")]
    public async Task SelfAndClanMemberAccessReturnsMastery(string targetUsername)
    {
        var fixture = await CreateFixtureAsync();
        var controller = fixture.CreateController();

        var result = await controller.GetMasteryInfoByPlayer(targetUsername);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(targetUsername, fixture.MasteryService.RequestedPlayer?.username);
    }

    [Fact]
    public async Task AdminCanAccessUnrelatedPlayer()
    {
        var fixture = await CreateFixtureAsync(isAdmin: true);
        var controller = fixture.CreateController();

        var result = await controller.GetMasteryInfoByPlayer("Unrelated");

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Unrelated", fixture.MasteryService.RequestedPlayer?.username);
    }

    [Theory]
    [InlineData("Unrelated")]
    [InlineData("Missing")]
    public async Task UnrelatedAndMissingPlayersReturnSameNotFoundResponse(string targetUsername)
    {
        var fixture = await CreateFixtureAsync();
        var controller = fixture.CreateController();

        var result = await controller.GetMasteryInfoByPlayer(targetUsername);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Player not found", notFound.Value);
        Assert.Null(fixture.MasteryService.RequestedPlayer);
    }

    [Fact]
    public async Task SuccessfulImportReturnsReceiptSummary()
    {
        var fixture = await CreateFixtureAsync();
        var controller = fixture.CreateController();
        var file = new FormFile(
            new MemoryStream("{}"u8.ToArray()),
            0,
            2,
            "jsonFile",
            "out.json");

        IActionResult result = await controller.UpdatePlayerMastery(file);

        var ok = Assert.IsType<OkObjectResult>(result);
        var receipt = Assert.IsType<MasteryImportReceiptDto>(ok.Value);
        Assert.Equal(42, receipt.ProcessedCount);
        Assert.Equal("{}", fixture.MasteryService.ImportedJson);
        Assert.Equal("Viewer", fixture.MasteryService.RequestedPlayer?.username);
    }

    [Fact]
    public async Task DashboardSummaryUsesCurrentUsersPlayer()
    {
        var fixture = await CreateFixtureAsync();
        var controller = fixture.CreateController();

        var result = await controller.GetDashboardSummary();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<DashboardSummaryDto>(ok.Value);
        Assert.Equal("Viewer", fixture.MasteryService.RequestedPlayer?.username);
    }

    [Fact]
    public async Task DashboardSummaryReturnsUnauthorizedForMissingUser()
    {
        var fixture = await CreateFixtureAsync();
        var controller = fixture.CreateController("Missing");

        var result = await controller.GetDashboardSummary();

        Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.Null(fixture.MasteryService.RequestedPlayer);
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

    private sealed class TestFixture(WarframeTrackerDbContext dbContext, bool isAdmin)
    {
        public FakeMasteryService MasteryService { get; } = new();

        public MasteryController CreateController(string username = "Account")
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, username) };
            if (isAdmin) claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

            return new MasteryController(
                MasteryService,
                new PlayerService(dbContext),
                new UserService(dbContext))
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

    private sealed class FakeMasteryService : IMasteryService
    {
        public Player? RequestedPlayer { get; private set; }

        public string? ImportedJson { get; private set; }

        public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByPlayerAsync(Player player)
        {
            RequestedPlayer = player;
            return Task.FromResult<IEnumerable<MasteryItemDTO>>([]);
        }

        public Task<MasteryImportReceiptDto> UpdatePlayerMasteryAsync(Player player, string jsonData)
        {
            RequestedPlayer = player;
            ImportedJson = jsonData;
            return Task.FromResult(new MasteryImportReceiptDto { ProcessedCount = 42 });
        }
        public Task<IEnumerable<MasteryItemDTO>> GetMasteryInfoByClanAsync(Clan clan) => throw new NotImplementedException();
        public Task<DashboardSummaryDto> GetDashboardSummaryAsync(Player player)
        {
            RequestedPlayer = player;
            return Task.FromResult(new DashboardSummaryDto());
        }
        public Task<List<DashboardProgressEntryDTO>> GetLatestProgressEntriesAsync(Player player) => throw new NotImplementedException();
        public Task<List<DashboardProgressDayDTO>> GetDailyProgressAsync(Player player, int days) => throw new NotImplementedException();
        public Task<MasteryImportReceiptDto?> GetLatestImportReceiptAsync(Player player) => throw new NotImplementedException();
    }
}
