using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Employee.API.Controllers;
using Employee.API.DTOs;
using Employee.API.Validators;
using Employee.BusinessLogic;
using Employee.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Employee.Tests;

public class EmployeeControllerTests
{
    private readonly Mock<IEmployeeService> _serviceMock = new();

    private EmployeesController CreateController(
        bool includeUserId = true)
    {
        var controller = new EmployeesController(
            _serviceMock.Object,
            new EmployeeValidator(),
            NullLogger<EmployeesController>.Instance);

        var claims = new List<Claim>();

        if (includeUserId)
        {
            claims.Add(new Claim(
                ClaimTypes.NameIdentifier, "42"));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "Test"))
            }
        };

        return controller;
    }

    private static Employee1 CreateValidEmployee()
    {
        return new Employee1
        {
            Id = 1,
            Name = "Rahul Sharma",
            Email = "rahul@example.com",
            Age = 25,
            Team = "Development",
            Salary = 30000
        };
    }

    [Fact]
    public async Task GetEmployees_ValidManager_ReturnsOk()
    {
        var employees = new List<Employee1>
        {
            CreateValidEmployee()
        };

        _serviceMock
            .Setup(s => s.GetAllAsync(42))
            .ReturnsAsync(employees);

        var controller = CreateController();

        var result = await controller.GetEmployees();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(employees, okResult.Value);
    }

    [Fact]
    public async Task GetEmployees_MissingUserId_ReturnsUnauthorized()
    {
        var controller = CreateController(includeUserId: false);

        var result = await controller.GetEmployees();

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
        _serviceMock.Verify(
            s => s.GetAllAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEmployee_EmployeeNotFound_ReturnsNotFound()
    {
        _serviceMock
            .Setup(s => s.GetByIdAsync(10, 42))
            .ReturnsAsync((Employee1?)null);

        var controller = CreateController();

        var result = await controller.GetEmployee(10);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateEmployee_ValidRequest_ReturnsCreated()
    {
        var employee = CreateValidEmployee();

        _serviceMock
            .Setup(s => s.AddAsync(
                It.IsAny<Employee1>(), 42))
            .ReturnsAsync((Employee1 e, int _) =>
            {
                e.Id = 1;
                return e;
            });

        var controller = CreateController();

        var request = new CreateEmployeeRequest
        {
            Name = employee.Name,
            Email = employee.Email,
            Age = employee.Age,
            Team = employee.Team,
            Salary = employee.Salary
        };

        var result = await controller.CreateEmployee(request);

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(nameof(EmployeesController.GetEmployee),
            createdResult.ActionName);
    }

    [Fact]
    public async Task CreateEmployee_InvalidEmail_ReturnsBadRequest()
    {
        var controller = CreateController();

        var request = new CreateEmployeeRequest
        {
            Name = "Rahul Sharma",
            Email = "invalid-email",
            Age = 25,
            Team = "Development",
            Salary = 30000
        };

        var result = await controller.CreateEmployee(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);

        _serviceMock.Verify(
            s => s.AddAsync(It.IsAny<Employee1>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteEmployee_NotFound_ReturnsNotFound()
    {
        _serviceMock
            .Setup(s => s.DeleteAsync(10, 42))
            .ReturnsAsync(false);

        var controller = CreateController();

        var result = await controller.DeleteEmployee(10);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
