using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;
using LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Infrastructure.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly LeaveDbContext _ctx;
    public LeaveBalanceRepository(LeaveDbContext ctx) => _ctx = ctx;

    public async Task<LeaveBalance?> GetAsync(int userId, int leaveTypeId, int year)
        => await _ctx.LeaveBalances
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.LeaveTypeId == leaveTypeId &&
                x.Year == year);

    public async Task<IEnumerable<LeaveBalance>> GetAllByUserAsync(int userId, int year)
        => await _ctx.LeaveBalances
            .Include(x => x.LeaveType)
            .Where(x => x.UserId == userId && x.Year == year)
            .ToListAsync();

    public async Task<IEnumerable<LeaveBalance>> GetAllByLeaveTypeAsync(
        int leaveTypeId, int year)
        => await _ctx.LeaveBalances
            .Where(x => x.LeaveTypeId == leaveTypeId && x.Year == year)
            .ToListAsync();

    public async Task AddAsync(LeaveBalance b)
    {
        await _ctx.LeaveBalances.AddAsync(b);
        await _ctx.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<LeaveBalance> balances)
    {
        await _ctx.LeaveBalances.AddRangeAsync(balances);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveBalance b)
    {
        _ctx.LeaveBalances.Update(b);
        await _ctx.SaveChangesAsync();
    }
}