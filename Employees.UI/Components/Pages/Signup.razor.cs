//using Employees.UI.Services;
//using Microsoft.AspNetCore.Components;

//namespace Employees.UI.Components.Pages
//{
//    public partial class Signup
//    {
//        [SupplyParameterFromQuery(Name = "department")]
//        public string Department { get; set; } = string.Empty;


//        private string Name = string.Empty;
//        private string Email = string.Empty;
//        private string Password = string.Empty;
//        private string ConfirmPassword = string.Empty;

//        private string ErrorMessage = string.Empty;
//        private string SuccessMessage = string.Empty;

//        private bool IsBusy;

//        private static readonly string[] HrTeams =
//        {
//            "Development",
//            "Designer",
//            "Sales",
//            "SEO"
//        };

//        private static readonly string[] TechnicalTeams =
//        {
//            "System Engineer",
//            "DevOps"
//        };

//        private bool IsAllowedTeam
//        {
//            get
//            {
//                if (!Session.IsLoggedIn ||
//                    !string.Equals(
//                        Session.Role,
//                        "Manager",
//                        StringComparison.OrdinalIgnoreCase))
//                {
//                    return false;
//                }

//                if (string.Equals(
//                    Session.Department,
//                    "HR",
//                    StringComparison.OrdinalIgnoreCase))
//                {
//                    return HrTeams.Contains(
//                        Department,
//                        StringComparer.OrdinalIgnoreCase);
//                }

//                if (string.Equals(
//                    Session.Department,
//                    "Technical",
//                    StringComparison.OrdinalIgnoreCase))
//                {
//                    return TechnicalTeams.Contains(
//                        Department,
//                        StringComparer.OrdinalIgnoreCase);
//                }

//                return false;
//            }
//        }

//        protected override void OnInitialized()
//        {
//            if (!Session.IsLoggedIn ||
//                !string.Equals(
//                    Session.Role,
//                    "Manager",
//                    StringComparison.OrdinalIgnoreCase))
//            {
//                Navigation.NavigateTo("/manager-login");
//            }
//        }

//        private async Task SignupUser()
//        {
//            ErrorMessage = string.Empty;
//            SuccessMessage = string.Empty;

//            if (!IsAllowedTeam)
//            {
//                ErrorMessage =
//                    "You are not authorized to create an employee for this team.";
//                return;
//            }

//            if (string.IsNullOrWhiteSpace(Name) ||
//                string.IsNullOrWhiteSpace(Email) ||
//                string.IsNullOrWhiteSpace(Password))
//            {
//                ErrorMessage = "All fields are required.";
//                return;
//            }

//            if (Password != ConfirmPassword)
//            {
//                ErrorMessage = "Passwords do not match.";
//                return;
//            }

//            IsBusy = true;

//            try
//            {
//                var result = await Api.Signup(new SignupRequest
//                {
//                    Name = Name.Trim(),
//                    Email = Email.Trim(),
//                    Password = Password,

//                    // This is the selected team.
//                    Department = Department
//                });

//                if (result == null || !result.Success)
//                {
//                    ErrorMessage =
//                        result?.Message ?? "Signup failed.";
//                    return;
//                }

//                SuccessMessage = "Account created successfully.";

//                Navigation.NavigateTo("/employee-login");
//            }
//            catch (Exception)
//            {
//                ErrorMessage =
//                    "Unable to create the account. Please try again.";
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        private void BackToDashboard()
//        {
//            Navigation.NavigateTo("/manager-dashboard");
//        }
//    }
//}





using Employees.UI.Services;
using Microsoft.AspNetCore.Components;

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

        private static readonly string[] HrTeams =
        {
            "Development", "Designer", "Sales", "SEO"
        };

        private static readonly string[] TechnicalTeams =
        {
            "System Engineer", "DevOps"
        };

        private string[] AvailableTeams =>
            string.Equals(Session.Department, "HR",
                StringComparison.OrdinalIgnoreCase)
                ? HrTeams
                : string.Equals(Session.Department, "Technical",
                    StringComparison.OrdinalIgnoreCase)
                    ? TechnicalTeams
                    : Array.Empty<string>();

        private bool IsAllowedTeam =>
            Session.IsLoggedIn &&
            (string.Equals(Session.Role, "Manager",
                 StringComparison.OrdinalIgnoreCase) ||
             string.Equals(Session.Role, "HR",
                 StringComparison.OrdinalIgnoreCase) ||
             string.Equals(Session.Role, "Technical",
                 StringComparison.OrdinalIgnoreCase)) &&
            AvailableTeams.Contains(
                Department, StringComparer.OrdinalIgnoreCase);

        protected override void OnInitialized()
        {
            if (!Session.IsLoggedIn ||
                !(string.Equals(Session.Role, "Manager",
                      StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(Session.Role, "HR",
                      StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(Session.Role, "Technical",
                      StringComparison.OrdinalIgnoreCase)))
            {
                Navigation.NavigateTo("/login");
                return;
            }

            // Choose a default team if no team was supplied in the URL.
            if (string.IsNullOrWhiteSpace(Department) &&
                AvailableTeams.Length > 0)
            {
                Department = AvailableTeams[0];
            }
        }

        private async Task SignupUser()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!IsAllowedTeam)
            {
                ErrorMessage = "Please select a valid team.";
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
                    Name = Name.Trim(),
                    Email = Email.Trim(),
                    Password = Password,
                    Department = Department
                });

                if (result == null || !result.Success)
                {
                    ErrorMessage = result?.Message ?? "Signup failed.";
                    return;
                }

                Navigation.NavigateTo("/employee-login");
            }
            catch
            {
                ErrorMessage =
                    "Unable to create the account. Please try again.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void BackToDashboard()
        {
            Navigation.NavigateTo(
                string.Equals(Session.Role, "Manager",
                    StringComparison.OrdinalIgnoreCase)
                    ? "/manager-dashboard"
                    : "/login");
        }
    }
}
