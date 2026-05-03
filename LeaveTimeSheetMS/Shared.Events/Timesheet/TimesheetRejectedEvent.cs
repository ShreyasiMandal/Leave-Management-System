namespace Shared.Events.Timesheet;

/// <summary>
/// Published by: TimesheetService (when manager rejects entry)
/// Consumed by:  NotificationService → sends email + in-app to employee
/// RabbitMQ routing key: "TimesheetRejected"
/// </summary>
public class TimesheetRejectedEvent
{
    public int EntryId { get; set; }
    public int UserId { get; set; }  // Employee
    public int RejectedById { get; set; } // Manager
    public DateTime WeekStart { get; set; }
    public string Comment { get; set; } = string.Empty; // Mandatory reason
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}