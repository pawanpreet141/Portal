using Employee.Data.Data;
using Employee.Data.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Employee.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly EmployeeDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IValidator<SignupRequest> _signupValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            EmployeeDbContext context,
            IConfiguration configuration,
            IValidator<SignupRequest> signupValidator,
            IValidator<LoginRequest> loginValidator,
            ILogger<AccountController> logger)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
            _signupValidator = signupValidator;
            _loginValidator = loginValidator;
            _logger = logger;
        }

        //HR and Technical  CREATES EMPLOYEE ACCOUNTS




        [Authorize(Roles = "HR,Technical")]
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(
            [FromBody] SignupRequest request)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized(new { message = "User ID not found." });

            // Find the logged-in HR or Technical user.
            var creator = await _context.Users.FirstOrDefaultAsync(
                x => x.Id == userId.Value);

            if (creator == null)
                return Unauthorized(new { message = "User not found." });

            string[] allowedTeams;

            if (string.Equals(
                creator.Role, "HR",
                StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                creator.Department, "HR",
                StringComparison.OrdinalIgnoreCase))
            {
                allowedTeams = new[]
                {
            "Development", "Designer", "Sales", "SEO"
        };
            }
            else if (string.Equals(
                creator.Role, "Technical",
                StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                creator.Department, "Technical",
                StringComparison.OrdinalIgnoreCase))
            {
                allowedTeams = new[]
                {
            "System Engineer", "DevOps"
        };
            }
            else
            {
                return Forbid();
            }

            var validation = await _signupValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    message = validation.Errors.First().ErrorMessage
                });
            }

            var selectedTeam = allowedTeams.FirstOrDefault(team =>
                string.Equals(
                    team,
                    request.Department?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (selectedTeam == null)
            {
                return BadRequest(new
                {
                    message = "You are not authorized to create an employee for this team."
                });
            }

            var email = request.Email.Trim();

            if (await _context.Users.AnyAsync(x => x.Email == email))
            {
                return BadRequest(new
                {
                    message = "Email already exists."
                });
            }

            var employee = new User
            {
                Name = request.Name.Trim(),
                Email = email,
                Role = "Employee",
                Department = selectedTeam,

                // Preserve the original Manager and Admin relationships.
                ManagerId = creator.ManagerId ?? creator.Id,
                AdminId = creator.AdminId
            };

            employee.PasswordHash = _passwordHasher.HashPassword(
                employee, request.Password);

            _context.Users.Add(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Employee created: {UserId}, by {CreatorId}",
                employee.Id, creator.Id);

            return Ok(new
            {
                id = employee.Id,
                name = employee.Name,
                email = employee.Email,
                role = employee.Role,
                department = employee.Department,
                managerId = employee.ManagerId,
                message = "Employee account created successfully."
            });
        }



        //[Authorize(Roles = "Manager")]
        //[HttpPost("signup")]
        //public async Task<IActionResult> Signup(
        //    [FromBody] SignupRequest request)
        //{
        //    var managerId = GetUserId();

        //    if (managerId == null)
        //        return Unauthorized(new { message = "Manager ID not found." });

        //    var manager = await _context.Users.FirstOrDefaultAsync(
        //        x => x.Id == managerId.Value && x.Role == "Manager");

        //    if (manager == null)
        //        return Forbid();

        //    var validation = await _signupValidator.ValidateAsync(request);

        //    if (!validation.IsValid)
        //    {
        //        return BadRequest(new
        //        {
        //            message = validation.Errors.First().ErrorMessage
        //        });
        //    }

        //    string[] allowedTeams;

        //    if (string.Equals(
        //        manager.Department, "HR",
        //        StringComparison.OrdinalIgnoreCase))
        //    {
        //        allowedTeams = new[]
        //        {
        //            "Development", "Designer", "Sales", "SEO"
        //        };
        //    }
        //    else if (string.Equals(
        //        manager.Department, "Technical",
        //        StringComparison.OrdinalIgnoreCase))
        //    {
        //        allowedTeams = new[]
        //        {
        //            "System Engineer", "DevOps"
        //        };
        //    }
        //    else
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Invalid manager department."
        //        });
        //    }

        //    var selectedTeam = allowedTeams.FirstOrDefault(team =>
        //        string.Equals(
        //            team, request.Department?.Trim(),
        //            StringComparison.OrdinalIgnoreCase));

        //    if (selectedTeam == null)
        //    {
        //        return BadRequest(new
        //        {
        //            message = "You are not authorized to create an employee for this team."
        //        });
        //    }

        //    var email = request.Email.Trim();

        //    if (await _context.Users.AnyAsync(x => x.Email == email))
        //    {
        //        return BadRequest(new { message = "Email already exists." });
        //    }

        //    var user = new User
        //    {
        //        Name = request.Name.Trim(),
        //        Email = email,
        //        Role = "Employee",
        //        Department = selectedTeam,
        //        ManagerId = manager.Id,
        //        AdminId = manager.AdminId
        //    };

        //    user.PasswordHash = _passwordHasher.HashPassword(
        //        user, request.Password);

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    _logger.LogInformation(
        //        "Employee created: {UserId}, manager: {ManagerId}",
        //        user.Id, manager.Id);

        //    return Ok(new
        //    {
        //        id = user.Id,
        //        name = user.Name,
        //        email = user.Email,
        //        role = user.Role,
        //        department = user.Department,
        //        managerId = user.ManagerId,
        //        message = "Employee account created successfully."
        //    });
        //}

        // ADMIN CREATES MANAGERS

        //[Authorize(Roles = "Admin")]
        //[HttpPost("create-manager")]
        //public async Task<IActionResult> CreateManager(
        //    [FromBody] CreateManagerRequest request)
        //{
        //    var adminId = GetUserId();

        //    if (adminId == null)
        //        return Unauthorized(new { message = "Admin ID not found." });

        //    var admin = await _context.Users.FirstOrDefaultAsync(
        //        x => x.Id == adminId.Value && x.Role == "Admin");

        //    if (admin == null)
        //        return Forbid();

        //    if (string.IsNullOrWhiteSpace(request.Name) ||
        //        string.IsNullOrWhiteSpace(request.Email) ||
        //        string.IsNullOrWhiteSpace(request.Password))
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Name, email and password are required."
        //        });
        //    }

        //    var department = request.Department?.Trim();

        //    if (department == null ||
        //        !(department.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
        //          department.Equals("Technical", StringComparison.OrdinalIgnoreCase)))
        //    {
        //        return BadRequest(new
        //        {
        //            message = "Manager department must be HR or Technical."
        //        });
        //    }

        //    if (await _context.Users.AnyAsync(
        //        x => x.Email == request.Email.Trim()))
        //    {
        //        return BadRequest(new { message = "Email already exists." });
        //    }

        //    var manager = new User
        //    {
        //        Name = request.Name.Trim(),
        //        Email = request.Email.Trim(),
        //        Role = "Manager",
        //        Department = department.Equals(
        //            "HR", StringComparison.OrdinalIgnoreCase)
        //            ? "HR"
        //            : "Technical",
        //        AdminId = admin.Id,
        //        ManagerId = null
        //    };

        //    manager.PasswordHash = _passwordHasher.HashPassword(
        //        manager, request.Password);

        //    _context.Users.Add(manager);
        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        id = manager.Id,
        //        name = manager.Name,
        //        email = manager.Email,
        //        role = manager.Role,
        //        department = manager.Department,
        //        adminId = manager.AdminId,
        //        message = "Manager created successfully."
        //    });
        //}



        [Authorize(Roles = "Admin")]
        [HttpPost("create-manager")]
        public async Task<IActionResult> CreateManager(
            [FromBody] CreateManagerRequest request)
        {
            var adminId = GetUserId();

            if (adminId == null)
                return Unauthorized(new { message = "Admin ID not found." });

            var admin = await _context.Users.FirstOrDefaultAsync(
                x => x.Id == adminId.Value && x.Role == "Admin");

            if (admin == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Name, email and password are required."
                });
            }

            var email = request.Email.Trim();

            if (await _context.Users.AnyAsync(x => x.Email == email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var manager = new User
            {
                Name = request.Name.Trim(),
                Email = email,
                Role = "Manager",
                Department = string.Empty,
                AdminId = admin.Id,
                ManagerId = null
            };

            manager.PasswordHash = _passwordHasher.HashPassword(
                manager, request.Password);

            _context.Users.Add(manager);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = manager.Id,
                name = manager.Name,
                email = manager.Email,
                role = manager.Role,
                department = manager.Department,
                adminId = manager.AdminId,
                message = "Manager created successfully."
            });
        }


        //  MANAGER CREATES HR OR TECHNICAL ACCOUNTS

        [Authorize(Roles = "Manager")]
        [HttpPost("create-department-account")]
        public async Task<IActionResult> CreateDepartmentAccount(
            [FromBody] CreateDepartmentAccountRequest request)
        {
            var managerId = GetUserId();

            if (managerId == null)
                return Unauthorized(new { message = "Manager ID not found." });

            var manager = await _context.Users.FirstOrDefaultAsync(
                x => x.Id == managerId.Value && x.Role == "Manager");

            if (manager == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Name, email and password are required."
                });
            }

            var requestedDepartment = request.Department?.Trim();

            if (string.IsNullOrWhiteSpace(requestedDepartment) ||
                !(requestedDepartment.Equals(
                      "HR", StringComparison.OrdinalIgnoreCase) ||
                  requestedDepartment.Equals(
                      "Technical", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    message = "Department must be HR or Technical."
                });
            }

            //// A manager can create accounts only in their own department.
            //if (!string.Equals(
            //    manager.Department?.Trim(),
            //    requestedDepartment,
            //    StringComparison.OrdinalIgnoreCase))
            //{
            //    return BadRequest(new
            //    {
            //        message = "You can create accounts only for your own department."
            //    });
            //}

            var email = request.Email.Trim();

            if (await _context.Users.AnyAsync(x => x.Email == email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var department = requestedDepartment.Equals(
                "HR", StringComparison.OrdinalIgnoreCase)
                ? "HR"
                : "Technical";


            var assignedRole = department == "HR"
                ? "HR"
                : "Technical";

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = email,
                Role = assignedRole,
                Department = department,
                AdminId = manager.AdminId,
                ManagerId = manager.Id
            };




            //var user = new User
            //{
            //    Name = request.Name.Trim(),
            //    Email = email,
            //    Role = "Employee",
            //    Department = department,
            //    AdminId = manager.AdminId,
            //    ManagerId = manager.Id
            //};

            user.PasswordHash = _passwordHasher.HashPassword(
                user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "{Role} account created with ID {UserId}",
                user.Role, user.Id);

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                department = user.Department,
                managerId = user.ManagerId,
                message = $"{user.Role} account created successfully."
            });
        }




        //  LOGIN FOR ALL ACCOUNT TYPES


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var validation = await _loginValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    message = validation.Errors.First().ErrorMessage
                });
            }

            var email = request.Email.Trim();

            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.Email == email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                department = user.Department,
                token,
                message = "Login successful."
            });
        }

        // 5. JWT TOKEN GENERATION

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException(
                    "JWT key is not configured.");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (!string.IsNullOrWhiteSpace(user.Department))
            {
                claims.Add(new Claim("Department", user.Department));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }


        // 6. GET LOGGED-IN USER ID

        private int? GetUserId()
        {
            var value = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                value = User.FindFirst("sub")?.Value;

            return int.TryParse(value, out var id) ? id : null;
        }

        // 7. REQUEST MODELS

        public class SignupRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class CreateManagerRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
          //  public string Department { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class CreateDepartmentAccountRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
        }
    }
}