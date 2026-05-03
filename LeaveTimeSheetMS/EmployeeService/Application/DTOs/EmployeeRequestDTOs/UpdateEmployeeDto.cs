namespace EmployeeService.Application.DTOs.EmployeeRequestDTOs
{
    public class UpdateEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string EmploymentType { get; set; } = "Full-time";
        public int DepartmentId { get; set; }
        public int? ManagerId { get; set; }
        public DateTime DateOfJoining { get; set; }
    }
}
