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
            if (!UserSession.IsLoggedIn)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            await LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                employees = await HttpClient.GetAsync<List<Employee1>>(
                    $"api/Employees?userId={UserSession.UserId}")
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

        private void ShowAddForm()
        {
            employee = new Employee1
            {
                UserId = UserSession.UserId,
                Team = string.Empty
            };

            editingId = 0;
            Message = string.Empty;
            showForm = true;
        }

        private void EditEmployee(Employee1 selectedEmployee)
        {
            employee = new Employee1
            {
                Id = selectedEmployee.Id,
                Name = selectedEmployee.Name,
                Email = selectedEmployee.Email,
                Salary = selectedEmployee.Salary,
                Department = selectedEmployee.Department,
                Team = selectedEmployee.Team,
                Age = selectedEmployee.Age,
                UserId = UserSession.UserId
            };

            editingId = selectedEmployee.Id;
            Message = string.Empty;
            showForm = true;
        }

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
                    response = await HttpClient.PostAsync(
                        $"api/Employees?userId={UserSession.UserId}",
                        employee);
                }
                else
                {
                    response = await HttpClient.PutAsync(
                        $"api/Employees/{editingId}?userId={UserSession.UserId}",
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

        private async Task DeleteEmployee(int id)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            Message = string.Empty;

            try
            {
                using var response = await HttpClient.DeleteAsync(
                    $"api/Employees/{id}?userId={UserSession.UserId}");

                if (response.IsSuccessStatusCode)
                {
                    Message = "Employee deleted successfully.";
                    IsSuccess = true;
                    await LoadEmployees();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

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

        private void Cancel()
        {
            showForm = false;
            employee = new Employee1();
            editingId = 0;
            Message = string.Empty;
        }

        private void Logout()
        {
            UserSession.Logout();
            Navigation.NavigateTo("/login", forceLoad: true);
        }
    }
}




