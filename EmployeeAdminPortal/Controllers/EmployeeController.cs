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
    [Authorize(Roles = "Manager")]
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


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee1>>> GetEmployees()
        {
            var managerId = GetUserId();

            if (managerId == null)
            {
                return Unauthorized("User ID not found.");
            }

            _logger.LogInformation(
                "Getting employees for ManagerId {ManagerId}",
                managerId.Value);

            var employees = await _employeeService
                .GetAllAsync(managerId.Value);

            return Ok(employees);
        }




        [HttpGet("{id}")]
        public async Task<ActionResult<Employee1>> GetEmployee(int id)
        {
            var managerId = GetUserId();

            if (managerId == null)
            {
                return Unauthorized("User ID not found.");
            }

            _logger.LogInformation(
                "Getting EmployeeId {EmployeeId} for ManagerId {ManagerId}",
                id,
                managerId.Value);

            var employee = await _employeeService
                .GetByIdAsync(id, managerId.Value);

            if (employee == null)
            {
                return NotFound(
                    "Employee not found or you do not have access to this employee.");
            }

            return Ok(employee);
        }




        [HttpPost]
        public async Task<ActionResult<Employee1>> CreateEmployee(
            [FromBody] CreateEmployeeRequest request)
        {
            var managerId = GetUserId();

            if (managerId == null)
            {
                return Unauthorized("User ID not found.");
            }

            _logger.LogInformation(
                "Creating employee for ManagerId {ManagerId}",
                managerId.Value);


            var employee = new Employee1
            {
                Id = 0,
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                Salary = request.Salary,
                Team = request.Team
            };


            var validationResult =
                await _validator.ValidateAsync(employee);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                // ManagerId comes from JWT.
                var result = await _employeeService
                    .AddAsync(employee, managerId.Value);

                return CreatedAtAction(
                    nameof(GetEmployee),
                    new
                    {
                        id = result.Id
                    },
                    result);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    "Unauthorized employee creation attempt by UserId {UserId}",
                    managerId.Value);

                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            [FromBody] Employee1 employee)
        {
            var managerId = GetUserId();

            if (managerId == null)
            {
                return Unauthorized("User ID not found.");
            }

            _logger.LogInformation(
                "Updating EmployeeId {EmployeeId} for ManagerId {ManagerId}",
                id,
                managerId.Value);

            if (id != employee.Id)
            {
                return BadRequest("Employee ID does not match.");
            }

            var validationResult =
                await _validator.ValidateAsync(employee);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _employeeService
                    .UpdateAsync(employee, managerId.Value);

                if (result == null)
                {
                    return NotFound(
                        "Employee not found or you do not have access to this employee.");
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var managerId = GetUserId();

            if (managerId == null)
            {
                return Unauthorized("User ID not found.");
            }

            _logger.LogInformation(
                "Deleting EmployeeId {EmployeeId} for ManagerId {ManagerId}",
                id,
                managerId.Value);

            var result = await _employeeService
                .DeleteAsync(id, managerId.Value);

            if (!result)
            {
                return NotFound(
                    "Employee not found or you do not have access to this employee.");
            }

            return NoContent();
        }


        private int? GetUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                userIdClaim =
                    User.FindFirst("sub")?.Value;
            }

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }

        [HttpGet("test-exception")]
        public IActionResult TestException()
        {
            throw new Exception("Test exception");
        }
    }
}