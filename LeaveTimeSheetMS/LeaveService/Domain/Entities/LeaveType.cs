namespace LeaveService.Domain.Entities;

// FR-LT-001, FR-LT-002, FR-LT-003, FR-LT-004, FR-LA-006
public class LeaveType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
    public string AccrualFrequency { get; set; } = "Monthly";
    public int CarryForwardMax { get; set; } = 0;
    public string? GenderApplicability { get; set; } // null=All, "Female"=Female only
    public bool IsDocumentRequired { get; set; } = false;
    public bool IsAutoApprove { get; set; } = false; // FR-LA-006
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
  


    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}