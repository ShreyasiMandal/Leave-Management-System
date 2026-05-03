using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.HolidayDtos;
using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;

namespace LeaveService.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly IHolidayRepository _repo;

    public HolidayService(IHolidayRepository repo) => _repo = repo;

    public async Task<IEnumerable<HolidayDto>> GetByYearAsync(int year)
        => (await _repo.GetByYearAsync(year)).Select(Map);

    public async Task<HolidayDto> CreateAsync(CreateHolidayDto dto)
    {
        var h = new Holiday
        {
            Name = dto.Name,
            Date = dto.Date.Date,
            Year = dto.Date.Year,
            Applicability = dto.Applicability,
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(h);
        return Map(h);
    }

    public async Task DeleteAsync(int id)
    {
        var h = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Holiday {id} not found.");
        await _repo.DeleteAsync(h);
    }

    // FR-HC-003: Copy all holidays from one year to next
    public async Task<int> CopyCalendarAsync(CopyCalendarDto dto)
    {
        var source = (await _repo.GetAllByYearAsync(dto.FromYear)).ToList();
        if (!source.Any())
            throw new InvalidOperationException(
                $"No holidays found for {dto.FromYear}.");

        var copies = source.Select(h => new Holiday
        {
            Name = h.Name,
            Date = new DateTime(dto.ToYear, h.Date.Month, h.Date.Day),
            Year = dto.ToYear,
            Applicability = h.Applicability,
            DepartmentId = h.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _repo.AddRangeAsync(copies);
        return copies.Count;
    }

    private static HolidayDto Map(Holiday h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        Date = h.Date,
        Year = h.Year,
        Applicability = h.Applicability,
        DepartmentId = h.DepartmentId,
        IsActive = h.IsActive
    };
}