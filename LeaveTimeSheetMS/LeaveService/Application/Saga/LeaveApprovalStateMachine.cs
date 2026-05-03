using LeaveService.Domain.Entities;
using MassTransit;
using Shared.Events.Saga;

namespace LeaveService.Application.Saga;

public class LeaveApprovalStateMachine
    : MassTransitStateMachine<LeaveApprovalSagaState>
{
    // ── STATES ───────────────────────────────────────────────────────────────
    // ✅ FIX: States must be public properties
    // MassTransit uses reflection to find them — must NOT be null!
    public State Pending { get; private set; } = null!;
    public State WaitingHrApproval { get; private set; } = null!;
    public State Approved { get; private set; } = null!;
    public State Rejected { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    // ── EVENTS ────────────────────────────────────────────────────────────────
    public Event<StartLeaveApprovalSaga> LeaveSubmitted { get; private set; } = null!;
    public Event<ManagerApprovedLeave> ManagerApproved { get; private set; } = null!;
    public Event<ManagerRejectedLeave> ManagerRejected { get; private set; } = null!;
    public Event<HrApprovedLeave> HrApproved { get; private set; } = null!;
    public Event<HrRejectedLeave> HrRejected { get; private set; } = null!;
    public Event<CancelLeaveRequest> LeaveCancelled { get; private set; } = null!;

    public LeaveApprovalStateMachine()
    {
        // ✅ FIX 1: Tell MassTransit which property stores state name
        InstanceState(x => x.CurrentState);

        // ✅ FIX 2: Explicitly register all states
        // This is what was missing — MassTransit needs this
        State(() => Pending);
        State(() => WaitingHrApproval);
        State(() => Approved);
        State(() => Rejected);
        State(() => Cancelled);

        // ✅ FIX 3: Explicitly register all events with correlation
        Event(() => LeaveSubmitted,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => ManagerApproved,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => ManagerRejected,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => HrApproved,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => HrRejected,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Event(() => LeaveCancelled,
            x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        // ── INITIAL → PENDING ─────────────────────────────────────────────────
        Initially(
            When(LeaveSubmitted)
                .Then(ctx =>
                {
                    ctx.Saga.LeaveId = ctx.Message.LeaveId;
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.LeaveTypeId = ctx.Message.LeaveTypeId;
                    ctx.Saga.LeaveTypeName = ctx.Message.LeaveTypeName;
                    ctx.Saga.Days = ctx.Message.Days;
                    ctx.Saga.StartDate = ctx.Message.StartDate;
                    ctx.Saga.EndDate = ctx.Message.EndDate;
                    ctx.Saga.NeedsHrApproval = ctx.Message.NeedsHrApproval;
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                })
                .Publish(ctx => new SendLeaveNotification
                {
                    UserId = ctx.Saga.UserId,
                    NotifType = "LeaveCreated",
                    LeaveTypeName = ctx.Saga.LeaveTypeName,
                    StartDate = ctx.Saga.StartDate,
                    EndDate = ctx.Saga.EndDate
                })
                .TransitionTo(Pending)
        );

        // ── PENDING → MANAGER ACTS ────────────────────────────────────────────
        During(Pending,

            When(ManagerApproved)
                .Then(ctx =>
                {
                    ctx.Saga.ManagerId = ctx.Message.ManagerId;
                    ctx.Saga.ManagerActedAt = DateTime.UtcNow;
                })
                .IfElse(
                    ctx => ctx.Saga.NeedsHrApproval,

                    // Needs HR — escalate
                    thenBinder => thenBinder
                        .Publish(ctx => new SendLeaveNotification
                        {
                            UserId = ctx.Saga.UserId,
                            NotifType = "LeaveEscalatedToHR",
                            LeaveTypeName = ctx.Saga.LeaveTypeName,
                            StartDate = ctx.Saga.StartDate,
                            EndDate = ctx.Saga.EndDate
                        })
                        .TransitionTo(WaitingHrApproval),

                    // No HR needed — fully approved
                    elseBinder => elseBinder
                        .Publish(ctx => new ApproveLeaveBalance
                        {
                            LeaveId = ctx.Saga.LeaveId,
                            UserId = ctx.Saga.UserId,
                            Days = ctx.Saga.Days,
                            Year = DateTime.UtcNow.Year,
                            LeaveTypeId = ctx.Saga.LeaveTypeId
                        })
                        .Publish(ctx => new SendLeaveNotification
                        {
                            UserId = ctx.Saga.UserId,
                            NotifType = "LeaveApproved",
                            LeaveTypeName = ctx.Saga.LeaveTypeName,
                            StartDate = ctx.Saga.StartDate,
                            EndDate = ctx.Saga.EndDate
                        })
                        .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                        .TransitionTo(Approved)
                ),

            When(ManagerRejected)
                .Then(ctx =>
                {
                    ctx.Saga.ManagerId = ctx.Message.ManagerId;
                    ctx.Saga.RejectionComment = ctx.Message.Comment;
                    ctx.Saga.ManagerActedAt = DateTime.UtcNow;
                })
                // COMPENSATING TRANSACTION
                .Publish(ctx => new RestoreLeaveBalance
                {
                    LeaveId = ctx.Saga.LeaveId,
                    UserId = ctx.Saga.UserId,
                    Days = ctx.Saga.Days,
                    Year = DateTime.UtcNow.Year,
                    LeaveTypeId = ctx.Saga.LeaveTypeId,
                    Reason = "Manager rejected leave request"
                })
                .Publish(ctx => new SendLeaveNotification
                {
                    UserId = ctx.Saga.UserId,
                    NotifType = "LeaveRejected",
                    LeaveTypeName = ctx.Saga.LeaveTypeName,
                    StartDate = ctx.Saga.StartDate,
                    EndDate = ctx.Saga.EndDate,
                    Comment = ctx.Saga.RejectionComment
                })
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .TransitionTo(Rejected),

            When(LeaveCancelled)
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .Publish(ctx => new RestoreLeaveBalance
                {
                    LeaveId = ctx.Saga.LeaveId,
                    UserId = ctx.Saga.UserId,
                    Days = ctx.Saga.Days,
                    Year = DateTime.UtcNow.Year,
                    LeaveTypeId = ctx.Saga.LeaveTypeId,
                    Reason = "Employee cancelled leave request"
                })
                .TransitionTo(Cancelled)
        );

        // ── WAITING HR APPROVAL ───────────────────────────────────────────────
        During(WaitingHrApproval,

            When(HrApproved)
                .Then(ctx =>
                {
                    ctx.Saga.HrId = ctx.Message.HrId;
                    ctx.Saga.HrActedAt = DateTime.UtcNow;
                })
                .Publish(ctx => new ApproveLeaveBalance
                {
                    LeaveId = ctx.Saga.LeaveId,
                    UserId = ctx.Saga.UserId,
                    Days = ctx.Saga.Days,
                    Year = DateTime.UtcNow.Year,
                    LeaveTypeId = ctx.Saga.LeaveTypeId
                })
                .Publish(ctx => new SendLeaveNotification
                {
                    UserId = ctx.Saga.UserId,
                    NotifType = "LeaveApproved",
                    LeaveTypeName = ctx.Saga.LeaveTypeName,
                    StartDate = ctx.Saga.StartDate,
                    EndDate = ctx.Saga.EndDate
                })
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .TransitionTo(Approved),

            When(HrRejected)
                .Then(ctx =>
                {
                    ctx.Saga.HrId = ctx.Message.HrId;
                    ctx.Saga.RejectionComment = ctx.Message.Comment;
                    ctx.Saga.HrActedAt = DateTime.UtcNow;
                })
                .Publish(ctx => new RestoreLeaveBalance
                {
                    LeaveId = ctx.Saga.LeaveId,
                    UserId = ctx.Saga.UserId,
                    Days = ctx.Saga.Days,
                    Year = DateTime.UtcNow.Year,
                    LeaveTypeId = ctx.Saga.LeaveTypeId,
                    Reason = "HR rejected leave request"
                })
                .Publish(ctx => new SendLeaveNotification
                {
                    UserId = ctx.Saga.UserId,
                    NotifType = "LeaveRejected",
                    LeaveTypeName = ctx.Saga.LeaveTypeName,
                    StartDate = ctx.Saga.StartDate,
                    EndDate = ctx.Saga.EndDate,
                    Comment = ctx.Saga.RejectionComment
                })
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .TransitionTo(Rejected)
        );

        // ── APPROVED — Employee cancels after approval ────────────────────────
        During(Approved,
            When(LeaveCancelled)
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .Publish(ctx => new RestoreLeaveBalance
                {
                    LeaveId = ctx.Saga.LeaveId,
                    UserId = ctx.Saga.UserId,
                    Days = ctx.Saga.Days,
                    Year = DateTime.UtcNow.Year,
                    LeaveTypeId = ctx.Saga.LeaveTypeId,
                    Reason = "Employee cancelled approved leave"
                })
                .TransitionTo(Cancelled)
        );

        // ✅ FIX 4: Mark final states
        // This tells MassTransit the saga is complete — row can be deleted
        SetCompletedWhenFinalized();
    }
}