namespace TimesheetService.Application.DTOs.TimesheetDTOs
{
    public class CreateTimesheetEntryDto
    {
        public DateTime Date { get; set; }
        public int ProjectId { get; set; }
        public decimal Hours { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Regular";
    }
}
