namespace LeaveService.Domain.Entities;

// FR-LB-001 to FR-LB-006
public class LeaveBalance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
    public int Year { get; set; }
    public decimal Entitled { get; set; }
    public decimal Used { get; set; }
    public decimal Pending { get; set; }
    public decimal Carried { get; set; }
    public string? AdjustmentReason { get; set; } // FR-LB-004
    public decimal Available => Entitled + Carried - Used - Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}