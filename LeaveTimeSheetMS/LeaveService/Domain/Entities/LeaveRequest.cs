namespace LeaveService.Domain.Entities;

// FR-LR-001 to FR-LR-008, FR-LA-001 to FR-LA-007
public class LeaveRequest
{
    public int Id { get; set; }
    public int UserId { get; set; } // Cross-service ref to AuthService
    public Guid? CorrelationId { get; set; }
    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Days { get; set; } // FR-LR-001: auto-calculated
    public string HalfDaySession { get; set; } = "Full"; // FR-LR-004
    public string Status { get; set; } = "Draft"; // FR-LR-007
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }

    // FR-LA-001: First level — Manager
    public int? ManagerApproverId { get; set; }
    public string? ManagerComment { get; set; }
    public DateTime? ManagerActedAt { get; set; }

    // FR-LA-003: Second level — HR (for > 5 days)
    public int? HrApproverId { get; set; }
    public string? HrComment { get; set; }
    public DateTime? HrActedAt { get; set; }
    public bool NeedsHrApproval { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}