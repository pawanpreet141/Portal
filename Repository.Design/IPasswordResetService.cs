namespace Employee.BusinessLogic
{
    public interface IPasswordResetService
    {
        Task<PasswordOtpResult> RequestOtpAsync(string email);

        Task<string?> VerifyOtpAsync(
            string email,
            string otp);

        Task<bool> ResetPasswordAsync(
            string email,
            string resetToken,
            string newPassword);

        Task<int> GetResendWaitSecondsAsync(string email);
    }

    public class PasswordOtpResult
    {
        public bool Success { get; set; }

        public bool UserNotFound { get; set; }

        public bool IsRateLimited { get; set; }

        public int RetryAfterSeconds { get; set; }
    }
}
