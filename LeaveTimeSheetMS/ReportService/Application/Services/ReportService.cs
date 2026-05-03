using ReportService.Application.DTOs;
using ReportService.Application.Interfaces;

namespace ReportService.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo) => _repo = repo;

    public async Task<IEnumerable<LeaveReportRowDto>> GetLeaveReportAsync(
        LeaveReportFilterDto filter)
    {
        var data = await _repo.GetLeaveReportAsync(
            filter.StartDate, filter.EndDate,
            filter.UserId, filter.DepartmentId,
            filter.LeaveType, filter.Status);

        return data.Select(r => new LeaveReportRowDto
        {
            UserId = r.UserId,
            EmployeeName = r.EmployeeName,
            Department = r.Department,
            LeaveType = r.LeaveType,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Days = r.Days,
            Status = r.Status,
            AppliedOn = r.AppliedOn
        });
    }

    public async Task<IEnumerable<TimesheetReportRowDto>> GetTimesheetReportAsync(
        TimesheetReportFilterDto filter)
    {
        var data = await _repo.GetTimesheetReportAsync(
            filter.StartDate, filter.EndDate,
            filter.UserId, filter.ProjectId);

        return data.Select(r => new TimesheetReportRowDto
        {
            UserId = r.UserId,
            EmployeeName = r.EmployeeName,
            ProjectName = r.ProjectName,
            WeekStart = r.WeekStart,
            TotalHours = r.TotalHours,
            Category = r.Category,
            Status = r.Status
        });
    }

    public async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(DateTime date)
    {
        var data = await _repo.GetAttendanceSummaryAsync(date);
        return new AttendanceSummaryDto
        {
            Date = data.Date,
            PresentCount = data.PresentCount,
            OnLeaveCount = data.OnLeaveCount,
            AbsentCount = data.AbsentCount,
            TotalCount = data.TotalCount
        };
    }
}