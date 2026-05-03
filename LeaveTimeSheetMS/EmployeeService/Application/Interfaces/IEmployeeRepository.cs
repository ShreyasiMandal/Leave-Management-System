using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByUserIdAsync(int userId);
        Task<Employee?> GetByEmailAsync(string email);
        Task<IEnumerable<Employee>> GetAllAsync(bool includeInactive = false);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
        Task<IEnumerable<Employee>> GetDirectReportsAsync(int managerId);
        Task<Employee?> GetWithManagerChainAsync(int id);
        Task<bool> UserIdExistsAsync(int userId);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<string> GenerateEmployeeCodeAsync();
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            string? search = null,
            int? departmentId = null,
            bool includeInactive = false);
    }
}
