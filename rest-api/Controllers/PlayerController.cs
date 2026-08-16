using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.Models;
using rest_api.Services;

namespace rest_api.Controllers;

[Authorize]
[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IUserService _userService;

    public PlayerController(IPlayerService playerService, IUserService userService)
    {
        _playerService = playerService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        Registered_user? user = await _userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();

        return Ok(await _playerService.GetAccessiblePlayersAsync(user.player?.id, User.IsInRole("ADMIN")));
    }
}
