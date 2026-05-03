using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;
using LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Infrastructure.Repositories;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly LeaveDbContext _ctx;
    public LeaveTypeRepository(LeaveDbContext ctx) => _ctx = ctx;

    public async Task<LeaveType?> GetByIdAsync(int id)
        => await _ctx.LeaveTypes.FindAsync(id);

    public async Task<IEnumerable<LeaveType>> GetAllAsync(bool includeInactive = false)
    {
        var q = _ctx.LeaveTypes.AsQueryable();
        if (!includeInactive) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<IEnumerable<LeaveType>> GetAllActiveAsync()
    {
        return await _ctx.LeaveTypes
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        => await _ctx.LeaveTypes.AnyAsync(x =>
            x.Code == code.ToUpper() &&
            (!excludeId.HasValue || x.Id != excludeId.Value));

    public async Task AddAsync(LeaveType lt)
    {
        await _ctx.LeaveTypes.AddAsync(lt);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveType lt)
    {
        _ctx.LeaveTypes.Update(lt);
        await _ctx.SaveChangesAsync();
    }
}