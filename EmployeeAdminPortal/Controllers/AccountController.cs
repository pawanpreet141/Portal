using Employee.BusinessLogic;
using Employee.Data.Data;
using Employee.Data.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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

        //13/11
        private readonly IPasswordResetService _passwordResetService;

        public AccountController(
            EmployeeDbContext context,
            IConfiguration configuration,
            IValidator<SignupRequest> signupValidator,
            IValidator<LoginRequest> loginValidator,
            ILogger<AccountController> logger,
            IPasswordResetService passwordResetService)

        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
            _signupValidator = signupValidator;
            _loginValidator = loginValidator;
            _logger = logger;
            _passwordResetService = passwordResetService;
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

            var creator = await _context.Users.FirstOrDefaultAsync(
                x => x.Id == userId.Value);

            if (creator == null)
                return Unauthorized(new { message = "User not found." });

            string[] allowedDepartments;

            if (string.Equals(
                creator.Role, "HR",
                StringComparison.OrdinalIgnoreCase))
            {
                allowedDepartments = new[]
                {
            "Developer",
            "Development",
            "Sales",
            "SEO"
        };
            }
            else if (string.Equals(
                creator.Role, "Technical",
                StringComparison.OrdinalIgnoreCase))
            {
                allowedDepartments = new[]
                {
            "DevOps",
            "System Engineer"
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

            var selectedDepartment = allowedDepartments.FirstOrDefault(
                x => string.Equals(
                    x,
                    request.Department?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (selectedDepartment == null)
            {
                return BadRequest(new
                {
                    message = "Invalid department for your role."
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

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Create employee login account
                var employeeUser = new User
                {
                    Name = request.Name.Trim(),
                    Email = email,
                    Role = "Employee",
                    Department = selectedDepartment,

                    AdminId = creator.AdminId,
                    ManagerId = creator.ManagerId
                };

                employeeUser.PasswordHash =
                    _passwordHasher.HashPassword(
                        employeeUser,
                        request.Password);

                _context.Users.Add(employeeUser);

                await _context.SaveChangesAsync();

                // Create employee profile
                //var employeeProfile = new Employee1
                //{
                //    Name = employeeUser.Name,
                //    Email = employeeUser.Email,
                //    Department = selectedDepartment,
                //    Team = selectedDepartment,

                //    // ID of HR or Technical who created employee
                //    UserId = creator.Id,

                //    // Preserve manager relationship
                //    ManagerId = employeeUser.ManagerId
                //};
                var employeeProfile = new Employee1
                {
                    Name = employeeUser.Name,
                    Email = employeeUser.Email,
                    Department = selectedDepartment,
                    Team = selectedDepartment,

                    // HR or Technical creator ID
                    UserId = creator.Id,

                    // Employee's own login ID
                    LoginUserId = employeeUser.Id,

                    ManagerId = employeeUser.ManagerId,

                    CreatedAt = DateTime.UtcNow
                };






                _context.Employees.Add(employeeProfile);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Employee {EmployeeId} created by user {CreatorId}",
                    employeeProfile.Id,
                    creator.Id);

                return Ok(new
                {
                    id = employeeProfile.Id,
                    userId = employeeUser.Id,
                    name = employeeProfile.Name,
                    email = employeeProfile.Email,
                    department = employeeProfile.Department,
                    message = "Employee created successfully."
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }




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



        //FORGOT PASSWORD - SEND OTP


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            try
            {
                var result = await _passwordResetService.RequestOtpAsync(
                    request.Email.Trim());

                if (result.UserNotFound)
                {
                    return NotFound(new
                    {
                        message = "Email is not registered."
                    });
                }

                if (result.IsRateLimited)
                {
                    return StatusCode(429, new
                    {
                        message = "Please wait before requesting another OTP.",
                        retryAfterSeconds = result.RetryAfterSeconds
                    });
                }

                if (!result.Success)
                {
                    return StatusCode(500, new
                    {
                        message = "Unable to send OTP. Please try again."
                    });
                }

                return Ok(new
                {
                    message = "OTP sent successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process password reset request.");

                return StatusCode(500, new
                {
                    message = "Unable to send OTP. Please try again."
                });
            }
        }

        // RESEND OTP
        [AllowAnonymous]
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(
            [FromBody] ForgotPasswordRequest request)
        {
            return await SendPasswordOtp(request.Email);
        }



        [AllowAnonymous]
        [HttpGet("otp-resend-status")]
        public async Task<IActionResult> GetOtpResendStatus(
            [FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var seconds =
                await _passwordResetService.GetResendWaitSecondsAsync(email);

            return Ok(new
            {
                retryAfterSeconds = seconds
            });
        }




        // STEP 2: VERIFY OTP

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new
                {
                    message = "Email and OTP are required."
                });
            }

            if (request.Otp.Length != 6 ||
                !request.Otp.All(char.IsDigit))
            {
                return BadRequest(new
                {
                    message = "Please enter a valid six-digit OTP."
                });
            }

            var resetToken = await _passwordResetService.VerifyOtpAsync(
                request.Email.Trim(),
                request.Otp.Trim());

            if (resetToken == null)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired OTP."
                });
            }

            return Ok(new
            {
                resetToken,
                message = "OTP verified successfully."
            });
        }



        // STEP 3: RESET PASSWORD

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.ResetToken) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new
                {
                    message = "All fields are required."
                });
            }

            if (request.NewPassword.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new
                {
                    message = "Passwords do not match."
                });
            }

            var result = await _passwordResetService.ResetPasswordAsync(
                request.Email.Trim(),
                request.ResetToken,
                request.NewPassword);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Password reset failed. Please verify your OTP again."
                });
            }

            _logger.LogInformation(
                "Password reset completed for account.");

            return Ok(new
            {
                message = "Password changed successfully. Please login."
            });
        }


        // COMMON OTP SENDING METHOD 19/11
        private async Task<IActionResult> SendPasswordOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            try
            {
                var result =
                    await _passwordResetService.RequestOtpAsync(email.Trim());

                if (result.UserNotFound)
                {
                    return NotFound(new
                    {
                        message = "Email is not registered."
                    });
                }

                if (result.IsRateLimited)
                {
                    return StatusCode(429, new
                    {
                        message = "Please wait before requesting another OTP.",
                        retryAfterSeconds = result.RetryAfterSeconds
                    });
                }

                if (!result.Success)
                {
                    return StatusCode(500, new
                    {
                        message = "Unable to send OTP. Please try again."
                    });
                }

                return Ok(new
                {
                    message = "OTP sent successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send password reset OTP.");

                return StatusCode(500, new
                {
                    message = "Unable to send OTP. Please try again."
                });
            }
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




        // MANAGER DASHBOARD - GET HR AND TECHNICAL USERS

        [Authorize(Roles = "Manager")]
        [HttpGet("manager-dashboard")]
        public async Task<IActionResult> GetManagerDashboard()
        {
            // Get logged-in manager ID from JWT token
            var managerId = GetUserId();

            if (managerId == null)
                return Unauthorized(new
                {
                    message = "Manager ID not found."
                });

            // Get HR and Technical users created by this manager
            var users = await _context.Users
                .Where(x =>
                    x.ManagerId == managerId.Value &&
                    (x.Role == "HR" || x.Role == "Technical"))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Role
                })
                .ToListAsync();

            // Separate HR and Technical users
            var hrUsers = users
                .Where(x => x.Role == "HR")
                .ToList();

            var technicalUsers = users
                .Where(x => x.Role == "Technical")
                .ToList();

            // Return dashboard information
            return Ok(new
            {
                TotalHR = hrUsers.Count,
                TotalTechnical = technicalUsers.Count,
                TotalCreated = users.Count,

                HRUsers = hrUsers,
                TechnicalUsers = technicalUsers
            });
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
            public string Password { get; set; } = string.Empty;
        }

        public class CreateDepartmentAccountRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
        }


        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class VerifyOtpRequest
        {
            public string Email { get; set; } = string.Empty;

            public string Otp { get; set; } = string.Empty;
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; } = string.Empty;

            public string ResetToken { get; set; } = string.Empty;

            public string NewPassword { get; set; } = string.Empty;

            public string ConfirmPassword { get; set; } = string.Empty;
        }

    }
}