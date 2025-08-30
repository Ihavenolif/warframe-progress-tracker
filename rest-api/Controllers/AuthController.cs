using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rest_api.Data;
using rest_api.DTO;
using rest_api.Models;
using rest_api.Services;

namespace rest_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly WarframeTrackerDbContext _dbContext;
    private readonly IUserService userService;
    private readonly ConfigurationService _config;
    private readonly ITokenService _tokenService;

    public AuthController(WarframeTrackerDbContext context, ConfigurationService config, IUserService userService, ITokenService tokenService)
    {
        _dbContext = context;
        _config = config;
        this.userService = userService;
        this._tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
    {
        if (!await userService.VerifyUser(username, password))
        {
            return Unauthorized("Username and password combination not found.");
        }

        var token = _tokenService.GenerateAccessToken(username);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromQuery] string username, [FromQuery] string password)
    {
        if (await userService.GetUserByUsernameAsync(username) != null)
        {
            return Conflict("User already exists");
        }

        // TODO: Input validation
        await userService.CreateUserAsync(username, password);

        var token = _tokenService.GenerateAccessToken(username);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

        throw new NotImplementedException();
    }

    [HttpPost("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        string username = User.Identity?.Name!;

        Registered_user user = (await userService.GetUserByUsernameAsync(username))!;

        return Ok(new UserInfoDTO(user));
    }
}