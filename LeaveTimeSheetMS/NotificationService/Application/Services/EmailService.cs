using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NotificationService.Application.Interfaces;
//using System.Net.Mail;

namespace NotificationService.Application.Services;

/// <summary>
/// FR-NOTIF-002: SMTP email delivery via MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(
                _config["Smtp:FromEmail"] ?? "noreply@ltma.com"));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Smtp:Host"] ?? "localhost",
                int.Parse(_config["Smtp:Port"] ?? "587"),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _config["Smtp:Username"] ?? "",
                _config["Smtp:Password"] ?? "");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "Email sent to {Email} — Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            // Email failure should never crash the app — just log
            _logger.LogError(ex,
                "Failed to send email to {Email}", toEmail);
        }
    }
}