namespace NotificationService.Domain.Entities;

/// <summary>
/// FR-NOTIF-001: In-app notification record.
/// FR-NOTIF-005: Read/unread tracking.
/// </summary>
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }   // Recipient
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // NotificationType
    public bool IsRead { get; set; } = false;         // FR-NOTIF-005
    public int? EntityId { get; set; }   // LeaveId / TimesheetId
    public string? EntityType { get; set; }  // "Leave" / "Timesheet"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}