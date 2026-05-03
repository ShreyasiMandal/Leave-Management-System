using LeaveService.Application.Interfaces;
using LeaveService.Domain.Enums;
using MassTransit;
using Shared.Events.Saga;

namespace LeaveService.Application.Consumers;

public class ApproveLeaveBalanceConsumer : IConsumer<ApproveLeaveBalance>
{
    private readonly ILeaveBalanceRepository _balanceRepo;
    private readonly ILeaveRequestRepository _leaveRepo;
    private readonly ILogger<ApproveLeaveBalanceConsumer> _logger;

    public ApproveLeaveBalanceConsumer(
        ILeaveBalanceRepository balanceRepo,
        ILeaveRequestRepository leaveRepo,
        ILogger<ApproveLeaveBalanceConsumer> logger)
    {
        _balanceRepo = balanceRepo;
        _leaveRepo = leaveRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ApproveLeaveBalance> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "ApproveLeaveBalance received — LeaveId: {LeaveId}, " +
            "UserId from SAGA: {UserId}",
            msg.LeaveId, msg.UserId);

        // ── STEP 1: Get actual leave request ──────────────────────────────
        var leave = await _leaveRepo.GetByIdAsync(msg.LeaveId);
        if (leave == null)
        {
            _logger.LogWarning(
                "Leave {LeaveId} not found — skipping balance update",
                msg.LeaveId);
            return;
        }

        // ── STEP 2: Use UserId FROM LeaveRequest not from SAGA msg ────────
        // SAGA may store a different UserId — LeaveRequest is the truth
        int actualUserId = leave.UserId;

        _logger.LogInformation(
            "Actual UserId from LeaveRequest: {UserId}", actualUserId);

        // ── STEP 3: Update LeaveRequest status to Approved ────────────────
        leave.Status = LeaveStatus.Approved;
        leave.UpdatedAt = DateTime.UtcNow;
        await _leaveRepo.UpdateAsync(leave);

        _logger.LogInformation(
            "Leave {LeaveId} status updated to Approved", msg.LeaveId);

        // ── STEP 4: Update balance — move Pending → Used ──────────────────
        var balance = await _balanceRepo.GetAsync(
            actualUserId,
            leave.LeaveTypeId,
            msg.Year);

        if (balance != null)
        {
            // Remove from pending (can't go below 0)
            balance.Pending = Math.Max(0, balance.Pending - leave.Days);
            // Add to used
            balance.Used += leave.Days;
            balance.UpdatedAt = DateTime.UtcNow;

            await _balanceRepo.UpdateAsync(balance);

            // ✅ FIX: 3 placeholders matching 3 parameters exactly
            _logger.LogInformation(
                "Balance updated — UserId: {UserId}, " +
                "Used: +{Days}, Pending: -{PendingDays}",
                actualUserId,
                leave.Days,
                leave.Days);
        }
        else
        {
            _logger.LogWarning(
                "Balance NOT FOUND — UserId: {UserId}, " +
                "LeaveTypeId: {LeaveTypeId}, Year: {Year}",
                actualUserId,
                leave.LeaveTypeId,
                msg.Year);
        }
    }
}