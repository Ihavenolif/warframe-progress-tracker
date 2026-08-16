using Microsoft.AspNetCore.Mvc;
using rest_api.Controllers;
using rest_api.DTO;
using rest_api.Models;
using rest_api.Services;

namespace rest_api_testing.ControllerTests;

public class AuthControllerTest
{
    [Fact]
    public void LoginAcceptsCredentialsFromBodyOnly()
    {
        var parameter = typeof(AuthController).GetMethod(nameof(AuthController.Login))!
            .GetParameters()
            .Single();

        Assert.Equal(typeof(LoginRequestDto), parameter.ParameterType);
        Assert.NotNull(parameter.GetCustomAttributes(typeof(FromBodyAttribute), true).SingleOrDefault());
        Assert.Empty(parameter.GetCustomAttributes(typeof(FromQueryAttribute), true));
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedForInvalidCredentials()
    {
        var userService = new InvalidCredentialsUserService();
        var controller = new AuthController(null!, null!, userService, null!);
        var request = new LoginRequestDto
        {
            Username = "test-user",
            Password = "wrong-password"
        };

        var result = await controller.Login(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Username and password combination not found.", unauthorized.Value);
        Assert.Equal(("test-user", "wrong-password"), userService.Credentials);
    }

    private sealed class InvalidCredentialsUserService : IUserService
    {
        public (string Username, string Password)? Credentials { get; private set; }

        public Task<bool> VerifyUser(string username, string password)
        {
            Credentials = (username, password);
            return Task.FromResult(false);
        }

        public Task<Registered_user?> GetUserByUsernameAsync(string username) => throw new NotImplementedException();

        public Task<Registered_user> CreateUserAsync(string username, string password) => throw new NotImplementedException();

        public Task AddPlayerToUser(Registered_user user, string username) => throw new NotImplementedException();

        public Task RemovePlayerFromUser(Registered_user user) => throw new NotImplementedException();
    }
}
