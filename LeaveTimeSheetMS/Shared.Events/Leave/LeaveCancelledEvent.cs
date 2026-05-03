namespace Shared.Events.Leave;

/// <summary>
/// Published by: LeaveService (when employee cancels their leave)
/// Consumed by:  NotificationService → sends confirmation to employee
/// RabbitMQ routing key: "LeaveCancelled"
/// </summary>
public class LeaveCancelledEvent
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}