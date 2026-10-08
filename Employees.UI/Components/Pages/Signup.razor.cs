using System.Text.Json;
using Employees.UI.Services;

namespace Employees.UI.Components.Pages
{
    public partial class Signup
    {
        private string Name = string.Empty;
        private string Email = string.Empty;
        private string Department = string.Empty;
        private string Password = string.Empty;
        private string ConfirmPassword = string.Empty;

        private string ErrorMessage = string.Empty;
        private string SuccessMessage = string.Empty;

        private async Task SignupUser()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Validate fields individually
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Name is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Department))
            {
                ErrorMessage = "Department is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Confirm Password is required.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            var result = await Api.Signup(
                new SignupRequest
                {
                    Name = Name,
                    Email = Email,
                    Password = Password,
                    Department = Department
                });

            if (result == null || !result.Success)
            {
                ErrorMessage = GetMessage(
                    result?.Message,
                    "Signup failed.");

                return;
            }

            SuccessMessage = "Account created successfully.";

            await Task.Delay(1000);

            Navigation.NavigateTo("/login");
        }

        private string GetMessage(
            string? message,
            string defaultMessage)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return defaultMessage;
            }

            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(message);

                if (document.RootElement.TryGetProperty(
                    "message",
                    out JsonElement messageElement))
                {
                    return messageElement.GetString()
                           ?? defaultMessage;
                }
            }
            catch (JsonException)
            {
                // Message is already plain text
            }

            return message;
        }
    }
}