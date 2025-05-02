using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rest_api.Data;
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

    public AuthController(WarframeTrackerDbContext context, ConfigurationService config, IUserService userService)
    {
        _dbContext = context;
        _config = config;
        this.userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
    {
        if (!await userService.VerifyUser(username, password))
        {
            return Unauthorized("Username and password combination not found.");
        }

        var keyBytes = _config.GetJwtKey();

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(ClaimTypes.Name, username) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

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

        var keyBytes = _config.GetJwtKey();

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(ClaimTypes.Name, username) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

        throw new NotImplementedException();
    }

    [HttpPost("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        string username = User.Identity?.Name!;

        return Ok(await userService.GetUserByUsernameAsync(username));
    }
}