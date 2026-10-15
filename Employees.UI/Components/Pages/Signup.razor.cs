using Employees.UI.Services;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Employees.UI.Components.Pages
{
    public partial class Signup
    {



        [SupplyParameterFromQuery(Name = "department")]
        public string Department { get; set; } = string.Empty;

        private string Name = string.Empty;
        private string Email = string.Empty;
        private string Password = string.Empty;
        private string ConfirmPassword = string.Empty;
        private string ErrorMessage = string.Empty;
        private string SuccessMessage = string.Empty;
        private bool IsBusy;

        private static readonly string[] AllowedDepartments =
        {
        "Development", "Designer", "Sales", "SEO",
        "System Engineer", "DevOps"
    };

        private async Task SignupUser()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!AllowedDepartments.Contains(
                Department, StringComparer.OrdinalIgnoreCase))
            {
                ErrorMessage = "Please select a valid department.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "All fields are required.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            IsBusy = true;

            try
            {
                var result = await Api.Signup(new SignupRequest
                {
                    Name = Name,
                    Email = Email,
                    Password = Password,
                    Department = Department
                });

                if (result == null || !result.Success)
                {
                    ErrorMessage = result?.Message ?? "Signup failed.";
                    return;
                }

                SuccessMessage = "Account created successfully.";

                await Task.Delay(1000);

                // Navigation.NavigateTo("/employee-login");
                Navigation.NavigateTo("/login");

            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}