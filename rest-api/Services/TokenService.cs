using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using rest_api.Data;
using rest_api.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace rest_api.Services;

public interface ITokenService
{
    public JwtSecurityToken GenerateAccessToken(Registered_user user);
    public Task<RefreshToken> GenerateRefreshToken(Registered_user user, string? ip);
    public Task<RefreshToken?> GetRefreshTokenAsync(string token);
    public Task InvalidateRefreshTokenAsync(RefreshToken token);
}

class TokenService : ITokenService
{
    private readonly ConfigurationService _config;
    private readonly WarframeTrackerDbContext _dbContext;
    private readonly IUserService _userService;

    private static readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public TokenService(ConfigurationService config, WarframeTrackerDbContext dbContext, IUserService userService)
    {
        _config = config;
        this._dbContext = dbContext;
        this._userService = userService;
    }

    public JwtSecurityToken GenerateAccessToken(Registered_user user)
    {
        var keyBytes = _config.GetJwtKey();

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.username)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return token;
    }

    private static string GenerateRandomString(int length)
    {
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        var stringBuilder = new StringBuilder(length);
        foreach (var byteValue in bytes)
        {
            stringBuilder.Append(_chars[byteValue % _chars.Length]);
        }

        return stringBuilder.ToString();
    }

    public async Task<RefreshToken> GenerateRefreshToken(Registered_user user, string? ip)
    {
        while (true)
        {
            string token = GenerateRandomString(256);
            RefreshToken refreshToken = new RefreshToken(token, user, ip);

            try
            {
                await _dbContext.AddAsync(refreshToken);
                await _dbContext.SaveChangesAsync();
                return refreshToken;
            }
            catch (DbUpdateException ex)
            {
                // IF there is another error than duplicating tokens, throw
                if (!ex.InnerException!.Message.Contains("23505"))
                {
                    throw;
                }
            }
        }
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        var refreshToken = await _dbContext.refresh_tokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.Revoked);

        return refreshToken;
    }

    public Task InvalidateRefreshTokenAsync(RefreshToken token)
    {
        token.Revoked = true;
        _dbContext.Update(token);
        return _dbContext.SaveChangesAsync();
    }
}