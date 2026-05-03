namespace LeaveService.Application.DTOs.LeaveRequestDtos
{
    public class CreateLeaveRequestDto
    {
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string HalfDaySession { get; set; } = "Full";
        public string Reason { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
    }
}
