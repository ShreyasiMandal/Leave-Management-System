using LeaveService.Application.Interfaces;
using MassTransit;
using Shared.Events.Saga;

namespace LeaveService.Application.Consumers;

/// <summary>
/// COMPENSATING TRANSACTION handler.
/// Called by SAGA when leave is rejected or cancelled.
/// Restores the balance that was reserved when leave was submitted.
/// </summary>
public class RestoreLeaveBalanceConsumer : IConsumer<RestoreLeaveBalance>
{
    private readonly ILeaveBalanceRepository _balanceRepo;
    private readonly ILogger<RestoreLeaveBalanceConsumer> _logger;

    public RestoreLeaveBalanceConsumer(
        ILeaveBalanceRepository balanceRepo,
        ILogger<RestoreLeaveBalanceConsumer> logger)
    {
        _balanceRepo = balanceRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RestoreLeaveBalance> context)
    {
        var msg = context.Message;
        var balance = await _balanceRepo.GetAsync(
            msg.UserId, msg.LeaveTypeId, msg.Year);

        if (balance == null)
        {
            _logger.LogWarning(
                "Balance not found for UserId {UserId}, " +
                "LeaveTypeId {LeaveTypeId}, Year {Year}",
                msg.UserId, msg.LeaveTypeId, msg.Year);
            return;
        }

        // Restore the pending days back to available
        balance.Pending -= msg.Days;
        balance.UpdatedAt = DateTime.UtcNow;
        await _balanceRepo.UpdateAsync(balance);

        _logger.LogInformation(
            "Compensating transaction: Restored {Days} days " +
            "for UserId {UserId}. Reason: {Reason}",
            msg.Days, msg.UserId, msg.Reason);
    }
}