using Employee.Data.Models;

namespace Employee.BusinessLogic;

public interface IEmployeeRepository
{
    // Get employees created by logged-in HR or Technical user
    Task<List<Employee1>> GetAllAsync(int userId);

    // Get one employee created by logged-in user
    Task<Employee1?> GetByIdAsync(
        int id,
        int userId);

    // Create employee
    Task<Employee1> AddAsync(Employee1 employee);

    // Update only employee created by logged-in user
    Task<Employee1?> UpdateAsync(
        Employee1 employee,
        int userId);

    // Delete only employee created by logged-in user
    Task<bool> DeleteAsync(
        int id,
        int userId);

    // Get employee by ID for leave management
    Task<Employee1?> GetEmployeeForLeaveAsync(int id);

    Task<Employee1?> GetEmployeeByLoginUserIdAsync(int loginUserId);
}
