using LeaveService.Domain.Entities;

namespace LeaveService.Application.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetAsync(int userId, int leaveTypeId, int year);
    Task<IEnumerable<LeaveBalance>> GetAllByUserAsync(int userId, int year);
    Task<IEnumerable<LeaveBalance>> GetAllByLeaveTypeAsync(int leaveTypeId, int year);
    Task AddAsync(LeaveBalance balance);
    Task UpdateAsync(LeaveBalance balance);
    Task AddRangeAsync(IEnumerable<LeaveBalance> balances);
}