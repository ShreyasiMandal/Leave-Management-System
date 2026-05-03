using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events.Leave;
using Shared.Events.Saga;

namespace LeaveService.Infrastructure.Messaging;

public class LeaveEventPublisher : ILeaveEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LeaveEventPublisher> _logger;

    public LeaveEventPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<LeaveEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishLeaveCreatedAsync(LeaveRequest l)
    {
        var message = new LeaveCreatedEvent
        {
            LeaveId = l.Id,
            UserId = l.UserId,
            LeaveTypeId = l.LeaveTypeId,
            LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Days = l.Days,
            Reason = l.Reason,
            EventAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(message, ctx =>
        {
            ctx.SetRoutingKey("LeaveCreated");   // 🔥 FIX
        });
        _logger.LogInformation("Published LeaveCreatedEvent for LeaveId {Id}", l.Id);
    }

    public async Task PublishLeaveApprovedAsync(LeaveRequest l)
    {
        var message = new LeaveApprovedEvent
        {
            LeaveId = l.Id,
            UserId = l.UserId,
            ApproverId = l.ManagerApproverId ?? l.HrApproverId ?? 0,
            LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Days = l.Days,
            ApprovedBy = l.HrApproverId.HasValue ? "HR" : "Manager",
            EventAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(message, ctx =>
        {
            ctx.SetRoutingKey("LeaveApproved");
        });
        _logger.LogInformation("Published LeaveApprovedEvent for LeaveId {Id}", l.Id);
    }

    public async Task PublishLeaveRejectedAsync(LeaveRequest l)
    {
        var message = new LeaveRejectedEvent
        {
            LeaveId = l.Id,
            UserId = l.UserId,
            RejectedById = l.ManagerApproverId ?? l.HrApproverId ?? 0,
            LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Comment = l.ManagerComment ?? l.HrComment ?? string.Empty,
            EventAt = DateTime.UtcNow
        };
        await _publishEndpoint.Publish(message, ctx =>
        {
            ctx.SetRoutingKey("LeaveRejected");
        });


        _logger.LogInformation("Published LeaveRejectedEvent for LeaveId {Id}", l.Id);
    }

    public async Task PublishLeaveCancelledAsync(LeaveRequest l)
    {
        var message = new LeaveCancelledEvent
        {
            LeaveId = l.Id,
            UserId = l.UserId,
            LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            EventAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(message, ctx =>
        {
            ctx.SetRoutingKey("LeaveCancelled");
        });
        _logger.LogInformation("Published LeaveCancelledEvent for LeaveId {Id}", l.Id);
    }

    public async Task PublishLeaveEscalatedAsync(LeaveRequest l)
    {
        var message = new LeaveEscalatedEvent
        {
            LeaveId = l.Id,
            UserId = l.UserId,
            ManagerId = l.ManagerApproverId ?? 0,
            LeaveTypeName = l.LeaveType?.Name ?? string.Empty,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Days = l.Days,
            EscalationReason = l.Days > 5
                ? "ExceedsFiveDays"
                : "RequiresHRApproval",
            EventAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(message, ctx =>
        {
            ctx.SetRoutingKey("LeaveEscalatedToHR");  // match consumer
        });
        _logger.LogInformation("Published LeaveEscalatedEvent for LeaveId {Id}", l.Id);
    }

    public async Task PublishStartSagaAsync(StartLeaveApprovalSaga message)
    {
        await _publishEndpoint.Publish(message, ctx =>
        {
            ctx.SetRoutingKey("StartLeaveApprovalSaga");
        });
        _logger.LogInformation("Published StartLeaveApprovalSaga for LeaveId {Id}", message.LeaveId);
    }
}