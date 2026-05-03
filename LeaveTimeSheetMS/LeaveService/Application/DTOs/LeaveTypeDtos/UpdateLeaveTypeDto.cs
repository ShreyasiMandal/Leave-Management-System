namespace LeaveService.Application.DTOs.LeaveTypeDtos
{
    public class UpdateLeaveTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public int MaxDaysPerYear { get; set; }
        public bool IsPaid { get; set; }
        public string AccrualFrequency { get; set; } = "Monthly";
        public int CarryForwardMax { get; set; }
        public string? GenderApplicability { get; set; }
        public bool IsDocumentRequired { get; set; }
        public bool IsAutoApprove { get; set; }
    }
}
