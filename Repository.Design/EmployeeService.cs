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

    // Get employees created by logged-in HR or Technical
    public async Task<List<Employee1>> GetAllAsync(int userId)
    {
        var creator = await GetCreatorAsync(userId);

        if (creator == null)
            return new List<Employee1>();

        return await _employeeRepository.GetAllAsync(userId);
    }

    // Get employee by ID belonging to logged-in user
    public async Task<Employee1?> GetByIdAsync(
        int id,
        int userId)
    {
        var creator = await GetCreatorAsync(userId);

        if (creator == null)
            return null;

        return await _employeeRepository.GetByIdAsync(id, userId);
    }

    // Create employee
    public async Task<Employee1> AddAsync(
        Employee1 employee,
        int userId)
    {
        var creator = await GetCreatorAsync(userId);

        if (creator == null)
        {
            throw new UnauthorizedAccessException(
                "Only HR and Technical users can create employees.");
        }

        if (!IsTeamAllowed(creator.Role, employee.Team))
        {
            throw new ArgumentException(
                GetTeamErrorMessage(creator.Role));
        }



        employee.Id = 0;

        // Save logged-in HR or Technical user ID
        employee.UserId = creator.Id;

        employee.ManagerId = creator.ManagerId;

        employee.Department = employee.Team;

        // Save employee creation date and time
        // employee.CreatedAt = DateTime.UtcNow;

        employee.CreatedAt = DateTime.UtcNow.AddHours(5).AddMinutes(30);

        return await _employeeRepository.AddAsync(employee);
    }

    // Update only employees created by logged-in user
    public async Task<Employee1?> UpdateAsync(
        Employee1 employee,
        int userId)
    {
        var creator = await GetCreatorAsync(userId);

        if (creator == null)
            return null;

        if (!IsTeamAllowed(creator.Role, employee.Team))
        {
            throw new ArgumentException(
                GetTeamErrorMessage(creator.Role));
        }

        employee.Department = employee.Team;

        return await _employeeRepository.UpdateAsync(
            employee,
            userId);
    }

    // Delete only employees created by logged-in user
    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var creator = await GetCreatorAsync(userId);

        if (creator == null)
            return false;

        return await _employeeRepository.DeleteAsync(
            id,
            userId);
    }

    // Find logged-in HR or Technical user
    private async Task<User?> GetCreatorAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(x =>
            x.Id == userId &&
            (x.Role == "HR" || x.Role == "Technical"));
    }

    // Department validation
    private bool IsTeamAllowed(string role, string team)
    {
        if (string.Equals(
            role, "HR", StringComparison.OrdinalIgnoreCase))
        {
            string[] allowedTeams =
            {
                "Designer",
                "Development",
                "Sales",
                "SEO"
            };

            return allowedTeams.Contains(
                team, StringComparer.OrdinalIgnoreCase);
        }

        if (string.Equals(
            role, "Technical", StringComparison.OrdinalIgnoreCase))
        {
            string[] allowedTeams =
            {
                "DevOps",
                "System Engineer"
            };

            return allowedTeams.Contains(
                team, StringComparer.OrdinalIgnoreCase);
        }

        return false;
    }

    private string GetTeamErrorMessage(string role)
    {
        if (string.Equals(
            role, "HR", StringComparison.OrdinalIgnoreCase))
        {
            return "HR can create Designer, Development, Sales, or SEO employees.";
        }

        if (string.Equals(
            role, "Technical", StringComparison.OrdinalIgnoreCase))
        {
            return "Technical can create DevOps or System Engineer employees.";
        }

        return "Invalid user role.";
    }
}
