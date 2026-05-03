namespace TimesheetService.Application.DTOs.TimesheetDTOs
{
    public class WeeklyTimesheetDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<TimesheetEntryDto> Entries { get; set; } = new();
        public decimal TotalHours { get; set; }
        public string WeekStatus { get; set; } = string.Empty;

        // FR-TS-005: Warnings
        public bool HoursExceed12 { get; set; }
        public bool HoursBelowThreshold { get; set; }
        public decimal MinThreshold { get; set; } = 40;

        // FR-TA-004: Overdue flag
        public bool IsOverdue { get; set; }
    }
}
