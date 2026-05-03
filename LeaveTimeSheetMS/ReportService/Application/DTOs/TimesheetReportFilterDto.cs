namespace ReportService.Application.DTOs
{
    public class TimesheetReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }
    }
}
