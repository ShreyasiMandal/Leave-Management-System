namespace AuthService.Application.Interfaces;

public interface IEmailService
{
    // Called when HR creates a new user account
    Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string tempPassword);

    // Called when user requests OTP for password reset
    Task SendOtpEmailAsync(
        string toEmail,
        string fullName,
        string otp);

    // Called after password is reset successfully
    Task SendPasswordChangedEmailAsync(
        string toEmail,
        string fullName);
}