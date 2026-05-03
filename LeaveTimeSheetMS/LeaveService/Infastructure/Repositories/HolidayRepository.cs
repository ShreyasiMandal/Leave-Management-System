using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;
using LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Infrastructure.Repositories;

public class HolidayRepository : IHolidayRepository
{
    private readonly LeaveDbContext _ctx;
    public HolidayRepository(LeaveDbContext ctx) => _ctx = ctx;

    public async Task<Holiday?> GetByIdAsync(int id)
        => await _ctx.Holidays.FindAsync(id);

    public async Task<IEnumerable<Holiday>> GetByYearAsync(int year)
        => await _ctx.Holidays
            .Where(x => x.Year == year && x.IsActive)
            .OrderBy(x => x.Date)
            .ToListAsync();

    public async Task<IEnumerable<Holiday>> GetAllByYearAsync(int year)
        => await _ctx.Holidays
            .Where(x => x.Year == year && x.IsActive)
            .ToListAsync();

    public async Task<bool> IsHolidayAsync(DateTime date)
        => await _ctx.Holidays
            .AnyAsync(x => x.Date.Date == date.Date && x.IsActive);

    public async Task AddAsync(Holiday h)
    {
        await _ctx.Holidays.AddAsync(h);
        await _ctx.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Holiday> holidays)
    {
        await _ctx.Holidays.AddRangeAsync(holidays);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Holiday h)
    {
        _ctx.Holidays.Update(h);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Holiday h)
    {
        _ctx.Holidays.Remove(h);
        await _ctx.SaveChangesAsync();
    }
}