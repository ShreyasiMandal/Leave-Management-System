namespace EmployeeService.Application.DTOs.DepartmentResponseDTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? HeadId { get; set; }
        public string? HeadName { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
    }
}
