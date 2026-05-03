namespace NotificationService.Domain.Entities;

/// <summary>
/// FR-NOTIF-003: HR Admin configures templates with dynamic placeholders.
/// Placeholders: {EmployeeName}, {LeaveType}, {StartDate}, {EndDate},
///               {Days}, {Status}, {Comment}, {WeekStart}
/// </summary>
public class NotificationTemplate
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // NotificationType key
    public string Subject { get; set; } = string.Empty; // Email subject
    public string Body { get; set; } = string.Empty; // Template with placeholders
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}