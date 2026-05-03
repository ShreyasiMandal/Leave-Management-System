namespace LeaveService.Application.DTOs.LeaveTypeDtos
{
    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int MaxDaysPerYear { get; set; }
        public bool IsPaid { get; set; }
        public string AccrualFrequency { get; set; } = string.Empty;
        public int CarryForwardMax { get; set; }
        public string? GenderApplicability { get; set; }
        public bool IsDocumentRequired { get; set; }
        public bool IsAutoApprove { get; set; }
        public bool IsActive { get; set; }
    }
}
