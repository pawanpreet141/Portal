using Employee.Data.Data;
using Employee.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Employee.BusinessLogic
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly EmployeeDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public PasswordResetService(
            EmployeeDbContext context,
            IEmailService emailService,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
        }



        public async Task<bool> RequestOtpAsync(string email)
        {
            email = email.Trim();

            Console.WriteLine($"Forgot Password requested for: {email}");

            // Check whether email exists.
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email.Trim().ToLower() == email.ToLower());

            if (user == null)
            {
                Console.WriteLine("USER NOT FOUND IN DATABASE.");
                return false;
            }

            Console.WriteLine($"User found: {user.Name}");
            Console.WriteLine($"Role: {user.Role}");

            // Invalidate previous unused OTPs.
            var oldOtps = await _context.PasswordResetOtps
                .Where(x => x.UserId == user.Id && !x.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            // Generate six-digit OTP.
            var otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            var resetOtp = new PasswordResetOtp
            {
                UserId = user.Id,
                OtpHash = HashOtp(otp),
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false,
                IsUsed = false,
                Attempts = 0
            };

            _context.PasswordResetOtps.Add(resetOtp);

            await _context.SaveChangesAsync();

            var body =
                $"Hello {user.Name},\n\n" +
                $"Your password reset OTP is: {otp}\n\n" +
                "This OTP expires in 5 minutes.\n\n" +
                "If you did not request this, please ignore this email.";

            await _emailService.SendEmailAsync(
                user.Email,
                "Employee Management - Password Reset OTP",
                body);

            Console.WriteLine($"OTP EMAIL SENT: {user.Email}");

            return true;
        }

        // STEP 2: Verify OTP
        public async Task<string?> VerifyOtpAsync(
            string email,
            string otp)
        {
            email = email.Trim();
            otp = otp.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return null;

            var resetOtp = await _context.PasswordResetOtps
                .Where(x =>
                    x.UserId == user.Id &&
                    !x.IsUsed &&
                    !x.IsVerified)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetOtp == null)
                return null;

            // Check expiration.
            if (resetOtp.ExpiryTime <= DateTime.UtcNow)
                return null;

            // Maximum five attempts.
            if (resetOtp.Attempts >= 5)
            {
                resetOtp.IsUsed = true;
                await _context.SaveChangesAsync();
                return null;
            }

            // Increment attempt count.
            resetOtp.Attempts++;

            // Compare OTP hashes.
            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(HashOtp(otp)),
                Convert.FromHexString(resetOtp.OtpHash)))
            {
                if (resetOtp.Attempts >= 5)
                {
                    resetOtp.IsUsed = true;
                }

                await _context.SaveChangesAsync();
                return null;
            }

            // OTP successfully verified.
            resetOtp.IsVerified = true;

            // Generate secure reset token.
            var resetToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));

            // Store only reset token hash.
            resetOtp.OtpHash = HashOtp(resetToken);

            await _context.SaveChangesAsync();

            return resetToken;
        }

        // STEP 3: Reset password
        public async Task<bool> ResetPasswordAsync(
            string email,
            string resetToken,
            string newPassword)
        {
            email = email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return false;

            var resetOtp = await _context.PasswordResetOtps
                .Where(x =>
                    x.UserId == user.Id &&
                    x.IsVerified &&
                    !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetOtp == null)
                return false;

            // Check token expiration.
            if (resetOtp.ExpiryTime <= DateTime.UtcNow)
                return false;

            // Verify reset token.
            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(HashOtp(resetToken)),
                Convert.FromHexString(resetOtp.OtpHash)))
            {
                return false;
            }

            // Hash new password.
            user.PasswordHash =
                _passwordHasher.HashPassword(user, newPassword);

            // Mark OTP as used.
            resetOtp.IsUsed = true;

            await _context.SaveChangesAsync();

            return true;
        }

        // STEP 4: Hash OTP and reset token.
        private static string HashOtp(string value)
        {
            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(value));

            return Convert.ToHexString(bytes);
        }
    }
}
