namespace Shared.Events.Leave;

/// <summary>
/// Published by: LeaveService (when manager OR HR approves)
/// Consumed by:  NotificationService → sends email + in-app to employee
/// RabbitMQ routing key: "LeaveApproved"
/// </summary>
public class LeaveApprovedEvent
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }  // Employee
    public int ApproverId { get; set; }  // Manager or HR who approved
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Days { get; set; }
    public string ApprovedBy { get; set; } = string.Empty; // "Manager" or "HR"
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}