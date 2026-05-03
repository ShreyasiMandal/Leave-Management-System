using AuthService.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AuthService.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration config,
        ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── WELCOME EMAIL ─────────────────────────────────────────────────────────
    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string tempPassword)
    {
        var subject = "Welcome to LTMA — Your Login Credentials";

        var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; font-family: Arial, sans-serif;
             background-color: #f4f4f4;'>
  <div style='max-width:600px; margin:30px auto; background:#ffffff;
              border-radius:8px; overflow:hidden;
              box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>

    <!-- Header -->
    <div style='background-color:#1F4E78; padding:30px;
                text-align:center;'>
      <h1 style='color:#ffffff; margin:0; font-size:24px;'>
        Welcome to LTMA
      </h1>
      <p style='color:#a8d4f5; margin:8px 0 0 0; font-size:14px;'>
        Leave & Timesheet Management Application
      </p>
    </div>

    <!-- Body -->
    <div style='padding:30px;'>
      <p style='font-size:16px; color:#333;'>
        Hello, <strong>{fullName}</strong>
      </p>
      <p style='color:#555; line-height:1.6;'>
        Your account has been created by HR.
        Below are your login credentials.
        Please log in and change your password immediately.
      </p>

      <!-- Credentials Table -->
      <table style='width:100%; border-collapse:collapse;
                    margin:20px 0; border-radius:6px;
                    overflow:hidden;'>
        <tr style='background-color:#1F4E78;'>
          <td style='padding:12px 16px; color:#ffffff;
                     font-weight:bold; width:40%;'>
            Email
          </td>
          <td style='padding:12px 16px; color:#ffffff;'>
            {toEmail}
          </td>
        </tr>
        <tr style='background-color:#f0f7ff;'>
          <td style='padding:12px 16px; font-weight:bold;
                     color:#1F4E78; border:1px solid #d0e4f7;'>
            Temporary Password
          </td>
          <td style='padding:12px 16px;
                     font-size:18px; font-weight:bold;
                     color:#1F4E78; letter-spacing:2px;
                     border:1px solid #d0e4f7;'>
            {tempPassword}
          </td>
        </tr>
      </table>

      <!-- Warning Box -->
      <div style='background:#fff8e1; border-left:4px solid #f9a825;
                  padding:15px; border-radius:4px; margin:20px 0;'>
        <strong style='color:#e65100;'>⚠ Important:</strong>
        <span style='color:#555;'>
          This is a system-generated temporary password.
          You must change it when you first log in.
          Never share your credentials with anyone.
        </span>
      </div>

      <p style='color:#555;'>
        Login URL:
        <a href='http://localhost:4200'
           style='color:#1F4E78;'>
          http://localhost:4200
        </a>
      </p>
    </div>

    <!-- Footer -->
    <div style='background:#f8f8f8; padding:20px;
                text-align:center; border-top:1px solid #eee;'>
      <p style='color:#aaa; font-size:12px; margin:0;'>
        This is an automated message from LTMA System.
        Please do not reply to this email.
      </p>
    </div>

  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    // ── OTP EMAIL ─────────────────────────────────────────────────────────────
    public async Task SendOtpEmailAsync(
        string toEmail,
        string fullName,
        string otp)
    {
        var subject = "LTMA — Your Password Reset OTP";

        var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; font-family: Arial, sans-serif;
             background-color: #f4f4f4;'>
  <div style='max-width:600px; margin:30px auto; background:#ffffff;
              border-radius:8px; overflow:hidden;
              box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>

    <!-- Header -->
    <div style='background-color:#1F4E78; padding:30px;
                text-align:center;'>
      <h1 style='color:#ffffff; margin:0; font-size:24px;'>
        Password Reset
      </h1>
      <p style='color:#a8d4f5; margin:8px 0 0 0; font-size:14px;'>
        LTMA — Leave & Timesheet Management
      </p>
    </div>

    <!-- Body -->
    <div style='padding:30px; text-align:center;'>
      <p style='font-size:16px; color:#333; text-align:left;'>
        Hello, <strong>{fullName}</strong>
      </p>
      <p style='color:#555; text-align:left; line-height:1.6;'>
        We received a request to reset your LTMA password.
        Use the OTP below. It is valid for
        <strong>10 minutes only</strong>.
      </p>

      <!-- OTP Box -->
      <div style='margin:30px auto; display:inline-block;'>
        <div style='background:#1F4E78; color:#ffffff;
                    font-size:40px; font-weight:bold;
                    letter-spacing:12px; padding:20px 40px;
                    border-radius:10px; text-align:center;'>
          {otp}
        </div>
      </div>

      <!-- Warning -->
      <div style='background:#ffeaea; border-left:4px solid #e53935;
                  padding:15px; border-radius:4px;
                  margin:20px 0; text-align:left;'>
        <strong style='color:#c62828;'>⚠ Warning:</strong>
        <span style='color:#555;'>
          This OTP expires in 10 minutes.
          If you did not request this, please ignore this email.
          Your password will not change.
        </span>
      </div>

      <p style='color:#aaa; font-size:13px; text-align:left;'>
        Do not share this OTP with anyone.
        LTMA staff will never ask for your OTP.
      </p>
    </div>

    <!-- Footer -->
    <div style='background:#f8f8f8; padding:20px;
                text-align:center; border-top:1px solid #eee;'>
      <p style='color:#aaa; font-size:12px; margin:0;'>
        This is an automated message from LTMA System.
        Please do not reply to this email.
      </p>
    </div>

  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    // ── PASSWORD CHANGED CONFIRMATION ─────────────────────────────────────────
    public async Task SendPasswordChangedEmailAsync(
        string toEmail,
        string fullName)
    {
        var subject = "LTMA — Password Changed Successfully";

        var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; font-family: Arial, sans-serif;
             background-color: #f4f4f4;'>
  <div style='max-width:600px; margin:30px auto; background:#ffffff;
              border-radius:8px; overflow:hidden;'>

    <div style='background-color:#2E7D32; padding:30px;
                text-align:center;'>
      <h1 style='color:#ffffff; margin:0;'>
        ✓ Password Changed
      </h1>
    </div>

    <div style='padding:30px;'>
      <p style='font-size:16px; color:#333;'>
        Hello, <strong>{fullName}</strong>
      </p>
      <p style='color:#555; line-height:1.6;'>
        Your LTMA password has been changed successfully.
      </p>
      <p style='color:#555;'>
        Time: <strong>{DateTime.UtcNow:dd MMM yyyy, HH:mm} UTC</strong>
      </p>
      <div style='background:#ffeaea; border-left:4px solid #e53935;
                  padding:15px; border-radius:4px; margin:20px 0;'>
        <strong style='color:#c62828;'>Not you?</strong>
        <span style='color:#555;'>
          If you did not make this change, contact HR Admin immediately.
        </span>
      </div>
    </div>

    <div style='background:#f8f8f8; padding:20px;
                text-align:center; border-top:1px solid #eee;'>
      <p style='color:#aaa; font-size:12px; margin:0;'>
        LTMA System — Automated Message
      </p>
    </div>

  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    // ── PRIVATE SMTP SENDER ───────────────────────────────────────────────────
    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody)
    {
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _config["Smtp:FromName"] ?? "LTMA System",
                _config["Smtp:FromEmail"] ?? "noreply@ltma.com"));

            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _config["Smtp:Host"] ?? "smtp.gmail.com",
                int.Parse(_config["Smtp:Port"] ?? "587"),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _config["Smtp:Username"] ?? "",
                _config["Smtp:Password"] ?? "");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "✅ Email sent to {Email} | Subject: {Subject}",
                toEmail, subject);
        }
        catch (Exception ex)
        {
            // Email failure never crashes the app
            _logger.LogError(ex,
                "❌ Failed to send email to {Email}", toEmail);
        }
    }
}