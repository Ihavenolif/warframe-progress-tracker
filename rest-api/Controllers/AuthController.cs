using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly WarframeTrackerDbContext _dbContext;
    private readonly IConfiguration _config;

    public AuthController(WarframeTrackerDbContext context, IConfiguration config)
    {
        _dbContext = context;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromQuery] string username, [FromQuery] string password)
    {
        if (username == "username" && password == "password")
        {
            // TODO: Export into a configuration manager
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "placeholderjwtkey";
            var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[] { new Claim(ClaimTypes.Name, username) },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });

        }

        return Unauthorized();
    }
}