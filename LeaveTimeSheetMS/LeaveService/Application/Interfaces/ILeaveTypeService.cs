using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveTypeDtos;

namespace LeaveService.Application.Interfaces;

public interface ILeaveTypeService
{
    Task<IEnumerable<LeaveTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<LeaveTypeDto?> GetByIdAsync(int id);
    Task<LeaveTypeDto> CreateAsync(CreateLeaveTypeDto dto);
    Task<LeaveTypeDto> UpdateAsync(int id, UpdateLeaveTypeDto dto);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}