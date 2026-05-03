namespace TimesheetService.Application.DTOs.TimesheetDTOs
{
    public class TimesheetEntryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public string? ApproverComment { get; set; }
        public bool IsLateSubmission { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
