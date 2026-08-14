using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTOs.Relics;
using rest_api.Services;

namespace rest_api.Controllers;

[ApiController]
[Authorize]
[Route("api/relics")]
public class RelicController : ControllerBase
{
    private readonly IRelicService _relicService;
    private readonly IUserService _userService;

    public RelicController(IRelicService relicService, IUserService userService)
    {
        _relicService = relicService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<RelicPageDTO>> GetRelics([FromQuery] RelicQueryDTO query)
    {
        var user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();
        if (user.player == null && !User.IsInRole("ADMIN")) return NotFound("Player not found");
        try
        {
            return Ok(await _relicService.GetRelicsAsync(user.player?.id ?? 0, query));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RelicDTO>> GetRelic(int id)
    {
        var user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();
        if (user.player == null && !User.IsInRole("ADMIN")) return NotFound("Player not found");
        var relic = await _relicService.GetRelicAsync(user.player?.id ?? 0, id);
        return relic == null ? NotFound("Relic not found") : Ok(relic);
    }
}
