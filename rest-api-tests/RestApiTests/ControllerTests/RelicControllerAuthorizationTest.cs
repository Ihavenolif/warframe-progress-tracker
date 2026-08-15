using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using rest_api.Controllers;

namespace rest_api_testing.ControllerTests;

public class RelicControllerAuthorizationTest
{
    [Fact]
    public void ControllerIsAuthenticatedAndReadOnly()
    {
        Assert.Contains(typeof(RelicController).GetCustomAttributes(typeof(AuthorizeAttribute), true),
            attribute => ((AuthorizeAttribute)attribute).Roles == null);

        var actions = typeof(RelicController).GetMethods()
            .Where(method => method.DeclaringType == typeof(RelicController)
                && method.GetCustomAttributes(typeof(HttpMethodAttribute), true).Length > 0)
            .ToList();

        Assert.Equal(["GetRelic", "GetRelics"], actions.Select(method => method.Name).Order());
        Assert.All(actions, method => Assert.IsType<HttpGetAttribute>(
            method.GetCustomAttributes(typeof(HttpMethodAttribute), true).Single()));
    }
}
