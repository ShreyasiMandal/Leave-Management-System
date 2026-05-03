namespace Shared.Events.Timesheet;

public class TimesheetCreatedEvent
{
    public int EntryId { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public int ProjectId { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public DateTime EventAt { get; set; }
}