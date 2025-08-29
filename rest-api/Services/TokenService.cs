using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace rest_api.Services;

public interface ITokenService
{
    public JwtSecurityToken GenerateAccessToken(string username);
}

class TokenService : ITokenService
{
    private readonly ConfigurationService _config;

    public TokenService(ConfigurationService config)
    {
        _config = config;
    }

    public JwtSecurityToken GenerateAccessToken(string username)
    {
        var keyBytes = _config.GetJwtKey();

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(ClaimTypes.Name, username) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return token;
    }
}