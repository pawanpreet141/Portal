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


        public async Task<List<Employee1>> GetAllAsync(
            int managerId)
        {
            return await _context.Employees
                .Include(e => e.Manager)
                .Include(e => e.User)
                .Where(e => e.ManagerId == managerId)
                .ToListAsync();
        }


        public async Task<Employee1?> GetByIdAsync(
            int id,
            int managerId)
        {
            return await _context.Employees
                .Include(e => e.Manager)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.ManagerId == managerId);
        }



        public async Task<Employee1> AddAsync(
            Employee1 employee)
        {
            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            return employee;
        }


        public async Task<Employee1?> UpdateAsync(
            Employee1 employee,
            int managerId)
        {
            var existingEmployee =
                await _context.Employees
                    .FirstOrDefaultAsync(e =>
                        e.Id == employee.Id &&
                        e.ManagerId == managerId);

            if (existingEmployee == null)
            {
                return null;
            }


            existingEmployee.Name =
                employee.Name;

            existingEmployee.Email =
                employee.Email;

            existingEmployee.Age =
                employee.Age;

            existingEmployee.Department =
                employee.Department;

            existingEmployee.Salary =
                employee.Salary;

            existingEmployee.Team =
                employee.Team;


            await _context.SaveChangesAsync();

            return existingEmployee;
        }



        public async Task<bool> DeleteAsync(
            int id,
            int managerId)
        {
            var employee =
                await _context.Employees
                    .FirstOrDefaultAsync(e =>
                        e.Id == id &&
                        e.ManagerId == managerId);

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

