using LeaveService.Domain.Entities;
using Shared.Events.Saga;

namespace LeaveService.Application.Interfaces;

public interface ILeaveEventPublisher
{
    Task PublishLeaveCreatedAsync(LeaveRequest leave);
    Task PublishLeaveApprovedAsync(LeaveRequest leave);
    Task PublishLeaveRejectedAsync(LeaveRequest leave);
    Task PublishLeaveCancelledAsync(LeaveRequest leave);
    Task PublishLeaveEscalatedAsync(LeaveRequest leave);
    Task PublishStartSagaAsync(StartLeaveApprovalSaga message);
}