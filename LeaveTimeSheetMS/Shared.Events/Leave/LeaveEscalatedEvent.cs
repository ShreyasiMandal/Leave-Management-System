namespace Shared.Events.Leave;

/// <summary>
/// Published by: LeaveService (when manager approves > 5 days
///               OR AlwaysRequiresHr = true leave type)
/// Consumed by:  NotificationService → notifies HR Admin
/// RabbitMQ routing key: "LeaveEscalatedToHR"
/// </summary>
public class LeaveEscalatedEvent
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }  // Employee
    public int ManagerId { get; set; }  // Manager who forwarded
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Days { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    // "ExceedsFiveDays" or "RequiresHRApproval"
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}