namespace Shared.Events.Timesheet;

/// <summary>
/// Published by: TimesheetService (when manager approves entry)
/// Consumed by:  NotificationService → sends email + in-app to employee
/// RabbitMQ routing key: "TimesheetApproved"
/// </summary>
public class TimesheetApprovedEvent
{
    public int EntryId { get; set; }
    public int UserId { get; set; }  // Employee
    public int ApproverId { get; set; }  // Manager
    public DateTime WeekStart { get; set; }
    public decimal Hours { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime EventAt { get; set; } = DateTime.UtcNow;
}