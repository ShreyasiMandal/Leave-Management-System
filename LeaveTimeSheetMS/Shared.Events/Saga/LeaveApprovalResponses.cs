namespace Shared.Events.Saga;

// ── RESPONSES (saga tells other services what happened) ──────────────────────

/// <summary>
/// SAGA tells LeaveService to update status + deduct balance.
/// </summary>
public class ApproveLeaveBalance
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }
    public decimal Days { get; set; }
    public int Year { get; set; }
    public int LeaveTypeId { get; set; }
}

/// <summary>
/// SAGA tells LeaveService to restore balance (compensating transaction).
/// Called when SAGA needs to UNDO an approval.
/// </summary>
public class RestoreLeaveBalance
{
    public int LeaveId { get; set; }
    public int UserId { get; set; }
    public decimal Days { get; set; }
    public int Year { get; set; }
    public int LeaveTypeId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// SAGA tells NotificationService to send notification.
/// </summary>
public class SendLeaveNotification
{
    public int UserId { get; set; }
    public string NotifType { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Comment { get; set; }
}