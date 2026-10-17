using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Tasqana.Controllers;

namespace Tasqana.Tests;

public class AppTokenTests
{
    [Test]
    public async Task AppToken_WithoutAuthorizationDoesNotCreateSession()
    {
        var controller = new UsersController(null!, NullLogger<UsersController>.Instance, null!)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.AppToken();

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }
}
