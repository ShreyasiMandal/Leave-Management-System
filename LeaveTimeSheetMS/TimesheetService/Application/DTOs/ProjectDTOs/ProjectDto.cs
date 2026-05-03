namespace TimesheetService.Application.DTOs.ProjectDTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public bool IsActive { get; set; }
    }
}
