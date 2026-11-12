using Employee.API.DTOs;
using Employee.BusinessLogic;
using Employee.Data.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Employee.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "HR,Technical")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IValidator<Employee1> _validator;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            IEmployeeService employeeService,
            IValidator<Employee1> validator,
            ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _validator = validator;
            _logger = logger;
        }

        // GET: api/Employees
        // Show only employees created by logged-in user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee1>>> GetEmployees()
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            var employees = await _employeeService
                .GetAllAsync(userId.Value);

            return Ok(employees);
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee1>> GetEmployee(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            var employee = await _employeeService
                .GetByIdAsync(id, userId.Value);

            if (employee == null)
            {
                return NotFound(
                    "Employee not found or you do not have access.");
            }

            return Ok(employee);
        }

        // POST: api/Employees
       
        [HttpPost]
        public async Task<ActionResult<Employee1>> CreateEmployee(
            [FromBody] CreateEmployeeRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Employee password is required."
                });
            }

            var employee = new Employee1
            {
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                Salary = request.Salary,
                Team = request.Team
            };

            var validationResult =
                await _validator.ValidateAsync(employee);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var result = await _employeeService.AddAsync(
                    employee,
                    userId.Value,
                    request.Password);

                return CreatedAtAction(
                    nameof(GetEmployee),
                    new { id = result.Id },
                    result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // PUT: api/Employees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            [FromBody] Employee1 employee)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            if (id != employee.Id)
                return BadRequest("Employee ID does not match.");

            var validationResult =
                await _validator.ValidateAsync(employee);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var result = await _employeeService
                    .UpdateAsync(employee, userId.Value);

                if (result == null)
                {
                    return NotFound(
                        "Employee not found or you do not have access.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized("User ID not found.");

            var result = await _employeeService
                .DeleteAsync(id, userId.Value);

            if (!result)
            {
                return NotFound(
                    "Employee not found or you do not have access.");
            }

            return NoContent();
        }

        // Get authenticated user's ID from JWT
        private int? GetUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null;
        }
    }
}
