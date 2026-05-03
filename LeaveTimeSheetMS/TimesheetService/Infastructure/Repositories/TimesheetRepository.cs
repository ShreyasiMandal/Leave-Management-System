using Microsoft.EntityFrameworkCore;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Entities;
using TimesheetService.Domain.Enums;
using TimesheetService.Infrastructure.Data;

namespace TimesheetService.Infrastructure.Repositories;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly TimesheetDbContext _ctx;
    public TimesheetRepository(TimesheetDbContext ctx) => _ctx = ctx;

    public async Task<TimesheetEntry?> GetByIdAsync(int id)
        => await _ctx.TimesheetEntries
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<TimesheetEntry>> GetByWeekAsync(
        int userId, DateTime weekStart)
        => await _ctx.TimesheetEntries
            .Include(x => x.Project)
            .Where(x => x.UserId == userId && x.WeekStart == weekStart.Date)
            .OrderBy(x => x.Date)
            .ToListAsync();

    public async Task<IEnumerable<TimesheetEntry>> GetByUserIdAsync(int userId)
        => await _ctx.TimesheetEntries
            .Include(x => x.Project)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ToListAsync();

    // FR-TA-001: All submitted entries pending manager approval
    public async Task<IEnumerable<TimesheetEntry>> GetPendingApprovalAsync()
        => await _ctx.TimesheetEntries
            .Include(x => x.Project)
            .Where(x => x.Status == TimesheetStatus.Submitted)
            .OrderBy(x => x.WeekStart).ThenBy(x => x.UserId)
            .ToListAsync();

    // FR-TA-003: Team entries for a specific week
    public async Task<IEnumerable<TimesheetEntry>> GetTeamEntriesByWeekAsync(
        IEnumerable<int> teamUserIds, DateTime weekStart)
    {
        var query = _ctx.TimesheetEntries
            .Include(x => x.Project)
            .Where(x => x.WeekStart == weekStart.Date);

        // If teamUserIds provided — filter by team
        var idList = teamUserIds.ToList();
        if (idList.Any())
            query = query.Where(x => idList.Contains(x.UserId));

        return await query
            .OrderBy(x => x.UserId).ThenBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetDailyTotalHoursAsync(int userId, DateTime date)
        => await _ctx.TimesheetEntries
            .Where(x => x.UserId == userId && x.Date.Date == date.Date)
            .SumAsync(x => x.Hours);

    public async Task<bool> WeekAlreadySubmittedAsync(int userId, DateTime weekStart)
        => await _ctx.TimesheetEntries
            .AnyAsync(x =>
                x.UserId == userId &&
                x.WeekStart == weekStart.Date &&
                (x.Status == TimesheetStatus.Submitted ||
                 x.Status == TimesheetStatus.Approved ||
                 x.Status == TimesheetStatus.Locked));

    public async Task AddAsync(TimesheetEntry entry)
    {
        await _ctx.TimesheetEntries.AddAsync(entry);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(TimesheetEntry entry)
    {
        _ctx.TimesheetEntries.Update(entry);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<TimesheetEntry> entries)
    {
        _ctx.TimesheetEntries.UpdateRange(entries);
        await _ctx.SaveChangesAsync();
    }
}