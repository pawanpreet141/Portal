using Employee.API.Controllers;
using Employee.API.Validators;
using Xunit;

namespace Employee.Tests;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    private AccountController.LoginRequest CreateValidRequest()
    {
        return new AccountController.LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
    }

    [Fact]
    public void Validate_ValidLoginRequest_Passes()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyEmail_Fails()
    {
        var request = CreateValidRequest();
        request.Email = "";

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Email) &&
            error.ErrorMessage == "Email is required.");
    }

    [Fact]
    public void Validate_InvalidEmail_Fails()
    {
        var request = CreateValidRequest();
        request.Email = "invalid-email";

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Email) &&
            error.ErrorMessage == "Please enter a valid email address.");
    }

    [Fact]
    public void Validate_EmptyPassword_Fails()
    {
        var request = CreateValidRequest();
        request.Password = "";

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Password) &&
            error.ErrorMessage == "Password is required.");
    }

    [Fact]
    public void Validate_EmptyEmailAndPassword_Fails()
    {
        var request = CreateValidRequest();
        request.Email = "";
        request.Password = "";

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Email));

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Password));
    }
}