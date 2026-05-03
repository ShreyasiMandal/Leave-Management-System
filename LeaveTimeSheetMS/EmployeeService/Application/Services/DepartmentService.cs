using EmployeeService.Application.DTOs;
using EmployeeService.Application.DTOs.DepartmentRequestDTOs;
using EmployeeService.Application.DTOs.DepartmentResponseDTOs;
using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;

namespace EmployeeService.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _deptRepo;
    private readonly IEmployeeRepository _empRepo;

    public DepartmentService(
        IDepartmentRepository deptRepo,
        IEmployeeRepository empRepo)
    {
        _deptRepo = deptRepo;
        _empRepo = empRepo;
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        if (await _deptRepo.NameExistsAsync(dto.Name))
            throw new InvalidOperationException(
                $"Department '{dto.Name}' already exists.");

        if (dto.HeadId.HasValue &&
            await _empRepo.GetByIdAsync(dto.HeadId.Value) == null)
            throw new InvalidOperationException(
                $"Employee ID {dto.HeadId} not found.");

        var dept = new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            HeadId = dto.HeadId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _deptRepo.AddAsync(dept);
        return await BuildDtoAsync(dept);
    }

    public async Task<DepartmentDto> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var dept = await _deptRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Department ID {id} not found.");

        if (await _deptRepo.NameExistsAsync(dto.Name, excludeId: id))
            throw new InvalidOperationException(
                $"Department name '{dto.Name}' already exists.");

        dept.Name = dto.Name;
        dept.Description = dto.Description;
        dept.HeadId = dto.HeadId;

        await _deptRepo.UpdateAsync(dept);
        return await BuildDtoAsync(dept);
    }

    public async Task DeactivateAsync(int id)
    {
        var dept = await _deptRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Department ID {id} not found.");

        var count = await _deptRepo.GetEmployeeCountAsync(id);
        if (count > 0)
            throw new InvalidOperationException(
                $"Cannot deactivate '{dept.Name}' — {count} active employee(s) assigned. " +
                "Reassign them first.");

        dept.IsActive = false;
        await _deptRepo.UpdateAsync(dept);
    }

    public async Task ReactivateAsync(int id)
    {
        var dept = await _deptRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Department ID {id} not found.");

        if (dept.IsActive)
            throw new InvalidOperationException("Department is already active.");

        dept.IsActive = true;
        await _deptRepo.UpdateAsync(dept);
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var dept = await _deptRepo.GetByIdAsync(id);
        return dept == null ? null : await BuildDtoAsync(dept);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(bool includeInactive = false)
    {
        var depts = await _deptRepo.GetAllAsync(includeInactive);
        var result = new List<DepartmentDto>();
        foreach (var d in depts)
            result.Add(await BuildDtoAsync(d));
        return result;
    }

    private async Task<DepartmentDto> BuildDtoAsync(Department d)
    {
        var headName = d.HeadId.HasValue
            ? (await _empRepo.GetByIdAsync(d.HeadId.Value))?.FullName
            : null;

        return new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            HeadId = d.HeadId,
            HeadName = headName,
            EmployeeCount = await _deptRepo.GetEmployeeCountAsync(d.Id),
            IsActive = d.IsActive
        };
    }
}