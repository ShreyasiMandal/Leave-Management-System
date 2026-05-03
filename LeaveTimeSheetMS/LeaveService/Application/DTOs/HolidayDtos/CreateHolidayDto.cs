namespace LeaveService.Application.DTOs.HolidayDtos
{
    public class CreateHolidayDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Applicability { get; set; } = "Global";
        public int? DepartmentId { get; set; }
    }
}
