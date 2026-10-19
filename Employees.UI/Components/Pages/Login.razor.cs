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

                if (result.Role?.Trim().Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase) == true)
                {
                    Navigation.NavigateTo("/create-manager");
                }
                else if (result.Role?.Trim().Equals(
                    "Manager",
                    StringComparison.OrdinalIgnoreCase) == true)
                {
                    Navigation.NavigateTo("/manager-dashboard");
                }
                else if (result.Role?.Trim().Equals(
                    "Employee",
                    StringComparison.OrdinalIgnoreCase) == true)
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