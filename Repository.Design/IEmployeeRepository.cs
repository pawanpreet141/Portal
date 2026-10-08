using Employee.Data.Models;

namespace Employee.BusinessLogic;

public interface IEmployeeRepository
{
    Task<List<Employee1>> GetAllAsync(int managerId);

    Task<Employee1?> GetByIdAsync(
        int id,
        int managerId);

    Task<Employee1> AddAsync(Employee1 employee);

    Task<Employee1?> UpdateAsync(
        Employee1 employee,
        int managerId);

    Task<bool> DeleteAsync(
        int id,
        int managerId);
}
