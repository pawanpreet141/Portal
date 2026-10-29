using Employee.BusinessLogic;
using Employee.Data.Data;
using Employee.Data.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Employee.Tests;

public class EmployeeServiceTests : IDisposable
{
    private readonly EmployeeDbContext _context;
    private readonly Mock<IEmployeeRepository> _repository;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseInMemoryDatabase(
                Guid.NewGuid().ToString())
            .Options;

        _context = new EmployeeDbContext(options);

        _repository = new Mock<IEmployeeRepository>();

        _service = new EmployeeService(
            _repository.Object,
            _context);
    }

    private async Task AddUserAsync(
        int id,
        string role,
        string? department = null)
    {
        _context.Users.Add(new User
        {
            Id = id,
            Role = role,
            Department = department
        });

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmployees_ForValidManager()
    {
        // Arrange
        await AddUserAsync(1, "Manager", "HR");

        var employees = new List<Employee1>
        {
            new Employee1 { Id = 10, Name = "Rahul" },
            new Employee1 { Id = 11, Name = "Priya" }
        };

        _repository
            .Setup(r => r.GetAllAsync(1))
            .ReturnsAsync(employees);

        // Act
        var result = await _service.GetAllAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Rahul", result[0].Name);

        _repository.Verify(
            r => r.GetAllAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenUserIsNotManager()
    {
        // Arrange
        await AddUserAsync(2, "Employee", "HR");

        // Act
        var result = await _service.GetAllAsync(2);

        // Assert
        Assert.Empty(result);

        _repository.Verify(
            r => r.GetAllAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserIsNotManager()
    {
        // Arrange
        await AddUserAsync(2, "Employee", "HR");

        // Act
        var result = await _service.GetByIdAsync(10, 2);

        // Assert
        Assert.Null(result);

        _repository.Verify(
            r => r.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task AddAsync_CreatesEmployee_ForValidHRManager()
    {
        // Arrange
        await AddUserAsync(1, "Manager", "HR");

        var employee = new Employee1
        {
            Id = 99,
            Name = "Rahul",
            Email = "rahul@example.com",
            Age = 25,
            Salary = 30000,
            Team = "Development"
        };

        _repository
            .Setup(r => r.AddAsync(It.IsAny<Employee1>()))
            .ReturnsAsync((Employee1 e) => e);

        // Act
        var result = await _service.AddAsync(employee, 1);

        // Assert
        Assert.Equal(0, result.Id);
        Assert.Equal(1, result.ManagerId);
        Assert.Equal("HR", result.Department);
        Assert.Equal("Development", result.Team);

        _repository.Verify(
            r => r.AddAsync(employee),
            Times.Once);
    }

    [Fact]
    public async Task AddAsync_ThrowsUnauthorizedAccess_WhenUserIsNotManager()
    {
        // Arrange
        await AddUserAsync(2, "Employee", "HR");

        var employee = new Employee1
        {
            Name = "Rahul",
            Team = "Development"
        };

        // Act and Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.AddAsync(employee, 2));

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Employee1>()),
            Times.Never);
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentException_ForInvalidTeam()
    {
        // Arrange
        await AddUserAsync(1, "Manager", "HR");

        var employee = new Employee1
        {
            Name = "Rahul",
            Team = "DevOps"
        };

        // Act and Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AddAsync(employee, 1));

        Assert.Contains("HR manager", exception.Message);

        _repository.Verify(
            r => r.AddAsync(It.IsAny<Employee1>()),
            Times.Never);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
