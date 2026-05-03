namespace Shared.Events.Leave;

/// <summary>
/// Published by: LeaveService (when manager OR HR rejects)
/// Consumed by:  NotificationService → sends email + in-app to employee
/// RabbitMQ routing key: "LeaveRejected"
/// </summary>
public class LeaveRejectedEvent
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }  // Employee
    public int RejectedById { get; set; }  // Manager or HR
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Comment { get; set; } = string.Empty; // Mandatory rejection reason
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}