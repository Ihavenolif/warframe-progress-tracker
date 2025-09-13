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
using Swashbuckle.AspNetCore.Annotations;

namespace rest_api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
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
    [SwaggerOperation(Summary = "Logs in a user", Description = "Authenticates a user with the provided username and password.")]
    [SwaggerResponse(200, "Login successful", typeof(TokenResponseDTO))]
    [SwaggerResponse(401, "Username and password combination not found.", typeof(string))]
    [SwaggerResponse(400, "Bad request", typeof(string))]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
    {
        if (!await userService.VerifyUser(username, password))
        {
            return Unauthorized("Username and password combination not found.");
        }

        Registered_user user = (await userService.GetUserByUsernameAsync(username))!;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = await _tokenService.GenerateRefreshToken(user, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _config.SecureCookies,
            SameSite = SameSiteMode.None,
            Domain = $".{_config.OriginUrl}",
            MaxAge = TimeSpan.FromDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(accessToken) });
    }

    [HttpPost("register")]
    [SwaggerOperation(Summary = "Registers a new user", Description = "Creates a new user with the provided username and password.")]
    [SwaggerResponse(200, "User registered successfully", typeof(TokenResponseDTO))]
    [SwaggerResponse(409, "User already exists", typeof(string))]
    [SwaggerResponse(400, "Bad request", typeof(string))]
    public async Task<IActionResult> Register([FromQuery] string username, [FromQuery] string password)
    {
        if (await userService.GetUserByUsernameAsync(username) != null)
        {
            return Conflict("User already exists");
        }

        // TODO: Input validation
        Registered_user user = await userService.CreateUserAsync(username, password);

        var token = _tokenService.GenerateAccessToken(user);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Refreshes the access token", Description = "Generates a new access token using a valid refresh token. Refresh token is sent as an HttpOnly cookie.")]
    [SwaggerResponse(200, "Token refreshed successfully", typeof(TokenResponseDTO))]
    [SwaggerResponse(401, "Invalid or expired refresh token", typeof(string))]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        string? refreshTokenRaw = Request.Cookies["refreshToken"];
        if (refreshTokenRaw == null)
        {
            return Unauthorized("No refresh token provided");
        }

        var refreshToken = await _tokenService.GetRefreshTokenAsync(refreshTokenRaw);
        if (refreshToken == null || refreshToken.IsExpired || refreshToken.Revoked)
        {
            return Unauthorized("Invalid refresh token");
        }

        if (refreshToken.IssuedByIp != Request.HttpContext.Connection.RemoteIpAddress?.ToString())
        {
            await _tokenService.InvalidateRefreshTokenAsync(refreshToken);
            return Unauthorized("Invalid refresh token");
        }

        var user = await userService.GetUserByUsernameAsync(refreshToken.User!.username);

        if (user is null)
        {
            return Unauthorized("Invalid refresh token");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = await _tokenService.GenerateRefreshToken(refreshToken.User, Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? null);
        await _tokenService.InvalidateRefreshTokenAsync(refreshToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _config.SecureCookies,
            SameSite = SameSiteMode.None,
            Domain = $".${_config.OriginUrl}",
            MaxAge = TimeSpan.FromDays(7)
        };

        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(accessToken) });
    }

    [HttpPost("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Gets the current user's information", Description = "Returns the information of the currently authenticated user.")]
    [SwaggerResponse(200, "User information retrieved successfully", typeof(UserInfoDTO))]
    [SwaggerResponse(401, "Unauthorized")]

    public async Task<IActionResult> Me()
    {
        string username = User.Identity?.Name!;

        Registered_user user = (await userService.GetUserByUsernameAsync(username))!;

        return Ok(new UserInfoDTO(user));
    }

    [HttpPost("logout")]
    [Authorize]
    [SwaggerOperation(Summary = "Logs out the current user", Description = "Invalidates the current refresh token and removes it from the client.")]
    [SwaggerResponse(200, "Logged out successfully", typeof(string))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> Logout()
    {
        string? refreshTokenRaw = Request.Cookies["refreshToken"];
        if (refreshTokenRaw != null)
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync(refreshTokenRaw);
            if (refreshToken != null)
            {
                await _tokenService.InvalidateRefreshTokenAsync(refreshToken);
            }
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _config.SecureCookies,
            SameSite = SameSiteMode.None,
            Domain = ".localhost.me",
            Expires = DateTime.UtcNow.AddDays(-1) // Expire the cookie
        };

        Response.Cookies.Append("refreshToken", "", cookieOptions);

        return Ok("Logged out successfully");
    }
}