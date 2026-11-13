
namespace Employee.BusinessLogic
{
    public interface IPasswordResetService
    {
        // Send OTP to registered email
        Task RequestOtpAsync(string email);

        // Verify OTP and return reset authorization token
        Task<string?> VerifyOtpAsync(
            string email,
            string otp);

        // Update password after OTP verification
        Task<bool> ResetPasswordAsync(
            string email,
            string resetToken,
            string newPassword);
    }
}
