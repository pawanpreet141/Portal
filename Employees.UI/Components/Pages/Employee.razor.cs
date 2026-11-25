using Employees.UI.Models;
using Employees.UI.Services;
using Microsoft.AspNetCore.Components;

namespace Employees.UI.Components.Pages
{
    public partial class Employee
    {
        public List<Employee1>? employees { get; set; }

        private Employee1 employee = new();

        private string employeePassword = string.Empty;

        private bool showForm;
        private bool IsBusy;
        private bool IsSuccess;

        private int editingId;

        private string Message = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (!UserSession.IsLoggedIn)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            if (!string.Equals(
                    UserSession.Role,
                    "HR",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    UserSession.Role,
                    "Technical",
                    StringComparison.OrdinalIgnoreCase))
            {
                Navigation.NavigateTo("/login");
                return;
            }

            await LoadEmployees();
        }

        // LOAD EMPLOYEES

        private async Task LoadEmployees()
        {
            try
            {
                employees = await HttpClient.GetAsync<List<Employee1>>(
                    "api/Employees")
                    ?? new List<Employee1>();

                Message = string.Empty;
            }
            catch (Exception ex)
            {
                employees = new List<Employee1>();

                Message = $"Unable to load employees: {ex.Message}";

                IsSuccess = false;
            }
        }

        // SHOW ADD FORM

        private void ShowAddForm()
        {
            employee = new Employee1
            {
                Team = string.Empty
            };

            employeePassword = string.Empty;

            editingId = 0;

            Message = string.Empty;

            showForm = true;
        }

        // EDIT EMPLOYEE

        private void EditEmployee(Employee1 selectedEmployee)
        {
            employee = new Employee1
            {
                Id = selectedEmployee.Id,
                Name = selectedEmployee.Name,
                Email = selectedEmployee.Email,
                Age = selectedEmployee.Age,
                Salary = selectedEmployee.Salary,
                Department = selectedEmployee.Department,
                Team = selectedEmployee.Team
            };

            editingId = selectedEmployee.Id;

            Message = string.Empty;

            showForm = true;
        }

        // SAVE EMPLOYEE

        private async Task SaveEmployee()
        {
            if (IsBusy)
                return;

            if (editingId == 0 &&
                string.IsNullOrWhiteSpace(employeePassword))
            {
                Message = "Employee password is required.";

                IsSuccess = false;

                return;
            }

            IsBusy = true;

            Message = string.Empty;

            try
            {
                HttpResponseMessage response;

                if (editingId == 0)
                {
                    // CREATE EMPLOYEE AND LOGIN ACCOUNT

                    var request = new
                    {
                        Name = employee.Name,
                        Email = employee.Email,
                        Age = employee.Age,
                        Salary = employee.Salary,
                        Team = employee.Team,
                        Password = employeePassword
                    };

                    response = await HttpClient.PostAsync(
                        "api/Employees",
                        request);
                }
                else
                {
                    // UPDATE EMPLOYEE

                    response = await HttpClient.PutAsync(
                        $"api/Employees/{editingId}",
                        employee);
                }

                using (response)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content
                            .ReadAsStringAsync();

                        Message = string.IsNullOrWhiteSpace(error)
                            ? $"Save failed: HTTP {(int)response.StatusCode}"
                            : error;

                        IsSuccess = false;

                        return;
                    }
                }

                Message = editingId == 0
                    ? "Employee and login account created successfully."
                    : "Employee updated successfully.";

                IsSuccess = true;

                showForm = false;

                employee = new Employee1();

                employeePassword = string.Empty;

                editingId = 0;

                await LoadEmployees();
            }
            catch (Exception ex)
            {
                Message = $"Error saving employee: {ex.Message}";

                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // DELETE EMPLOYEE

        private async Task DeleteEmployee(int id)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            Message = string.Empty;

            try
            {
                using var response = await HttpClient.DeleteAsync(
                    $"api/Employees/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Message = "Employee deleted successfully.";

                    IsSuccess = true;

                    await LoadEmployees();
                }
                else
                {
                    var error = await response.Content
                        .ReadAsStringAsync();

                    Message = string.IsNullOrWhiteSpace(error)
                        ? $"Delete failed: HTTP {(int)response.StatusCode}"
                        : error;

                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"Error deleting employee: {ex.Message}";

                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // CANCEL FORM

        private void Cancel()
        {
            showForm = false;

            employee = new Employee1();

            employeePassword = string.Empty;

            editingId = 0;

            Message = string.Empty;
        }

        // LOGOUT

        private void Logout()
        {
            UserSession.Logout();

            Navigation.NavigateTo(
                "/login",
                forceLoad: true);
        }

        private void OpenPendingLeaves()
        {
            Navigation.NavigateTo("/manage-leaves");
        }
    }
}

