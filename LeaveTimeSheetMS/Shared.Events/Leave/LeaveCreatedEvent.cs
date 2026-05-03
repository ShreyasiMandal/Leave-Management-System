namespace Shared.Events.Leave;

/// <summary>
/// Published by: LeaveService (when employee submits leave request)
/// Consumed by:  NotificationService → sends email + in-app to manager
/// RabbitMQ routing key: "LeaveCreated"
/// </summary>
public class LeaveCreatedEvent
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }  // Employee who applied
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Days { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}