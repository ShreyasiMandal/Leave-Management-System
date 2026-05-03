namespace ReportService.Domain.Models;

public class TimesheetReportModel
{
    public int UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public decimal TotalHours { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}