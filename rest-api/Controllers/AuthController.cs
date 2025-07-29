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

        var token = _tokenService.GenerateAccessToken(username);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

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
        await userService.CreateUserAsync(username, password);

        var token = _tokenService.GenerateAccessToken(username);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

        throw new NotImplementedException();
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
}