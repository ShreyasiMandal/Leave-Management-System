namespace LeaveService.Application.DTOs.LeaveBalanceDtos
{
    public class AdjustBalanceDto
    {
        public int UserId { get; set; }
        public int LeaveTypeId { get; set; }
        public decimal Adjustment { get; set; } // +ve = add, -ve = deduct
        public string Reason { get; set; } = string.Empty;
    }
}
