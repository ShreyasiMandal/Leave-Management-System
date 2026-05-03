namespace EmployeeService.Application.DTOs.EmployeeRequestDTOs
{
    public class CreateEmployeeDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string Designation { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = "Full-time";
        public int DepartmentId { get; set; }
        public int? ManagerId { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
