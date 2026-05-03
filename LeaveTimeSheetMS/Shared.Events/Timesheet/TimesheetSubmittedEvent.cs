namespace Shared.Events.Timesheet;

/// <summary>
/// Published by: TimesheetService (when employee submits weekly timesheet)
/// Consumed by:  NotificationService → sends email + in-app to manager
/// RabbitMQ routing key: "TimesheetSubmitted"
/// </summary>
public class TimesheetSubmittedEvent
{
    public int UserId { get; set; }  // Employee
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public decimal TotalHours { get; set; }
    public bool IsLate { get; set; }  // FR-TA-004: late submission flag
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}