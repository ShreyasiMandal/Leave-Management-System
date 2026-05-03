using TimesheetService.Application.DTOs;
using TimesheetService.Application.DTOs.ProjectDTOs;

namespace TimesheetService.Application.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllAsync(bool includeInactive = false);
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto);
    Task DeactivateAsync(int id);
}