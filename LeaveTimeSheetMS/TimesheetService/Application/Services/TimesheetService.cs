using TimesheetService.Application.DTOs;
using TimesheetService.Application.DTOs.TimesheetDTOs;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Entities;
using TimesheetService.Domain.Enums;

namespace TimesheetService.Application.Services;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _repo;
    private readonly IProjectRepository _projectRepo;
    private readonly ITimesheetEventPublisher _publisher;

    // FR-TS-004: Configurable thresholds
    private const decimal MaxDailyHours = 12m;
    private const decimal MinWeeklyHours = 40m;

    public TimesheetService(
        ITimesheetRepository repo,
        IProjectRepository projectRepo,
        ITimesheetEventPublisher publisher)
    {
        _repo = repo;
        _projectRepo = projectRepo;
        _publisher = publisher;
    }

    // ── LOG ENTRY ─────────────────────────────────────────────────────────────

    /// <summary>
    /// FR-TS-001: Log daily hours.
    /// FR-TS-003: No future dates.
    /// FR-TS-005: Warn if hours > 12 or daily total below threshold.
    /// </summary>
    public async Task<TimesheetLogResultDto> LogEntryAsync(
        int userId, CreateTimesheetEntryDto dto)
    {
        // FR-TS-003: Block future dates
        if (dto.Date.Date > DateTime.UtcNow.Date)
            throw new InvalidOperationException(
                "Cannot log timesheet for a future date. (FR-TS-003)");

        if (dto.Hours <= 0 || dto.Hours > 24)
            throw new InvalidOperationException(
                "Hours must be between 0.5 and 24.");

        // Validate project exists and is active
        var project = await _projectRepo.GetByIdAsync(dto.ProjectId)
            ?? throw new InvalidOperationException(
                $"Project ID {dto.ProjectId} not found.");

        if (!project.IsActive)
            throw new InvalidOperationException(
                $"Project '{project.Name}' is not active.");

        // FR-TS-008: Block editing if week already submitted/approved
        if (await _repo.WeekAlreadySubmittedAsync(userId, GetWeekStart(dto.Date)))
            throw new InvalidOperationException(
                "This week has already been submitted. " +
                "Cannot add entries. (FR-TS-008)");

        var entry = new TimesheetEntry
        {
            UserId = userId,
            Date = dto.Date.Date,
            ProjectId = dto.ProjectId,
            Hours = dto.Hours,
            Description = dto.Description,
            Category = dto.Category,
            Status = TimesheetStatus.Draft,
            WeekStart = GetWeekStart(dto.Date),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entry);
        await _publisher.PublishTimesheetCreatedAsync(entry);

        // Reload with navigation
        var saved = await _repo.GetByIdAsync(entry.Id);

        // FR-TS-005: Calculate daily total and generate warnings
        var dailyTotal = await _repo.GetDailyTotalHoursAsync(userId, dto.Date);
        bool exceeds = dto.Hours > MaxDailyHours;
        bool below = dailyTotal < 8m;

        return new TimesheetLogResultDto
        {
            Entry = Map(saved!),
            ExceedsMaxHours = exceeds,
            BelowThreshold = below,
            DailyTotal = dailyTotal,
            Warning = exceeds
                ? $"Hours logged ({dto.Hours}h) exceed the 12-hour daily maximum."
                : below
                    ? $"Daily total ({dailyTotal}h) is below the 8-hour threshold."
                    : null
        };
    }

    // ── UPDATE ENTRY ──────────────────────────────────────────────────────────

    /// <summary>
    /// FR-TS-008: Cannot edit if Approved or Locked.
    /// FR-TS-003: Cannot change date to future.
    /// </summary>
    public async Task<TimesheetLogResultDto> UpdateEntryAsync(
        int userId, int entryId, CreateTimesheetEntryDto dto)
    {
        var entry = await _repo.GetByIdAsync(entryId)
            ?? throw new KeyNotFoundException($"Timesheet entry {entryId} not found.");

        if (entry.UserId != userId)
            throw new UnauthorizedAccessException(
                "You can only edit your own timesheet entries.");

        if (entry.Status == TimesheetStatus.Approved ||
            entry.Status == TimesheetStatus.Locked)
            throw new InvalidOperationException(
                "Approved/Locked entries cannot be edited. (FR-TS-008)");

        if (entry.Status == TimesheetStatus.Submitted)
            throw new InvalidOperationException(
                "Submitted entries cannot be edited until manager action.");

        if (dto.Date.Date > DateTime.UtcNow.Date)
            throw new InvalidOperationException(
                "Cannot set a future date. (FR-TS-003)");

        var project = await _projectRepo.GetByIdAsync(dto.ProjectId)
            ?? throw new InvalidOperationException(
                $"Project ID {dto.ProjectId} not found.");

        entry.Date = dto.Date.Date;
        entry.ProjectId = dto.ProjectId;
        entry.Hours = dto.Hours;
        entry.Description = dto.Description;
        entry.Category = dto.Category;
        entry.WeekStart = GetWeekStart(dto.Date);
        entry.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entry);

        var updated = await _repo.GetByIdAsync(entryId);
        var dailyTotal = await _repo.GetDailyTotalHoursAsync(userId, dto.Date);
        bool exceeds = dto.Hours > MaxDailyHours;

        return new TimesheetLogResultDto
        {
            Entry = Map(updated!),
            ExceedsMaxHours = exceeds,
            DailyTotal = dailyTotal,
            Warning = exceeds
                ? $"Hours ({dto.Hours}h) exceed the 12-hour daily maximum."
                : null
        };
    }

    // ── WEEKLY VIEW ───────────────────────────────────────────────────────────

    /// <summary>
    /// FR-TS-002: Get all entries for a week + totals + warnings.
    /// FR-TA-004: Flag if week is overdue (past Friday, not submitted).
    /// </summary>
    public async Task<WeeklyTimesheetDto> GetWeeklyAsync(int userId, DateTime date)
    {
        var weekStart = GetWeekStart(date);
        var weekEnd = weekStart.AddDays(4); // Friday
        var entries = await _repo.GetByWeekAsync(userId, weekStart);
        var entryList = entries.ToList();

        decimal totalHours = entryList.Sum(e => e.Hours);

        // Determine overall week status from entries
        string weekStatus = entryList.Any()
            ? GetWeekStatus(entryList)
            : TimesheetStatus.Draft;

        // FR-TA-004: Overdue if past Friday and not submitted
        bool isOverdue = DateTime.UtcNow.Date > weekEnd.Date
            && weekStatus == TimesheetStatus.Draft;

        return new WeeklyTimesheetDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            Entries = entryList.Select(Map).ToList(),
            TotalHours = totalHours,
            WeekStatus = weekStatus,
            HoursExceed12 = entryList.Any(e => e.Hours > MaxDailyHours),
            HoursBelowThreshold = totalHours < MinWeeklyHours,
            MinThreshold = MinWeeklyHours,
            IsOverdue = isOverdue
        };
    }

    // ── HISTORY ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<TimesheetEntryDto>> GetMyHistoryAsync(int userId)
        => (await _repo.GetByUserIdAsync(userId)).Select(Map);

    // ── SUBMIT WEEK ───────────────────────────────────────────────────────────

    /// <summary>
    /// FR-TS-006: Submit all Draft entries for the week.
    /// FR-TA-004: Flag late submission.
    /// Publishes TimesheetSubmitted event to RabbitMQ → Notification Service.
    /// </summary>
    public async Task<WeeklyTimesheetDto> SubmitWeekAsync(
        int userId, DateTime weekStart)
    {
        var ws = GetWeekStart(weekStart);
        var entries = (await _repo.GetByWeekAsync(userId, ws)).ToList();

        if (!entries.Any())
            throw new InvalidOperationException(
                "No timesheet entries found for this week.");

        var draftEntries = entries
            .Where(e => e.Status == TimesheetStatus.Draft)
            .ToList();

        if (!draftEntries.Any())
            throw new InvalidOperationException(
                "No Draft entries to submit. Week may already be submitted.");

        decimal totalHours = entries.Sum(e => e.Hours);

        // FR-TA-004: Flag if submitting after Friday deadline
        var weekEnd = ws.AddDays(4);
        bool isLate = DateTime.UtcNow.Date > weekEnd.Date;

        foreach (var entry in draftEntries)
        {
            entry.Status = TimesheetStatus.Submitted;
            entry.IsLateSubmission = isLate;
            entry.UpdatedAt = DateTime.UtcNow;
        }

        await _repo.UpdateRangeAsync(draftEntries);

        // Publish async event → Notification Service notifies manager
        await _publisher.PublishTimesheetSubmittedAsync(userId, ws, totalHours);

        return await GetWeeklyAsync(userId, ws);
    }

    // ── MANAGER — APPROVAL ────────────────────────────────────────────────────

    public async Task<IEnumerable<TimesheetEntryDto>> GetPendingApprovalAsync()
        => (await _repo.GetPendingApprovalAsync()).Select(Map);

    /// <summary>
    /// FR-TA-003: Manager views consolidated team summary for a week.
    /// </summary>
    public async Task<IEnumerable<TeamTimesheetSummaryDto>> GetTeamSummaryAsync(
        int managerUserId, DateTime weekStart)
    {
        var ws = GetWeekStart(weekStart);
        var weekEnd = ws.AddDays(4);

        // In microservices: team userIds come from frontend (Angular passes them)
        // or from EmployeeService event projection.
        // For now: get all submitted entries for the week grouped by user.
        var allEntries = (await _repo.GetTeamEntriesByWeekAsync(
            Enumerable.Empty<int>(), ws)).ToList();

        return allEntries
            .GroupBy(e => e.UserId)
            .Select(g =>
            {
                var list = g.ToList();
                var totalHours = list.Sum(e => e.Hours);
                var weekStatus = GetWeekStatus(list);
                bool isOverdue = DateTime.UtcNow.Date > weekEnd.Date
                    && weekStatus == TimesheetStatus.Draft;

                return new TeamTimesheetSummaryDto
                {
                    UserId = g.Key,
                    EmployeeName = $"Employee {g.Key}",
                    WeekStart = ws,
                    TotalHours = totalHours,
                    WeekStatus = weekStatus,
                    EntryCount = list.Count,
                    IsOverdue = isOverdue,
                    Entries = list.Select(Map).ToList()
                };
            });
    }

    /// <summary>
    /// FR-TA-002: Approve a single timesheet entry.
    /// FR-TS-008: Approved entries become Locked.
    /// </summary>
    public async Task ApproveAsync(int entryId, int approverUserId)
    {
        var entry = await _repo.GetByIdAsync(entryId)
            ?? throw new KeyNotFoundException(
                $"Timesheet entry {entryId} not found.");

        if (entry.Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot approve entry with status '{entry.Status}'.");

        // FR-TS-008: Approved = Locked
        entry.Status = TimesheetStatus.Locked;
        entry.ApproverId = approverUserId;
        entry.ApprovedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entry);
        await _publisher.PublishTimesheetApprovedAsync(entry);
    }

    /// <summary>
    /// FR-TA-002: Reject with mandatory comment.
    /// Entry goes back to Draft for revision.
    /// </summary>
    public async Task RejectAsync(int entryId, int approverUserId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException(
                "Rejection comment is mandatory. (FR-TA-002)");

        var entry = await _repo.GetByIdAsync(entryId)
            ?? throw new KeyNotFoundException(
                $"Timesheet entry {entryId} not found.");

        if (entry.Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot reject entry with status '{entry.Status}'.");

        // Send back for revision
        entry.Status = TimesheetStatus.Rejected;
        entry.ApproverId = approverUserId;
        entry.ApproverComment = comment;
        entry.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entry);
        await _publisher.PublishTimesheetRejectedAsync(entry);
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────

    // Always get Monday of the week
    private static DateTime GetWeekStart(DateTime date)
    {
        var d = date.Date;
        int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }

    // Determine week's overall status from its entries
    private static string GetWeekStatus(List<TimesheetEntry> entries)
    {
        if (entries.All(e => e.Status == TimesheetStatus.Locked))
            return TimesheetStatus.Locked;
        if (entries.All(e => e.Status == TimesheetStatus.Approved))
            return TimesheetStatus.Approved;
        if (entries.Any(e => e.Status == TimesheetStatus.Submitted))
            return TimesheetStatus.Submitted;
        if (entries.Any(e => e.Status == TimesheetStatus.Rejected))
            return TimesheetStatus.Rejected;
        return TimesheetStatus.Draft;
    }

    private static TimesheetEntryDto Map(TimesheetEntry e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Date = e.Date,
        ProjectId = e.ProjectId,
        ProjectName = e.Project?.Name ?? string.Empty,
        ProjectCode = e.Project?.Code ?? string.Empty,
        Hours = e.Hours,
        Description = e.Description,
        Category = e.Category,
        Status = e.Status,
        WeekStart = e.WeekStart,
        ApproverComment = e.ApproverComment,
        IsLateSubmission = e.IsLateSubmission,
        CreatedAt = e.CreatedAt
    };
}