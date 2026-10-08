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


        [HttpPost("signup")]
        public async Task<IActionResult> Signup(
            [FromBody] SignupRequest request)
        {
            _logger.LogInformation(
                "Signup attempt for email {Email}",
                request.Email);

            var validationResult =
                await _signupValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = validationResult.Errors
                        .First()
                        .ErrorMessage
                });
            }

            var emailExists =
                await _context.Users
                    .AnyAsync(x => x.Email == request.Email);

            if (emailExists)
            {
                _logger.LogWarning(
                    "Signup failed. Email already exists: {Email}",
                    request.Email);

                return BadRequest(
                    "Email already exists.");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,

                Role = "Employee",

                Department = string.Empty,

                AdminId = null,
                ManagerId = null
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);

            try
            {
                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Account created successfully. UserId {UserId}, Email {Email}",
                    user.Id,
                    user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating account for email {Email}",
                    request.Email);

                return BadRequest(new
                {
                    message =
                        "An error occurred while creating the account."
                });
            }

            return Ok(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                message = "Account created successfully."
            });
        }



        [Authorize(Roles = "Admin")]
        [HttpPost("create-manager")]
        public async Task<IActionResult> CreateManager(
            [FromBody] CreateManagerRequest request)
        {
            _logger.LogInformation(
                "Admin attempting to create manager with email {Email}",
                request.Email);

            var adminId = GetUserId();

            if (adminId == null)
            {
                return Unauthorized(
                    "Admin ID not found.");
            }


            var admin = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == adminId.Value);

            if (admin == null ||
                !string.Equals(
                    admin.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var validDepartments = new[]
            {
                "HR",
                "Technical"
            };

            if (!validDepartments.Contains(
                    request.Department,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Manager department must be HR or Technical."
                });
            }

            var emailExists =
                await _context.Users
                    .AnyAsync(x =>
                        x.Email == request.Email);

            if (emailExists)
            {
                return BadRequest(
                    "Email already exists.");
            }

            var manager = new User
            {
                Name = request.Name,

                Email = request.Email,

                Role = "Manager",

                Department = request.Department,


                AdminId = adminId.Value,

                ManagerId = null
            };

            manager.PasswordHash =
                _passwordHasher.HashPassword(
                    manager,
                    request.Password);

            try
            {
                _context.Users.Add(manager);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Manager created successfully. ManagerId {ManagerId}, AdminId {AdminId}",
                    manager.Id,
                    adminId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating Manager for AdminId {AdminId}",
                    adminId.Value);

                return BadRequest(new
                {
                    message =
                        "An error occurred while creating the manager."
                });
            }

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



        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            _logger.LogInformation(
                "Login attempt for email {Email}",
                request.Email);


            var validationResult =
                await _loginValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Login validation failed for email {Email}",
                    request.Email);

                return BadRequest(
                    validationResult.Errors.Select(x => new
                    {
                        field = x.PropertyName,
                        message = x.ErrorMessage
                    })
                );
            }


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning(
                    "Login failed. User not found for email {Email}",
                    request.Email);

                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (result ==
                PasswordVerificationResult.Failed)
            {
                _logger.LogWarning(
                    "Login failed. Invalid password for email {Email}",
                    request.Email);

                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation(
                "Login successful. UserId {UserId}, Role {Role}",
                user.Id,
                user.Role);

            return Ok(new
            {
                id = user.Id,

                name = user.Name,

                email = user.Email,

                role = user.Role,

                department = user.Department,

                token = token,

                message = "Login successful."
            });
        }


        private string GenerateJwtToken(User user)
        {
            var key =
                _configuration["Jwt:Key"];

            var issuer =
                _configuration["Jwt:Issuer"];

            var audience =
                _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "JWT key is not configured.");
            }

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),


                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            // Add Department only when it has a value
            if (!string.IsNullOrWhiteSpace(user.Department))
            {
                claims.Add(
                    new Claim(
                        "Department",
                        user.Department));
            }

            var tokenDescriptor =
                new SecurityTokenDescriptor
                {
                    Subject =
                        new ClaimsIdentity(claims),

                    Expires =
                        DateTime.UtcNow.AddHours(2),

                    Issuer = issuer,

                    Audience = audience,

                    SigningCredentials =
                        credentials
                };

            var tokenHandler =
                new JwtSecurityTokenHandler();

            var token =
                tokenHandler.CreateToken(
                    tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        private int? GetUserId()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                userIdClaim =
                    User.FindFirst("sub")?.Value;
            }

            if (int.TryParse(
                    userIdClaim,
                    out int userId))
            {
                return userId;
            }

            return null;
        }


        public class SignupRequest
        {
            public string Name { get; set; }
                = string.Empty;

            public string Email { get; set; }
                = string.Empty;

            public string Department { get; set; }
                = string.Empty;

            public string Password { get; set; }
                = string.Empty;
        }


        public class LoginRequest
        {
            public string Email { get; set; }
                = string.Empty;

            public string Password { get; set; }
                = string.Empty;
        }

        public class CreateManagerRequest
        {
            public string Name { get; set; }
                = string.Empty;

            public string Email { get; set; }
                = string.Empty;

            public string Department { get; set; }
                = string.Empty;

            public string Password { get; set; }
                = string.Empty;
        }
    }
}
