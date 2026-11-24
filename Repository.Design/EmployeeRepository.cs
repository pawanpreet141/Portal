using Employee.Data.Data;
using Employee.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Employee.BusinessLogic
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeDbContext _context;

        public EmployeeRepository(EmployeeDbContext context)
        {
            _context = context;
        }

        // Get only employees created by logged-in HR or Technical
        public async Task<List<Employee1>> GetAllAsync(int userId)
        {
            return await _context.Employees
                //.Where(e => e.UserId == userId)
                .Where(e => e.CreatedBy == userId)
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }

        // Get employee by ID and creator ID
        public async Task<Employee1?> GetByIdAsync(
            int id,
            int userId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e =>
                    //e.Id == id &&
                    //e.UserId == userId);
                    e.Id == id &&
                    e.CreatedBy == userId);
        }

        // Create employee
        public async Task<Employee1> AddAsync(Employee1 employee)
        {
            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            return employee;
        }

        
        public async Task<Employee1?> GetEmployeeForLeaveAsync(int id)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // Update only employees created by logged-in user
        public async Task<Employee1?> GetEmployeeByLoginUserIdAsync(
    int loginUserId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == loginUserId);
        }


        // Update only employees created by logged-in user
        public async Task<Employee1?> UpdateAsync(
            Employee1 employee,
            int userId)
        {
            var existingEmployee =
                await _context.Employees
                    .FirstOrDefaultAsync(e =>
                        e.Id == employee.Id &&
                        e.CreatedBy == userId);

            if (existingEmployee == null)
            {
                return null;
            }

            existingEmployee.Name = employee.Name;
            existingEmployee.Email = employee.Email;
            existingEmployee.Age = employee.Age;
            existingEmployee.Department = employee.Department;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.Team = employee.Team;

            existingEmployee.UpdatedAt = DateTime.UtcNow
             .AddHours(5)
             .AddMinutes(30);

            await _context.SaveChangesAsync();

            return existingEmployee;
        }

        // Delete only employees created by logged-in user
        public async Task<bool> DeleteAsync(
            int id,
            int userId)
        {
            var employee =
                await _context.Employees
                    .FirstOrDefaultAsync(e =>
                        e.Id == id &&
                        e.CreatedBy == userId);

            if (employee == null)
            {
                return false;
            }

            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}


