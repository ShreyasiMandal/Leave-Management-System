namespace ReportService.Application.DTOs
{
    public class LeaveReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public string? LeaveType { get; set; }
        public string? Status { get; set; }
    }
}
