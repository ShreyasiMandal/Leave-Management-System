using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.HolidayDtos;

namespace LeaveService.Application.Interfaces;

public interface IHolidayService
{
    Task<IEnumerable<HolidayDto>> GetByYearAsync(int year);
    Task<HolidayDto> CreateAsync(CreateHolidayDto dto);
    Task DeleteAsync(int id);
    Task<int> CopyCalendarAsync(CopyCalendarDto dto); // FR-HC-003
}