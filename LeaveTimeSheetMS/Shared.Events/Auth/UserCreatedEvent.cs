namespace Shared.Events.Auth;

/// <summary>
/// Published by: AuthService (when new user registers)
/// Consumed by:  EmployeeService → auto-creates employee profile
///               NotificationService → sends welcome email
/// RabbitMQ routing key: "UserCreated"
/// </summary>
public class UserCreatedEvent
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}