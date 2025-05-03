using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTO;
using rest_api.Services;

namespace rest_api.Controllers;

[Authorize]
[ApiController]
[Route("api/mastery")]
public class MasteryController : ControllerBase
{
    private readonly IMasteryService masteryService;
    private readonly IPlayerService playerService;

    public MasteryController(IMasteryService masteryService, IPlayerService playerService)
    {
        this.masteryService = masteryService;
        this.playerService = playerService;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<IEnumerable<MasteryItemDTO>>> GetMasteryInfoByPlayer([FromRoute] string username)
    {
        var player = await playerService.FindPlayerByUsernameAsync(username);
        if (player == null) return NotFound("Player not found");
        var masteryData = await masteryService.GetMasteryInfoByPlayerAsync(player);
        return Ok(masteryData);
    }
}