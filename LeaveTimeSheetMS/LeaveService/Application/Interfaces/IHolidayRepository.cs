using LeaveService.Domain.Entities;

namespace LeaveService.Application.Interfaces;

public interface IHolidayRepository
{
    Task<Holiday?> GetByIdAsync(int id);
    Task<IEnumerable<Holiday>> GetByYearAsync(int year);
    Task<IEnumerable<Holiday>> GetAllByYearAsync(int year); // for copy
    Task<bool> IsHolidayAsync(DateTime date);
    Task AddAsync(Holiday holiday);
    Task AddRangeAsync(IEnumerable<Holiday> holidays);
    Task UpdateAsync(Holiday holiday);
    Task DeleteAsync(Holiday holiday);
}