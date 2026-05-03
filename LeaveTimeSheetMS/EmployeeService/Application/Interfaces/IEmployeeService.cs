using EmployeeService.Application.DTOs.EmployeeRequestDTOs;
using EmployeeService.Application.DTOs.EmployeeResponseDTOs;

namespace EmployeeService.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
        Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto);
        Task DeactivateAsync(int id);
        Task ReactivateAsync(int id);
        Task<EmployeeDto?> GetByIdAsync(int id);
        Task<EmployeeDto?> GetByUserIdAsync(int userId);
        Task<(IEnumerable<EmployeeSummaryDto> Items, int TotalCount)> GetAllAsync(
            int page, int pageSize,
            string? search = null,
            int? departmentId = null,
            bool includeInactive = false);
        Task<IEnumerable<EmployeeSummaryDto>> GetMyTeamAsync(int managerId);
        Task<EmployeeHierarchyDto?> GetHierarchyAsync(int employeeId);
        Task UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto);
        Task CreateFromEventAsync(int userId, string fullName, string email, string? gender = null);
        Task<bool> ExistsByUserIdAsync(int userId);
        Task<string?> GetGenderByUserIdAsync(int userId);
        Task<int?> GetManagerUserIdAsync(int employeeUserId);
    }
}
