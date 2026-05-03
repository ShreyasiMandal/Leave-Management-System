namespace Shared.Events.Timesheet;

/// <summary>
/// Published by: TimesheetService background job (FR-TA-004)
///               Runs every Monday morning — checks previous week
/// Consumed by:  NotificationService → sends reminder to employee
///               + escalation alert to manager
/// RabbitMQ routing key: "TimesheetOverdue"
/// </summary>
public class TimesheetOverdueEvent
{
    public int UserId { get; set; }  // Employee who didn't submit
    public DateTime WeekStart { get; set; }  // The overdue week
    public int DaysOverdue { get; set; }
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}