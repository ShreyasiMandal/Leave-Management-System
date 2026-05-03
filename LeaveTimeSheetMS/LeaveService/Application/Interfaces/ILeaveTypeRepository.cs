using LeaveService.Domain.Entities;

namespace LeaveService.Application.Interfaces;

public interface ILeaveTypeRepository
{
    Task<LeaveType?> GetByIdAsync(int id);
    Task<IEnumerable<LeaveType>> GetAllAsync(bool includeInactive = false);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task AddAsync(LeaveType leaveType);
    Task UpdateAsync(LeaveType leaveType);
    Task<IEnumerable<LeaveType>> GetAllActiveAsync();
}