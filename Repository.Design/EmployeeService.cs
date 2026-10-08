using Microsoft.EntityFrameworkCore;
using Employee.Data.Data;
using Employee.Data.Models;

namespace Employee.BusinessLogic;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly EmployeeDbContext _context;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        EmployeeDbContext context)
    {
        _employeeRepository = employeeRepository;
        _context = context;
    }



    public async Task<List<Employee1>> GetAllAsync(
        int managerId)
    {
        var manager = await GetManagerAsync(managerId);

        if (manager == null)
        {
            return new List<Employee1>();
        }

        return await _employeeRepository
            .GetAllAsync(managerId);
    }




    public async Task<Employee1?> GetByIdAsync(
        int id,
        int managerId)
    {
        var manager = await GetManagerAsync(managerId);

        if (manager == null)
        {
            return null;
        }


        return await _employeeRepository
            .GetByIdAsync(id, managerId);
    }




    public async Task<Employee1> AddAsync(
        Employee1 employee,
        int managerId)
    {

        var manager = await GetManagerAsync(managerId);

        if (manager == null)
        {
            throw new UnauthorizedAccessException(
                "Only a Manager can create an employee.");
        }



        if (string.IsNullOrWhiteSpace(manager.Department))
        {
            throw new InvalidOperationException(
                "Manager department is not configured.");
        }



        if (!IsTeamAllowed(
                manager.Department,
                employee.Team))
        {
            throw new ArgumentException(
                GetTeamErrorMessage(
                    manager.Department));
        }



        employee.Id = 0;


        employee.ManagerId = manager.Id;

        employee.Department = manager.Department;



        return await _employeeRepository
            .AddAsync(employee);
    }




    public async Task<Employee1?> UpdateAsync(
        Employee1 employee,
        int managerId)
    {

        var manager = await GetManagerAsync(managerId);

        if (manager == null)
        {
            return null;
        }



        if (!IsTeamAllowed(
                manager.Department,
                employee.Team))
        {
            throw new ArgumentException(
                GetTeamErrorMessage(
                    manager.Department));
        }



        employee.ManagerId = manager.Id;



        employee.Department = manager.Department;


        return await _employeeRepository
            .UpdateAsync(
                employee,
                managerId);
    }




    public async Task<bool> DeleteAsync(
        int id,
        int managerId)
    {
        var manager = await GetManagerAsync(managerId);

        if (manager == null)
        {
            return false;
        }


        return await _employeeRepository
            .DeleteAsync(
                id,
                managerId);
    }



    private async Task<User?> GetManagerAsync(
        int managerId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Id == managerId &&
                x.Role == "Manager");
    }



    private bool IsTeamAllowed(
        string department,
        string team)
    {
        if (string.Equals(
                department,
                "HR",
                StringComparison.OrdinalIgnoreCase))
        {
            string[] allowedTeams =
            {
                "Designer",
                "Development",
                "Sales",
                "SEO"
            };

            return allowedTeams.Contains(
                team,
                StringComparer.OrdinalIgnoreCase);
        }


        if (string.Equals(
                department,
                "Technical",
                StringComparison.OrdinalIgnoreCase))
        {
            string[] allowedTeams =
            {
                "System Engineer",
                "DevOps"
            };

            return allowedTeams.Contains(
                team,
                StringComparer.OrdinalIgnoreCase);
        }


        return false;
    }


    private string GetTeamErrorMessage(
        string department)
    {
        if (string.Equals(
                department,
                "HR",
                StringComparison.OrdinalIgnoreCase))
        {
            return
                "HR manager can only use Designer, Development, Sales, or SEO.";
        }


        if (string.Equals(
                department,
                "Technical",
                StringComparison.OrdinalIgnoreCase))
        {
            return
                "Technical manager can only use System Engineer or DevOps.";
        }


        return
            "Manager department must be HR or Technical.";
    }
}


