using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly NotificationDbContext _ctx;
    public TemplateRepository(NotificationDbContext ctx) => _ctx = ctx;

    public async Task<NotificationTemplate?> GetByTypeAsync(string type)
        => await _ctx.Templates
            .FirstOrDefaultAsync(t => t.Type == type && t.IsActive);

    public async Task<IEnumerable<NotificationTemplate>> GetAllAsync()
        => await _ctx.Templates.OrderBy(t => t.Type).ToListAsync();

    public async Task AddAsync(NotificationTemplate template)
    {
        await _ctx.Templates.AddAsync(template);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(NotificationTemplate template)
    {
        _ctx.Templates.Update(template);
        await _ctx.SaveChangesAsync();
    }
}