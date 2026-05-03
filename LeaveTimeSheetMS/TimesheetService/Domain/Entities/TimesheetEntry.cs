namespace TimesheetService.Domain.Entities;

/// <summary>
/// FR-TS-001 to FR-TS-008, FR-TA-001 to FR-TA-005
/// One record = one employee's hours for one day on one project.
/// </summary>
public class TimesheetEntry
{
    public int Id { get; set; }

    // Cross-service ref — no EF FK to AuthService/EmployeeService
    public int UserId { get; set; }

    public DateTime Date { get; set; }   // FR-TS-003: no future dates
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public decimal Hours { get; set; }   // FR-TS-001: hours worked
    public string Description { get; set; } = string.Empty;

    // FR-TS-001: Regular | Overtime | On-Call
    public string Category { get; set; } = "Regular";

    // FR-TS-007: Draft | Submitted | Approved | Rejected | Locked
    public string Status { get; set; } = "Draft";

    // The Monday of the week this entry belongs to
    public DateTime WeekStart { get; set; }

    // FR-TA-002: Approver info
    public int? ApproverId { get; set; }
    public string? ApproverComment { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // FR-TA-004: Late submission flag
    public bool IsLateSubmission { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}