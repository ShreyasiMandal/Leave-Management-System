namespace ReportService.Domain.Models;

public class LeaveReportModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Days { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedOn { get; set; }
}

public class AttendanceSummaryModel
{
    public DateTime Date { get; set; }
    public int PresentCount { get; set; }
    public int OnLeaveCount { get; set; }
    public int AbsentCount { get; set; }
    public int TotalCount { get; set; }
}