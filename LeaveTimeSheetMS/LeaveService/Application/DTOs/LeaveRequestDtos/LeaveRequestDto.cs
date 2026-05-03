namespace LeaveService.Application.DTOs.LeaveRequestDtos
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Guid? CorrelationId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveTypeCode { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Days { get; set; }
        public string HalfDaySession { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string? ManagerComment { get; set; }
        public string? HrComment { get; set; }
        public bool NeedsHrApproval { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
