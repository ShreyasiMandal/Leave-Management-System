namespace LeaveService.Application.DTOs.HolidayDtos
{
    public class HolidayDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public string Applicability { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public bool IsActive { get; set; }
    }
}
