
using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetByIdAsync(int id);
        Task<Department?> GetByNameAsync(string name);
        Task<IEnumerable<Department>> GetAllAsync(bool includeInactive = false);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> GetEmployeeCountAsync(int departmentId);
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
    }
}
