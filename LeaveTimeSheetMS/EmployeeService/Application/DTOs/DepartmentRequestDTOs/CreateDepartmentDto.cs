namespace EmployeeService.Application.DTOs.DepartmentRequestDTOs
{
    public class CreateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? HeadId { get; set; }
    }
}
