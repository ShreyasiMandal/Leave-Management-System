namespace TimesheetService.Application.DTOs.TimesheetDTOs
{
    public class TeamTimesheetSummaryDto
    {
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public decimal TotalHours { get; set; }
        public string WeekStatus { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public bool IsOverdue { get; set; }
        public List<TimesheetEntryDto> Entries { get; set; } = new();
    }
}
