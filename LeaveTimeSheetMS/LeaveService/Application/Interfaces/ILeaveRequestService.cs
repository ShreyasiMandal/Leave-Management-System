using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveRequestDtos;

namespace LeaveService.Application.Interfaces;

public interface ILeaveRequestService
{
    // Employee
    Task<LeaveApplyResultDto> ApplyAsync(int userId, CreateLeaveRequestDto dto);
    Task<LeaveRequestDto> SaveDraftAsync(int userId, CreateLeaveRequestDto dto);
    Task<IEnumerable<LeaveRequestDto>> GetMyLeavesAsync(int userId);
    Task<LeaveRequestDto?> GetByIdAsync(int id);
    Task CancelAsync(int leaveId, int userId);

    // Manager
    Task<IEnumerable<LeaveRequestDto>> GetPendingForManagerAsync();
    Task ApproveAsync(int leaveId, int approverUserId);
    Task RejectAsync(int leaveId, int approverUserId,
                                        string comment);
    // HR second-level
    Task<IEnumerable<LeaveRequestDto>> GetPendingHrAsync();
    Task HrApproveAsync(int leaveId, int hrUserId);
    Task HrRejectAsync(int leaveId, int hrUserId,
                                        string comment);
}