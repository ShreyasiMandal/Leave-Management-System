namespace LeaveService.Application.DTOs;

public class LeaveBalanceDto
{
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Entitled { get; set; }
    public decimal Carried { get; set; }
    public decimal Used { get; set; }
    public decimal Pending { get; set; }
    public decimal Available { get; set; }
    public bool IsPaid { get; set; }
}