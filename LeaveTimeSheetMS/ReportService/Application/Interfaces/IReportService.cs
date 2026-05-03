using ReportService.Application.DTOs;

namespace ReportService.Application.Interfaces;

public interface IReportService
{
    // FR-REP-001: Leave report
    Task<IEnumerable<LeaveReportRowDto>> GetLeaveReportAsync(
        LeaveReportFilterDto filter);

    // FR-REP-002: Timesheet report
    Task<IEnumerable<TimesheetReportRowDto>> GetTimesheetReportAsync(
        TimesheetReportFilterDto filter);

    // FR-REP-003: Attendance dashboard
    Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(DateTime date);
}