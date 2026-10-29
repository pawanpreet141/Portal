
using System.Collections.Generic;
using System.Threading.Tasks;
using Employee.API.Controllers;
using Employee.API.Validators;
using Employee.Data.Data;
using Employee.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Employee.Tests;

public class AccountControllerTests
{
    private static EmployeeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(
                Guid.NewGuid().ToString())
            .Options;

        return new EmployeeDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] =
                "ThisIsATestJwtKeyThatIsLongEnough123456789",
            ["Jwt:Issuer"] = "Employee.Tests",
            ["Jwt:Audience"] = "Employee.Tests"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static AccountController CreateController(
        EmployeeDbContext context)
    {
        return new AccountController(
            context,
            CreateConfiguration(),
            new SignupRequestValidator(),
            new LoginRequestValidator(),
            NullLogger<AccountController>.Instance);
    }

    [Fact]
    public async Task Login_EmptyEmail_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var request = new AccountController.LoginRequest
        {
            Email = "",
            Password = "Password123!"
        };

        var result = await controller.Login(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        using var context = CreateContext();
        var controller = CreateController(context);

        var request = new AccountController.LoginRequest
        {
            Email = "unknown@example.com",
            Password = "Password123!"
        };

        var result = await controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        using var context = CreateContext();

        var user = new User
        {
            Name = "Rahul Sharma",
            Email = "rahul@example.com",
            Role = "Employee",
            Department = "Development"
        };

        var hasher = new PasswordHasher<User>();

        user.PasswordHash = hasher.HashPassword(
            user, "Password123!");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var request = new AccountController.LoginRequest
        {
            Email = "rahul@example.com",
            Password = "Password123!"
        };

        var result = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}

