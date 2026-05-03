using EmployeeService.Application.DTOs;
using EmployeeService.Application.DTOs.EmployeeRequestDTOs;
using EmployeeService.Application.DTOs.EmployeeResponseDTOs;
using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Entities;
using System.Reflection;

namespace EmployeeService.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _deptRepo;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IDepartmentRepository deptRepo)
    {
        _employeeRepo = employeeRepo;
        _deptRepo = deptRepo;
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (await _employeeRepo.UserIdExistsAsync(dto.UserId))
            throw new InvalidOperationException(
                $"An employee profile already exists for UserId {dto.UserId}.");

        if (await _employeeRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException(
                $"Email '{dto.Email}' is already used by another employee.");

        var dept = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new InvalidOperationException(
                $"Department ID {dto.DepartmentId} not found.");

        if (dto.ManagerId.HasValue)
        {
            var mgr = await _employeeRepo.GetByIdAsync(dto.ManagerId.Value);
            if (mgr == null)
                throw new InvalidOperationException(
                    $"Manager with Employee ID {dto.ManagerId} not found.");
        }

        var code = await _employeeRepo.GenerateEmployeeCodeAsync();

        var employee = new Employee
        {
            UserId = dto.UserId,
            FullName = dto.FullName,
            Email = dto.Email.ToLower(),
            EmployeeCode = code,
            Designation = dto.Designation,
            EmploymentType = dto.EmploymentType,
            DepartmentId = dto.DepartmentId,
            ManagerId = dto.ManagerId,
            DateOfJoining = dto.DateOfJoining,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            IsActive = true,
            Gender = dto.Gender,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRepo.AddAsync(employee);

        var created = await _employeeRepo.GetByIdAsync(employee.Id);
        return MapToDto(created!);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee ID {id} not found.");

        _ = await _deptRepo.GetByIdAsync(dto.DepartmentId)
            ?? throw new InvalidOperationException(
                $"Department ID {dto.DepartmentId} not found.");

        if (dto.ManagerId.HasValue)
        {
            if (dto.ManagerId.Value == id)
                throw new InvalidOperationException(
                    "An employee cannot be their own manager.");

            var mgr = await _employeeRepo.GetByIdAsync(dto.ManagerId.Value);
            if (mgr == null)
                throw new InvalidOperationException(
                    $"Manager with Employee ID {dto.ManagerId} not found.");
        }

        employee.FullName = dto.FullName;
        employee.Designation = dto.Designation;
        employee.EmploymentType = dto.EmploymentType;
        employee.DepartmentId = dto.DepartmentId;
        employee.ManagerId = dto.ManagerId;
        employee.Gender = dto.Gender;
        employee.DateOfJoining = dto.DateOfJoining;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepo.UpdateAsync(employee);

        var updated = await _employeeRepo.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    // ── DEACTIVATE / REACTIVATE ───────────────────────────────────────────────

    public async Task DeactivateAsync(int id)
    {
        var employee = await _employeeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee ID {id} not found.");

        if (!employee.IsActive)
            throw new InvalidOperationException("Employee is already deactivated.");

        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepo.UpdateAsync(employee);
    }

    public async Task ReactivateAsync(int id)
    {
        var employee = await _employeeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee ID {id} not found.");

        if (employee.IsActive)
            throw new InvalidOperationException("Employee is already active.");

        employee.IsActive = true;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepo.UpdateAsync(employee);
    }

    // ── READ ──────────────────────────────────────────────────────────────────

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var e = await _employeeRepo.GetByIdAsync(id);
        return e == null ? null : MapToDto(e);
    }

    public async Task<EmployeeDto?> GetByUserIdAsync(int userId)
    {
        var e = await _employeeRepo.GetByUserIdAsync(userId);
        return e == null ? null : MapToDto(e);
    }

    public async Task<(IEnumerable<EmployeeSummaryDto> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize,
        string? search = null,
        int? departmentId = null,
        bool includeInactive = false)
    {
        var (items, total) = await _employeeRepo.GetPagedAsync(
            page, pageSize, search, departmentId, includeInactive);

        return (items.Select(MapToSummaryDto), total);
    }

    public async Task<IEnumerable<EmployeeSummaryDto>> GetMyTeamAsync(int managerId)
    {
        var reports = await _employeeRepo.GetDirectReportsAsync(managerId);
        return reports.Select(MapToSummaryDto);
    }

    public async Task<EmployeeHierarchyDto?> GetHierarchyAsync(int employeeId)
    {
        var employee = await _employeeRepo.GetWithManagerChainAsync(employeeId);
        return employee == null ? null : BuildHierarchyDto(employee);
    }

    // ── EMPLOYEE SELF-SERVICE ─────────────────────────────────────────────────

    public async Task UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto)
    {
        var employee = await _employeeRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Employee profile not found.");

        if (dto.PhoneNumber != null) employee.PhoneNumber = dto.PhoneNumber;
        if (dto.Address != null) employee.Address = dto.Address;
        if (dto.ProfilePhotoUrl != null) employee.ProfilePhotoUrl = dto.ProfilePhotoUrl;

        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepo.UpdateAsync(employee);
    }

    public async Task<string?> GetGenderByUserIdAsync(int userId)
    {
        var employee = await _employeeRepo.GetByUserIdAsync(userId);
        return employee?.Gender;
    }

    // ── INTERNAL / CROSS-SERVICE ──────────────────────────────────────────────

    public async Task CreateFromEventAsync(int userId, string fullName, string email, string? gender = null)
    {
        if (await _employeeRepo.UserIdExistsAsync(userId)) return;
        if (await _employeeRepo.EmailExistsAsync(email)) return;  // ← ADD THIS

        var code = await _employeeRepo.GenerateEmployeeCodeAsync();

        var employee = new Employee
        {
            UserId = userId,
            FullName = fullName,
            Email = email.ToLower(),
            EmployeeCode = code,
            Designation = "Pending Assignment",
            EmploymentType = "Full-time",
            DepartmentId = 1,
            DateOfJoining = DateTime.UtcNow,
            Gender = gender,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _employeeRepo.AddAsync(employee);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId)
        => await _employeeRepo.UserIdExistsAsync(userId);

    public async Task<int?> GetManagerUserIdAsync(int employeeUserId)
    {
        var employee = await _employeeRepo.GetByUserIdAsync(employeeUserId);
        if (employee?.ManagerId == null) return null;

        var manager = await _employeeRepo.GetByIdAsync(employee.ManagerId.Value);
        return manager?.UserId;
    }

    // ── PRIVATE MAPPING HELPERS ───────────────────────────────────────────────

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        FullName = e.FullName,
        Email = e.Email,

        EmployeeCode = e.EmployeeCode,
        Designation = e.Designation,
        EmploymentType = e.EmploymentType,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name ?? string.Empty,
        ManagerId = e.ManagerId,
        ManagerName = e.Manager?.FullName,
        DateOfJoining = e.DateOfJoining,
        IsActive = e.IsActive,
        Gender = e.Gender,
        ProfilePhotoUrl = e.ProfilePhotoUrl,
        PhoneNumber = e.PhoneNumber,
        Address = e.Address,
        CreatedAt = e.CreatedAt
    };

    private static EmployeeSummaryDto MapToSummaryDto(Employee e) => new()
    {
        Id = e.Id,
        FullName = e.FullName,
        Email = e.Email,
        EmployeeCode = e.EmployeeCode,
        Designation = e.Designation,
        DepartmentName = e.Department?.Name ?? string.Empty,
        IsActive = e.IsActive
    };

    private static EmployeeHierarchyDto BuildHierarchyDto(Employee e)
    {
        return new EmployeeHierarchyDto
        {
            Id = e.Id,
            FullName = e.FullName,
            Designation = e.Designation,
            DepartmentName = e.Department?.Name ?? string.Empty,

            // ONLY ONE LEVEL MANAGER (no recursion)
            Manager = e.Manager == null ? null : new EmployeeHierarchyDto
            {
                Id = e.Manager.Id,
                FullName = e.Manager.FullName,
                Designation = e.Manager.Designation
            },

            // ONLY ONE LEVEL DIRECT REPORTS (no recursion)
            DirectReports = e.DirectReports.Select(dr => new EmployeeHierarchyDto
            {
                Id = dr.Id,
                FullName = dr.FullName,
                Designation = dr.Designation
            }).ToList()
        };
    }
}