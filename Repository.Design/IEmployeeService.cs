using Employee.Data.Models;

namespace Employee.BusinessLogic;

public interface IEmployeeService
{
    Task<List<Employee1>> GetAllAsync(int userId);

    Task<Employee1?> GetByIdAsync(
        int id,
        int userId);

    Task<Employee1> AddAsync(
        Employee1 employee,
        int userId);

    Task<Employee1?> UpdateAsync(
        Employee1 employee,
        int userId);

    Task<bool> DeleteAsync(
        int id,
        int userId);
}
