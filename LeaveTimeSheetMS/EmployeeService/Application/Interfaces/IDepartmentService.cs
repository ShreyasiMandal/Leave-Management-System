using EmployeeService.Application.DTOs.DepartmentRequestDTOs;
using EmployeeService.Application.DTOs.DepartmentResponseDTOs;

namespace EmployeeService.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto);
        Task ReactivateAsync(int id);
        Task DeactivateAsync(int id);
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentDto>> GetAllAsync(bool includeInactive = false);
    }
}
