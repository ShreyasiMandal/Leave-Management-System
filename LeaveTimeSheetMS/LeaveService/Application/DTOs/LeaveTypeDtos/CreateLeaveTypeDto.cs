namespace LeaveService.Application.DTOs.LeaveTypeDtos
{
    public class CreateLeaveTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int MaxDaysPerYear { get; set; }
        public bool IsPaid { get; set; } = true;
        public string AccrualFrequency { get; set; } = "Monthly";
        public int CarryForwardMax { get; set; } = 0;
        public string? GenderApplicability { get; set; }
        public bool IsDocumentRequired { get; set; } = false;
        public bool IsAutoApprove { get; set; } = false;
    }
}
