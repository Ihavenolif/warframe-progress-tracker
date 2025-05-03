using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using rest_api.Data;
using rest_api.Models;

namespace rest_api.Services;

public interface IUserService
{
    public Task<Registered_user?> GetUserByUsernameAsync(string username);
    public Task CreateUserAsync(string username, string password);
    public Task<bool> VerifyUser(string username, string password);
    public Task AddPlayerToUser(Registered_user user, string username);
}

public class UserService : IUserService
{
    private readonly WarframeTrackerDbContext _dbContext;

    public UserService(WarframeTrackerDbContext context)
    {
        _dbContext = context;
    }

    public async Task AddPlayerToUser(Registered_user user, string playerName)
    {
        Player player = new Player(playerName);
        user.player = player;
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateUserAsync(string username, string password)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        Registered_user user = new Registered_user(username, hashedPassword);
        await _dbContext.registered_users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Registered_user?> GetUserByUsernameAsync(string username)
    {
        return await _dbContext.registered_users
            .Include(u => u.player)
            .FirstOrDefaultAsync(u => u.username == username);
    }

    public async Task<bool> VerifyUser(string username, string password)
    {
        var storedUser = await GetUserByUsernameAsync(username);

        if (storedUser == null) return false;

        return BCrypt.Net.BCrypt.Verify(password, storedUser.password_hash);
    }
}