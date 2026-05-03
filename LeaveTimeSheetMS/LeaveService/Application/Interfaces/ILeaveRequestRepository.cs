using LeaveService.Domain.Entities;

namespace LeaveService.Application.Interfaces;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(int userId);
    Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync();   // FR-LA-001
    Task<IEnumerable<LeaveRequest>> GetPendingHrApprovalAsync();   // FR-LA-003
    Task<bool> HasOverlapAsync(int userId,
        DateTime start, DateTime end, int? excludeId = null);      // FR-LR-002
    Task AddAsync(LeaveRequest request);
    Task UpdateAsync(LeaveRequest request);
}