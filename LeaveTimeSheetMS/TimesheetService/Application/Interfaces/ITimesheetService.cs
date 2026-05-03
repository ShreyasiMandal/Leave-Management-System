using TimesheetService.Application.DTOs;
using TimesheetService.Application.DTOs.TimesheetDTOs;

namespace TimesheetService.Application.Interfaces;

public interface ITimesheetService
{
    // ── EMPLOYEE ─────────────────────────────────────────────────────────────
    // FR-TS-001: Log daily entry
    Task<TimesheetLogResultDto> LogEntryAsync(
        int userId, CreateTimesheetEntryDto dto);

    // Update a draft entry
    Task<TimesheetLogResultDto> UpdateEntryAsync(
        int userId, int entryId, CreateTimesheetEntryDto dto);

    // FR-TS-002: Get weekly view
    Task<WeeklyTimesheetDto> GetWeeklyAsync(int userId, DateTime date);

    // Timesheet history
    Task<IEnumerable<TimesheetEntryDto>> GetMyHistoryAsync(int userId);

    // FR-TS-006: Submit whole week for approval
    Task<WeeklyTimesheetDto> SubmitWeekAsync(int userId, DateTime weekStart);

    // ── MANAGER ──────────────────────────────────────────────────────────────
    // FR-TA-001: Pending timesheets for manager
    Task<IEnumerable<TimesheetEntryDto>> GetPendingApprovalAsync();

    // FR-TA-003: Consolidated team view
    Task<IEnumerable<TeamTimesheetSummaryDto>> GetTeamSummaryAsync(
        int managerUserId, DateTime weekStart);

    // FR-TA-002: Approve single entry
    Task ApproveAsync(int entryId, int approverUserId);

    // FR-TA-002: Reject with mandatory comment
    Task RejectAsync(int entryId, int approverUserId, string comment);
}