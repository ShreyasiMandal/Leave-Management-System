namespace TimesheetService.Application.DTOs.TimesheetDTOs
{
    public class TimesheetLogResultDto
    {
        public TimesheetEntryDto Entry { get; set; } = null!;
        public bool ExceedsMaxHours { get; set; }
        public bool BelowThreshold { get; set; }
        public decimal DailyTotal { get; set; }
        public string? Warning { get; set; }
    }
}
