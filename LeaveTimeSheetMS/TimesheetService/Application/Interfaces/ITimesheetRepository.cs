using TimesheetService.Domain.Entities;

namespace TimesheetService.Application.Interfaces;

public interface ITimesheetRepository
{
    Task<TimesheetEntry?> GetByIdAsync(int id);

    // FR-TS-002: All entries for a user in a given week
    Task<IEnumerable<TimesheetEntry>> GetByWeekAsync(
        int userId, DateTime weekStart);

    // All entries for a user (history)
    Task<IEnumerable<TimesheetEntry>> GetByUserIdAsync(int userId);

    // FR-TA-001: Submitted entries pending manager approval
    Task<IEnumerable<TimesheetEntry>> GetPendingApprovalAsync();

    // FR-TA-003: Team timesheet summary for a manager
    Task<IEnumerable<TimesheetEntry>> GetTeamEntriesByWeekAsync(
        IEnumerable<int> teamUserIds, DateTime weekStart);

    // Total hours logged for a user on a specific day
    Task<decimal> GetDailyTotalHoursAsync(int userId, DateTime date);

    // Check if any submitted/approved entries exist for a week
    Task<bool> WeekAlreadySubmittedAsync(int userId, DateTime weekStart);

    Task AddAsync(TimesheetEntry entry);
    Task UpdateAsync(TimesheetEntry entry);
    Task UpdateRangeAsync(IEnumerable<TimesheetEntry> entries);
}