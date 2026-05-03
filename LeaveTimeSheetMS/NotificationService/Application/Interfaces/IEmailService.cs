namespace NotificationService.Application.Interfaces;

/// <summary>
/// FR-NOTIF-002: Send emails via SMTP.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body);
}