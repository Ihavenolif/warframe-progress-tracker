using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.Models;
using rest_api.Services;

namespace rest_api.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    IUserService userService;
    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPost("addPlayer")]
    public async Task<IActionResult> AddPlayerToUser([FromQuery] string playerName)
    {
        Registered_user? user = await this.userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();
        if (user.player != null) return Conflict("You already have a player linked.");

        await this.userService.AddPlayerToUser(user, playerName);

        return Created();
    }
}