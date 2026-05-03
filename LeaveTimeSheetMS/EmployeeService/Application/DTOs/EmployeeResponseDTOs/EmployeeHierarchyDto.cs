namespace EmployeeService.Application.DTOs.EmployeeResponseDTOs
{
    public class EmployeeHierarchyDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public EmployeeHierarchyDto? Manager { get; set; }
        public List<EmployeeHierarchyDto> DirectReports { get; set; } = new();
    }
}
