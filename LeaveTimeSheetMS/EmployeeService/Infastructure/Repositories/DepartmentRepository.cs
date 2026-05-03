using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;
using EmployeeService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly EmployeeDbContext _context;

    public DepartmentRepository(EmployeeDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(int id)
        => await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Department?> GetByNameAsync(string name)
        => await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == name);

    public async Task<IEnumerable<Department>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Departments.AsQueryable();
        if (!includeInactive)
            query = query.Where(d => d.IsActive);
        return await query.OrderBy(d => d.Name).ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        => await _context.Departments
            .AnyAsync(d => d.Name == name
                        && (!excludeId.HasValue || d.Id != excludeId.Value));

    public async Task<int> GetEmployeeCountAsync(int departmentId)
        => await _context.Employees
            .CountAsync(e => e.DepartmentId == departmentId && e.IsActive);

    public async Task AddAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }
}