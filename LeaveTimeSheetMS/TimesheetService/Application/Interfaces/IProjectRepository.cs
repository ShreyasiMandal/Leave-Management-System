using TimesheetService.Domain.Entities;

namespace TimesheetService.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<IEnumerable<Project>> GetAllAsync(bool includeInactive = false);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
}