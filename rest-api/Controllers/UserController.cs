using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rest_api.DTO;
using rest_api.DTOs;
using rest_api.Models;
using rest_api.Services;

namespace rest_api.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ITokenService tokenService;
    public UserController(IUserService userService, ITokenService tokenService)
    {
        this.userService = userService;
        this.tokenService = tokenService;
    }

    [HttpPost("addPlayer")]
    [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status201Created, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddPlayerToUser([FromBody] AddPlayerDTO dto)
    {
        Registered_user? user = await this.userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();
        if (user.player != null) return Conflict("You already have a player linked.");

        await this.userService.AddPlayerToUser(user, dto.PlayerName);

        var token = this.tokenService.GenerateAccessToken(user);

        return Created("", new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("removePlayer")]
    [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemovePlayerFromUser()
    {
        Registered_user? user = await this.userService.GetUserByUsernameAsync(User.Identity!.Name!);
        if (user == null) return Unauthorized();
        if (user.player == null) return Conflict("You don't have a player linked.");

        await this.userService.RemovePlayerFromUser(user);

        var token = this.tokenService.GenerateAccessToken(user);
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}