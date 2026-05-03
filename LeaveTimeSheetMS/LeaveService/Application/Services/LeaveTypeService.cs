using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveTypeDtos;
using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;

namespace LeaveService.Application.Services;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ILeaveTypeRepository _repo;

    public LeaveTypeService(ILeaveTypeRepository repo) => _repo = repo;

    public async Task<IEnumerable<LeaveTypeDto>> GetAllAsync(bool includeInactive = false)
        => (await _repo.GetAllAsync(includeInactive)).Select(Map);

    public async Task<LeaveTypeDto?> GetByIdAsync(int id)
    {
        var lt = await _repo.GetByIdAsync(id);
        return lt == null ? null : Map(lt);
    }

    public async Task<LeaveTypeDto> CreateAsync(CreateLeaveTypeDto dto)
    {
        if (await _repo.CodeExistsAsync(dto.Code))
            throw new InvalidOperationException($"Code '{dto.Code}' already exists.");

        var lt = new LeaveType
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            MaxDaysPerYear = dto.MaxDaysPerYear,
            IsPaid = dto.IsPaid,
            AccrualFrequency = dto.AccrualFrequency,
            CarryForwardMax = dto.CarryForwardMax,
            GenderApplicability = dto.GenderApplicability,
            IsDocumentRequired = dto.IsDocumentRequired,
            IsAutoApprove = dto.IsAutoApprove,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(lt);
        return Map(lt);
    }

    public async Task<LeaveTypeDto> UpdateAsync(int id, UpdateLeaveTypeDto dto)
    {
        var lt = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave type {id} not found.");

        lt.Name = dto.Name;
        lt.MaxDaysPerYear = dto.MaxDaysPerYear;
        lt.IsPaid = dto.IsPaid;
        lt.AccrualFrequency = dto.AccrualFrequency;
        lt.CarryForwardMax = dto.CarryForwardMax;
        lt.GenderApplicability = dto.GenderApplicability;
        lt.IsDocumentRequired = dto.IsDocumentRequired;
        lt.IsAutoApprove = dto.IsAutoApprove;
        lt.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(lt);
        return Map(lt);
    }

    public async Task ActivateAsync(int id)
    {
        var lt = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave type {id} not found.");
        lt.IsActive = true; lt.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(lt);
    }

    public async Task DeactivateAsync(int id)
    {
        var lt = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Leave type {id} not found.");
        lt.IsActive = false; lt.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(lt);
    }

    private static LeaveTypeDto Map(LeaveType lt) => new()
    {
        Id = lt.Id,
        Name = lt.Name,
        Code = lt.Code,
        MaxDaysPerYear = lt.MaxDaysPerYear,
        IsPaid = lt.IsPaid,
        AccrualFrequency = lt.AccrualFrequency,
        CarryForwardMax = lt.CarryForwardMax,
        GenderApplicability = lt.GenderApplicability,
        IsDocumentRequired = lt.IsDocumentRequired,
        IsAutoApprove = lt.IsAutoApprove,
        IsActive = lt.IsActive
    };
}