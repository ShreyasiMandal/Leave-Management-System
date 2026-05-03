using TimesheetService.Application.DTOs;
using TimesheetService.Application.DTOs.ProjectDTOs;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Entities;

namespace TimesheetService.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repo;

    public ProjectService(IProjectRepository repo) => _repo = repo;

    public async Task<IEnumerable<ProjectDto>> GetAllAsync(bool includeInactive = false)
        => (await _repo.GetAllAsync(includeInactive)).Select(Map);

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p == null ? null : Map(p);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        if (await _repo.CodeExistsAsync(dto.Code))
            throw new InvalidOperationException(
                $"Project code '{dto.Code}' already exists.");

        var project = new Project
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            ClientName = dto.ClientName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(project);
        return Map(project);
    }

    public async Task DeactivateAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Project {id} not found.");
        p.IsActive = false;
        await _repo.UpdateAsync(p);
    }

    private static ProjectDto Map(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Code = p.Code,
        ClientName = p.ClientName,
        IsActive = p.IsActive
    };
}