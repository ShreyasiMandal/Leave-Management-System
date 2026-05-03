using Microsoft.EntityFrameworkCore;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Entities;
using TimesheetService.Infrastructure.Data;

namespace TimesheetService.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly TimesheetDbContext _ctx;
    public ProjectRepository(TimesheetDbContext ctx) => _ctx = ctx;

    public async Task<Project?> GetByIdAsync(int id)
        => await _ctx.Projects.FindAsync(id);

    public async Task<IEnumerable<Project>> GetAllAsync(bool includeInactive = false)
    {
        var q = _ctx.Projects.AsQueryable();
        if (!includeInactive) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        => await _ctx.Projects.AnyAsync(x =>
            x.Code == code.ToUpper() &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

    public async Task AddAsync(Project project)
    {
        await _ctx.Projects.AddAsync(project);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _ctx.Projects.Update(project);
        await _ctx.SaveChangesAsync();
    }
}