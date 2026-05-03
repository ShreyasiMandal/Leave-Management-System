using MassTransit;
using LeaveService.Application.Interfaces;
using Shared.Events.Auth;

namespace LeaveService.Infastructure.Consumers;

public class UserCreatedLeaveConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILeaveBalanceService _balanceService;
    private readonly ILogger<UserCreatedLeaveConsumer> _logger;

    public UserCreatedLeaveConsumer(
        ILeaveBalanceService balanceService,
        ILogger<UserCreatedLeaveConsumer> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;

        // Only initialize for actual employees/managers/HR — not SystemAdmin
        if (evt.Role == "SystemAdmin") return;

        _logger.LogInformation(
            "UserCreated received: UserId={UserId}, Role={Role}, Gender={Gender}",
            evt.UserId, evt.Role, evt.Gender);

        await _balanceService.InitializeBalancesForNewUserAsync(
            evt.UserId,
            DateTime.UtcNow.Year,
            evt.Gender);
    }
}