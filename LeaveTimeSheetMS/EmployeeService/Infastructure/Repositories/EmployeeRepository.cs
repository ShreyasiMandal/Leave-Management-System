using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;
using EmployeeService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeDbContext _context;

    public EmployeeRepository(EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id)
        => await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Employee?> GetByUserIdAsync(int userId)
        => await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.UserId == userId);

    public async Task<Employee?> GetByEmailAsync(string email)
        => await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email == email.ToLower());

    public async Task<IEnumerable<Employee>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(e => e.IsActive);

        return await query.OrderBy(e => e.FullName).ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        => await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Where(e => e.DepartmentId == departmentId && e.IsActive)
            .OrderBy(e => e.FullName)
            .ToListAsync();

    public async Task<IEnumerable<Employee>> GetDirectReportsAsync(int managerId)
        => await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.ManagerId == managerId && e.IsActive)
            .OrderBy(e => e.FullName)
            .ToListAsync();

    public async Task<Employee?> GetWithManagerChainAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.DirectReports)
                .ThenInclude(r => r.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return null;

        await LoadManagerChainAsync(employee);
        return employee;
    }

    private async Task LoadManagerChainAsync(Employee employee, int depth = 0)
    {
        if (depth > 10 || !employee.ManagerId.HasValue) return;

        var manager = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == employee.ManagerId.Value);

        if (manager == null) return;

        employee.Manager = manager;
        await LoadManagerChainAsync(manager, depth + 1);
    }

    public async Task<bool> UserIdExistsAsync(int userId)
        => await _context.Employees.AnyAsync(e => e.UserId == userId);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await _context.Employees
            .AnyAsync(e => e.Email == email.ToLower()
                        && (!excludeId.HasValue || e.Id != excludeId.Value));

    public async Task<string> GenerateEmployeeCodeAsync()
    {
        var last = await _context.Employees
            .OrderByDescending(e => e.Id)
            .Select(e => e.EmployeeCode)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last != null && last.StartsWith("EMP-"))
            if (int.TryParse(last[4..], out int lastNum))
                next = lastNum + 1;

        return $"EMP-{next:D4}";
    }

    public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        int? departmentId = null,
        bool includeInactive = false)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(e => e.IsActive);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s) ||
                e.EmployeeCode.ToLower().Contains(s) ||
                e.Designation.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }
}