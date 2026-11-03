using Employees.UI.Models;
using Employees.UI.Services;
using Microsoft.AspNetCore.Components;

namespace Employees.UI.Components.Pages
{
    public partial class Employee
    {
        public List<Employee1>? employees { get; set; }

        private Employee1 employee = new();

        private bool showForm;
        private bool IsBusy;
        private bool IsSuccess;
        private int editingId;

        private string Message = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            // Check login
            if (!UserSession.IsLoggedIn)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            // Only HR and Technical users can access this page
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

        // Load employees created by logged-in HR or Technical user
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

        // Open employee creation form
        private void ShowAddForm()
        {
            employee = new Employee1
            {
                Team = string.Empty
            };

            editingId = 0;
            Message = string.Empty;
            showForm = true;
        }

        // Edit employee
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

        // Save employee
        private async Task SaveEmployee()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            Message = string.Empty;

            try
            {
                HttpResponseMessage response;

                if (editingId == 0)
                {
                    // Create employee
                    response = await HttpClient.PostAsync(
                        "api/Employees",
                        employee);
                }
                else
                {
                    // Update employee
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
                    ? "Employee added successfully."
                    : "Employee updated successfully.";

                IsSuccess = true;

                showForm = false;

                employee = new Employee1();

                editingId = 0;

                // Refresh employee list
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

        // Delete employee
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

        // Cancel form
        private void Cancel()
        {
            showForm = false;

            employee = new Employee1();

            editingId = 0;

            Message = string.Empty;
        }

        // Logout
        private void Logout()
        {
            UserSession.Logout();

            Navigation.NavigateTo(
                "/login",
                forceLoad: true);
        }
    }
}






