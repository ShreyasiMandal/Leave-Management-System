namespace LeaveService.Application.DTOs.LeaveRequestDtos
{
    public class LeaveApplyResultDto
    {
        public LeaveRequestDto Leave { get; set; } = null!;
        public bool InsufficientBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string? Warning { get; set; }
    }
}
