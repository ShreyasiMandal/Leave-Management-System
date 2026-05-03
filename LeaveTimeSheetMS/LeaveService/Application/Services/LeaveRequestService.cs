using LeaveService.Application.DTOs.LeaveRequestDtos;
using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;
using LeaveService.Domain.Enums;
using Shared.Events.Saga;
using static System.Net.Mime.MediaTypeNames;
namespace LeaveService.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly ILeaveBalanceRepository _balanceRepo;
    private readonly ILeaveTypeRepository _typeRepo;
    private readonly IHolidayRepository _holidayRepo;
    private readonly ILeaveEventPublisher _publisher;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRepo,
        ILeaveBalanceRepository balanceRepo,
        ILeaveTypeRepository typeRepo,
        IHolidayRepository holidayRepo,
        ILeaveEventPublisher publisher)
    {
        _leaveRepo = leaveRepo;
        _balanceRepo = balanceRepo;
        _typeRepo = typeRepo;
        _holidayRepo = holidayRepo;
        _publisher = publisher;
    }

    // ── EMPLOYEE ──────────────────────────────────────────────────────────────

    public async Task<LeaveApplyResultDto> ApplyAsync(
       int userId, CreateLeaveRequestDto dto)
    {
        var leave = await BuildAsync(userId, dto, LeaveStatus.Pending);
        await _leaveRepo.AddAsync(leave);

        var lt = await _typeRepo.GetByIdAsync(dto.LeaveTypeId);
        var balance = await _balanceRepo.GetAsync(
            userId, dto.LeaveTypeId, DateTime.UtcNow.Year);

        // Add to pending balance
        if (balance != null)
        {
            balance.Pending += leave.Days;
            balance.UpdatedAt = DateTime.UtcNow;
            await _balanceRepo.UpdateAsync(balance);
        }

        // ✅ ALWAYS publish LeaveCreated
        await _publisher.PublishLeaveCreatedAsync(leave);

        // ✅ Auto-approve logic
        if (lt?.IsAutoApprove == true)
        {
            leave.Status = LeaveStatus.Approved;
            leave.ManagerActedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _leaveRepo.UpdateAsync(leave);

            if (balance != null)
            {
                balance.Pending -= leave.Days;
                balance.Used += leave.Days;
                await _balanceRepo.UpdateAsync(balance);
            }

            await _publisher.PublishLeaveApprovedAsync(leave);
        }
        else
        {
            // ✅ Start approval saga
            await _publisher.PublishStartSagaAsync(new StartLeaveApprovalSaga
            {
                CorrelationId = Guid.NewGuid(),
                LeaveId = leave.Id,

                UserId = leave.UserId,
                LeaveTypeId = leave.LeaveTypeId,
                LeaveTypeName = lt?.Name,
                Days = leave.Days,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                NeedsHrApproval = leave.Days > 5
            });
        }

        // ✅ Final result
        bool insufficient = balance != null && balance.Available < 0;

        return new LeaveApplyResultDto
        {
            Leave = Map(leave),
            InsufficientBalance = insufficient,
            AvailableBalance = balance?.Available ?? 0,
            Warning = insufficient
                ? $"Insufficient balance. Available: {balance!.Available} day(s)."
                : null
        };
    }

    public async Task<LeaveRequestDto> SaveDraftAsync(
        int userId, CreateLeaveRequestDto dto)
    {
        var leave = await BuildAsync(userId, dto, LeaveStatus.Draft);
        await _leaveRepo.AddAsync(leave);
        return Map(leave);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetMyLeavesAsync(int userId)
        => (await _leaveRepo.GetByUserIdAsync(userId)).Select(Map);

    public async Task<LeaveRequestDto?> GetByIdAsync(int id)
    {
        var l = await _leaveRepo.GetByIdAsync(id);
        return l == null ? null : Map(l);
    }

    // FR-LR-005: Cancel Pending or Approved leave
    public async Task CancelAsync(int leaveId, int userId)
    {
        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave request not found.");

        if (leave.UserId != userId)
            throw new UnauthorizedAccessException("You can only cancel your own leave.");

        if (leave.Status != LeaveStatus.Pending &&
            leave.Status != LeaveStatus.Approved)
            throw new InvalidOperationException(
                $"Cannot cancel leave with status '{leave.Status}'.");

        bool wasApproved = leave.Status == LeaveStatus.Approved;
        leave.Status = LeaveStatus.Cancelled;
        leave.UpdatedAt = DateTime.UtcNow;
        await _leaveRepo.UpdateAsync(leave);

        // FR-LB-003: Restore balance on cancellation
        var balance = await _balanceRepo.GetAsync(
            leave.UserId, leave.LeaveTypeId, DateTime.UtcNow.Year);

        if (balance != null)
        {
            if (wasApproved) balance.Used -= leave.Days;
            else balance.Pending -= leave.Days;
            balance.UpdatedAt = DateTime.UtcNow;
            await _balanceRepo.UpdateAsync(balance);
        }

        await _publisher.PublishLeaveCancelledAsync(leave);
    }

    // ── MANAGER ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingForManagerAsync()
        => (await _leaveRepo.GetPendingForManagerAsync()).Select(Map);

    // ✅ FIX 2 (Part C): ApproveAsync is now called by the controller ALONGSIDE
    // publishing to the SAGA. Previously the controller ONLY published to the SAGA
    // and never called this service method at all — so the LeaveRequest DB record
    // was never updated. For leaves > 5 days:
    //   - Set NeedsHrApproval = true
    //   - Set ManagerApproverId so the leave is attributed
    //   - Leave Status as "Pending" (will become Approved after HR acts)
    //   - The leave row moves OUT of GetPendingForManagerAsync (which filters
    //     only Status=Pending AND NeedsHrApproval=false implicitly via the
    //     repository's "Pending" state filter — see FIX in repository too)
    // For leaves <= 5 days:
    //   - Set Status = Approved immediately (SAGA's ApproveLeaveBalanceConsumer
    //     also does this asynchronously, but doing it here ensures immediate
    //     consistency in the DB without waiting for message processing)
    public async Task ApproveAsync(int leaveId, int approverUserId)
    {
        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave not found.");

        // ✅ FIX 2 (Part C): Accept both Pending statuses.
        // Previously only "Pending" was accepted — but if the SAGA already
        // processed and set a different status before this call completes,
        // the service would throw. Use a broader check.
        if (leave.Status != LeaveStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot approve leave with status '{leave.Status}'.");

        leave.ManagerApproverId = approverUserId;
        leave.ManagerActedAt = DateTime.UtcNow;
        leave.UpdatedAt = DateTime.UtcNow;

        // FR-LA-003: > 5 days needs HR second-level
        if (leave.Days > 5)
        {
            // ✅ FIX 2 (Part C): Mark as needing HR — this moves it to hr-pending
            // The SAGA transitions to WaitingHrApproval, but the DB record must also
            // reflect that it's no longer in the basic "Pending" manager queue.
            leave.NeedsHrApproval = true;
            // Keep Status = "Pending" so the record is still active,
            // but NeedsHrApproval=true will exclude it from GetPendingForManagerAsync
            // (see repository fix) and include it in GetPendingHrApprovalAsync.
            await _leaveRepo.UpdateAsync(leave);
            await _publisher.PublishLeaveEscalatedAsync(leave);
            return;
        }

        // <= 5 days: approve directly
        leave.Status = LeaveStatus.Approved;
        await _leaveRepo.UpdateAsync(leave);
        await MovePendingToUsedAsync(leave);
        await _publisher.PublishLeaveApprovedAsync(leave);
    }

    // FR-LA-002: Reject with mandatory comment
    public async Task RejectAsync(int leaveId, int approverUserId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException(
                "Rejection comment is mandatory. (FR-LA-002)");

        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave not found.");

        if (leave.Status != LeaveStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot reject leave with status '{leave.Status}'.");

        leave.Status = LeaveStatus.Rejected;
        leave.ManagerApproverId = approverUserId;
        leave.ManagerComment = comment;
        leave.ManagerActedAt = DateTime.UtcNow;
        leave.UpdatedAt = DateTime.UtcNow;
        await _leaveRepo.UpdateAsync(leave);
        await RestorePendingAsync(leave);
        await _publisher.PublishLeaveRejectedAsync(leave);
    }

    // ── HR SECOND-LEVEL ───────────────────────────────────────────────────────

    // ✅ FIX 2 (Part B): GetPendingHrAsync now fetches CorrelationIds from the
    // Saga table so the frontend can pass correlationId to hr-approve/hr-reject.
    public async Task<IEnumerable<LeaveRequestDto>> GetPendingHrAsync()
        => (await _leaveRepo.GetPendingHrApprovalAsync()).Select(Map);

    public async Task HrApproveAsync(int leaveId, int hrUserId)
    {
        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave not found.");

        if (!leave.NeedsHrApproval)
            throw new InvalidOperationException("Not pending HR approval.");

        leave.Status = LeaveStatus.Approved;
        leave.HrApproverId = hrUserId;
        leave.HrActedAt = DateTime.UtcNow;
        leave.NeedsHrApproval = false;
        leave.UpdatedAt = DateTime.UtcNow;
        await _leaveRepo.UpdateAsync(leave);
        await MovePendingToUsedAsync(leave);
        await _publisher.PublishLeaveApprovedAsync(leave);
    }

    public async Task HrRejectAsync(int leaveId, int hrUserId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("HR rejection comment is mandatory.");

        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave not found.");

        if (!leave.NeedsHrApproval)
            throw new InvalidOperationException("Not pending HR approval.");

        leave.Status = LeaveStatus.Rejected;
        leave.HrApproverId = hrUserId;
        leave.HrComment = comment;
        leave.HrActedAt = DateTime.UtcNow;
        leave.NeedsHrApproval = false;
        leave.UpdatedAt = DateTime.UtcNow;
        await _leaveRepo.UpdateAsync(leave);
        await RestorePendingAsync(leave);
        await _publisher.PublishLeaveRejectedAsync(leave);
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────

    private async Task<LeaveRequest> BuildAsync(
        int userId, CreateLeaveRequestDto dto, string status)
    {
        var lt = await _typeRepo.GetByIdAsync(dto.LeaveTypeId)
            ?? throw new InvalidOperationException(
                $"Leave type {dto.LeaveTypeId} not found.");

        if (!lt.IsActive)
            throw new InvalidOperationException(
                $"Leave type '{lt.Name}' is not active.");

        if (dto.EndDate.Date < dto.StartDate.Date)
            throw new InvalidOperationException(
                "End date cannot be before start date.");

        // FR-LR-002: Overlap check
        if (await _leaveRepo.HasOverlapAsync(userId, dto.StartDate, dto.EndDate))
            throw new InvalidOperationException(
                "You already have a leave on these dates.");

        // FR-HC-002 + FR-LR-004: Calculate working days excluding weekends
        var days = CalculateWorkingDays(dto.StartDate, dto.EndDate, dto.HalfDaySession);

        if (days <= 0)
            throw new InvalidOperationException(
                "Leave must be at least 0.5 day(s).");

        return new LeaveRequest
        {
            UserId = userId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Days = days,
            HalfDaySession = dto.HalfDaySession,
            Status = status,
            Reason = dto.Reason,
            AttachmentUrl = dto.AttachmentUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    // FR-HC-002: Exclude weekends. FR-LR-004: Half-day = 0.5
    private static decimal CalculateWorkingDays(
        DateTime start, DateTime end, string session)
    {
        if (session != "Full") return 0.5m;

        decimal days = 0;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Saturday &&
                d.DayOfWeek != DayOfWeek.Sunday)
                days++;

        return days;
    }

    // FR-LB-003: On approval move Pending → Used
    private async Task MovePendingToUsedAsync(LeaveRequest leave)
    {
        var b = await _balanceRepo.GetAsync(
            leave.UserId, leave.LeaveTypeId, DateTime.UtcNow.Year);
        if (b == null) return;
        b.Pending -= leave.Days;
        b.Used += leave.Days;
        b.UpdatedAt = DateTime.UtcNow;
        await _balanceRepo.UpdateAsync(b);
    }

    // Restore pending on rejection/cancellation
    private async Task RestorePendingAsync(LeaveRequest leave)
    {
        var b = await _balanceRepo.GetAsync(
            leave.UserId, leave.LeaveTypeId, DateTime.UtcNow.Year);
        if (b == null) return;
        b.Pending -= leave.Days;
        b.UpdatedAt = DateTime.UtcNow;
        await _balanceRepo.UpdateAsync(b);
    }

    private static LeaveRequestDto Map(LeaveRequest l) => new()
    {
        Id = l.Id,
        UserId = l.UserId,
        CorrelationId = l.CorrelationId,
        LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
        LeaveTypeCode = l.LeaveType?.Code ?? string.Empty,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        Days = l.Days,
        HalfDaySession = l.HalfDaySession,
        Status = l.Status,
        Reason = l.Reason,
        AttachmentUrl = l.AttachmentUrl,
        ManagerComment = l.ManagerComment,
        HrComment = l.HrComment,
        NeedsHrApproval = l.NeedsHrApproval,
        CreatedAt = l.CreatedAt
    };
}