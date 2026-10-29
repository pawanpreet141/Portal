using System.Linq;
using Employee.API.Controllers;
using Employee.API.Validators;
using Xunit;

namespace Employee.Tests;

public class SignupRequestValidatorTests
{
    private readonly SignupRequestValidator _validator = new();

    private AccountController.SignupRequest CreateValidRequest()
    {
        return new AccountController.SignupRequest
        {
            Name = "Rahul Sharma",
            Email = "rahul@example.com",
            Password = "Password123!"
        };
    }

    [Fact]
    public void Validate_ValidSignupRequest_Passes()
    {
        var request = CreateValidRequest();

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var request = CreateValidRequest();
        request.Name = "";

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Name) &&
            error.ErrorMessage == "Name is required.");
    }

    [Fact]
    public void Validate_NameExceeds100Characters_Fails()
    {
        var request = CreateValidRequest();
        request.Name = new string('A', 101);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Name) &&
            error.ErrorMessage == "Name cannot exceed 100 characters.");
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

    [Theory]
    [InlineData("pass123!")]
    [InlineData("PASSWORD123!")]
    [InlineData("PasswordOnly!")]
    [InlineData("Password123")]
    [InlineData("Pa1!")]
    public void Validate_PasswordMissingRequiredCharacters_Fails(
        string password)
    {
        var request = CreateValidRequest();
        request.Password = password;

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(request.Password) &&
            error.ErrorMessage ==
                "Password must be at least 8 characters long, " +
                "include at least one uppercase letter, " +
                "one number, and one special character.");
    }
}
