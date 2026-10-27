using System.Text.Json;
using Employees.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Employees.UI.Components.Pages
{
    public partial class Login
    {
        private string Email = string.Empty;
        private string Password = string.Empty;
        private string ErrorMessage = string.Empty;
        private bool IsBusy;


        private async Task LoginUser()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email and password are required.";
                return;
            }

            IsBusy = true;

            try
            {
                var result = await Api.Login(new LoginRequest
                {
                    Email = Email,
                    Password = Password
                });

                if (result == null || !result.Success)
                {
                    ErrorMessage = result?.Message ?? "Login failed.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(result.Token))
                {
                    ErrorMessage = "Login response does not contain a token.";
                    return;
                }



                UserSession.Login(
                    result.Id,
                    result.Name,
                    result.Email,
                    result.Role,
                    result.Department,
                    result.Token);

                Console.WriteLine($"LOGIN USER: {UserSession.Name}");
                Console.WriteLine($"LOGIN ROLE: {UserSession.Role}");
                Console.WriteLine($"LOGIN STATUS: {UserSession.IsLoggedIn}");

                AuthProvider.NotifyUserLogin();



                var role = result.Role?.Trim();

                if (string.Equals(role, "Admin",
                    StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo("/create-manager");
                }
                else if (string.Equals(role, "Manager",
                    StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo("/manager-dashboard");
                }
                else if (string.Equals(role, "HR",
                    StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo("/signup");
                }
                else if (string.Equals(role, "Technical",
                    StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo("/signup");
                }
                else if (string.Equals(role, "Employee",
                    StringComparison.OrdinalIgnoreCase))
                {
                    Navigation.NavigateTo("/employee-dashboard");
                }
                else
                {
                    ErrorMessage = $"Unexpected role: {result.Role}";
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}