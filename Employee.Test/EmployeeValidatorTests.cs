
using System.Linq;
using Employee.API.Validators;
using Employee.Data.Models;
using Xunit;

namespace Employee.Tests;

public class EmployeeValidatorTests
{
    private readonly EmployeeValidator _validator = new();

    private Employee1 CreateValidEmployee()
    {
        return new Employee1
        {
            Name = "Rahul Sharma",
            Email = "rahul@example.com",
            Age = 25,
            Team = "Development",
            Salary = 30000
        };
    }

    [Fact]
    public void Validate_ValidEmployee_Passes()
    {
        // Arrange
        var employee = CreateValidEmployee();

        // Act
        var result = _validator.Validate(employee);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var employee = CreateValidEmployee();
        employee.Name = "";

        var result = _validator.Validate(employee);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Name) &&
            error.ErrorMessage == "Employee name is required.");
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var employee = CreateValidEmployee();
        employee.Email = "invalid-email";

        var result = _validator.Validate(employee);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Email));
    }

    [Theory]
    [InlineData(17)]
    [InlineData(61)]
    [InlineData(10)]
    public void Validate_AgeOutsideAllowedRange_Fails(int age)
    {
        var employee = CreateValidEmployee();
        employee.Age = age;

        var result = _validator.Validate(employee);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Age));
    }

    [Theory]
    [InlineData(18)]
    [InlineData(30)]
    [InlineData(60)]
    public void Validate_AgeInsideAllowedRange_Passes(int age)
    {
        var employee = CreateValidEmployee();
        employee.Age = age;

        var result = _validator.Validate(employee);

        Assert.DoesNotContain(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Age));
    }

    [Fact]
    public void Validate_InvalidTeam_Fails()
    {
        var employee = CreateValidEmployee();
        employee.Team = "Accounting";

        var result = _validator.Validate(employee);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Team));
    }

    [Theory]
    [InlineData("Designer")]
    [InlineData("Development")]
    [InlineData("Sales")]
    [InlineData("SEO")]
    [InlineData("System Engineer")]
    [InlineData("DevOps")]
    public void Validate_AllowedTeam_Passes(string team)
    {
        var employee = CreateValidEmployee();
        employee.Team = team;

        var result = _validator.Validate(employee);

        Assert.DoesNotContain(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Team));
    }

    [Fact]
    public void Validate_ZeroSalary_Fails()
    {
        var employee = CreateValidEmployee();
        employee.Salary = 0;

        var result = _validator.Validate(employee);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Salary));
    }

    [Fact]
    public void Validate_NegativeSalary_Fails()
    {
        var employee = CreateValidEmployee();
        employee.Salary = -100;

        var result = _validator.Validate(employee);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(Employee1.Salary));
    }
}

