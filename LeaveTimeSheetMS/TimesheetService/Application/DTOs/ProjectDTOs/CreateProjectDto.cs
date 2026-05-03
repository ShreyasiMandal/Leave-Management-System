namespace TimesheetService.Application.DTOs.ProjectDTOs
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? ClientName { get; set; }
    }
}
