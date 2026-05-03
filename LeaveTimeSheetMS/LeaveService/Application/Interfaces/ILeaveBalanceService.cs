using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveBalanceDtos;

namespace LeaveService.Application.Interfaces;

public interface ILeaveBalanceService
{
    Task<IEnumerable<LeaveBalanceDto>> GetMyBalancesAsync(int userId, int? year = null);
    Task<IEnumerable<LeaveBalanceDto>> GetByUserIdAsync(int userId, int? year = null);
    Task InitializeBalancesForNewUserAsync(int userId, int year, string? gender = null);
    Task AdjustAsync(AdjustBalanceDto dto);         // FR-LB-004
    Task ProcessCarryForwardAsync(int from, int to); // FR-LB-005
}