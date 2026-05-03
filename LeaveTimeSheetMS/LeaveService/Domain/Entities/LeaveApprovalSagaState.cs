using MassTransit;

namespace LeaveService.Domain.Entities;

public class LeaveApprovalSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    // ✅ FIX: Must be non-null string, not null
    public string CurrentState { get; set; } = "Initial";

    public int LeaveId { get; set; }
    public int UserId { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal Days { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool NeedsHrApproval { get; set; }
    public int? ManagerId { get; set; }
    public int? HrId { get; set; }
    public string? RejectionComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ManagerActedAt { get; set; }
    public DateTime? HrActedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}