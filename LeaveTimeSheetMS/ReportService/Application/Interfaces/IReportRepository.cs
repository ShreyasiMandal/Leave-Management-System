using ReportService.Domain.Models;

namespace ReportService.Application.Interfaces;

/// <summary>
/// Report Service reads from LeaveDB and TimesheetDB
/// using read-only connection strings.
/// No writes — purely for reporting.
/// </summary>
public interface IReportRepository
{
    Task<IEnumerable<LeaveReportModel>> GetLeaveReportAsync(
        DateTime? startDate, DateTime? endDate,
        int? userId, int? departmentId,
        string? leaveType, string? status);

    Task<IEnumerable<TimesheetReportModel>> GetTimesheetReportAsync(
        DateTime? startDate, DateTime? endDate,
        int? userId, int? projectId);

    // FR-REP-003: Attendance dashboard
    Task<AttendanceSummaryModel> GetAttendanceSummaryAsync(DateTime date);
}