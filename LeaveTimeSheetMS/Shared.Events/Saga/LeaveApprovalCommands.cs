namespace Shared.Events.Saga;

// ── COMMANDS (instructions sent to SAGA) ─────────────────────────────────────

/// <summary>
/// Sent by LeaveController when employee submits leave.
/// Starts the SAGA.
/// </summary>
public class StartLeaveApprovalSaga
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public int LeaveId { get; set; }
    public int UserId { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal Days { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool NeedsHrApproval { get; set; }
}

/// <summary>
/// Sent by ManagerController when manager approves.
/// </summary>
public class ManagerApprovedLeave
{
    public Guid CorrelationId { get; set; }
    public int LeaveId { get; set; }
    public int ManagerId { get; set; }
}

/// <summary>
/// Sent by ManagerController when manager rejects.
/// </summary>
public class ManagerRejectedLeave
{
    public Guid CorrelationId { get; set; }
    public int LeaveId { get; set; }
    public int ManagerId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// Sent by HRController when HR approves (second level).
/// </summary>
public class HrApprovedLeave
{
    public Guid CorrelationId { get; set; }
    public int LeaveId { get; set; }
    public int HrId { get; set; }
}

/// <summary>
/// Sent by HRController when HR rejects.
/// </summary>
public class HrRejectedLeave
{
    public Guid CorrelationId { get; set; }
    public int LeaveId { get; set; }
    public int HrId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// Sent when employee cancels leave.
/// SAGA runs compensating transaction — restores balance.
/// </summary>
public class CancelLeaveRequest
{
    public Guid CorrelationId { get; set; }
    public int LeaveId { get; set; }
    public int UserId { get; set; }
}