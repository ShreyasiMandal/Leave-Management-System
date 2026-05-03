namespace ReportService.Application.DTOs
{
    public class AttendanceSummaryDto
    {
        public DateTime Date { get; set; }
        public int PresentCount { get; set; }
        public int OnLeaveCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalCount { get; set; }
    }
}
