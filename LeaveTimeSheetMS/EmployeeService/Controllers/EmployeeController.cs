using EmployeeService.Application.DTOs;
using EmployeeService.Application.DTOs.EmployeeRequestDTOs;
using EmployeeService.Application.Interfaces;
using EmployeeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service)
    {
        _service = service;
    }

    private int GetCurrentUserId() =>
        int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : 0;

    private string GetCurrentRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // ── HR ADMIN ─────────────────────────────────────────────────────────────

    [Authorize(Policy = "HROrAbove")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(201, result);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(new { message = "Employee deactivated. Login access revoked." });
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(int id)
    {
        await _service.ReactivateAsync(id);
        return Ok(new { message = "Employee reactivated successfully." });
    }

    // ── READ — ALL ROLES ─────────────────────────────────────────────────────

    [Authorize(Policy = "HROrAbove")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] bool includeInactive = false)
    {
        var (items, total) = await _service.GetAllAsync(
            page, pageSize, search, departmentId, includeInactive);

        return Ok(new
        {
            Data = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _service.GetByIdAsync(id);
        if (employee == null)
            return NotFound(new { message = $"Employee ID {id} not found." });

        // FR-RBAC-002: Employee can only view their own record
        if (GetCurrentRole() == UserRoles.Employee
            && employee.UserId != GetCurrentUserId())
            return Forbid();

        return Ok(employee);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var employee = await _service.GetByUserIdAsync(GetCurrentUserId());
        if (employee == null)
            return NotFound(new { message = "Employee profile not found." });
        return Ok(employee);
    }

    [HttpGet("{id}/hierarchy")]
    public async Task<IActionResult> GetHierarchy(int id)
    {
        var hierarchy = await _service.GetHierarchyAsync(id);
        if (hierarchy == null)
            return NotFound(new { message = $"Employee ID {id} not found." });
        return Ok(hierarchy);
    }

    [HttpGet("me/hierarchy")]
    public async Task<IActionResult> GetMyHierarchy()
    {
        var employee = await _service.GetByUserIdAsync(GetCurrentUserId());
        if (employee == null)
            return NotFound(new { message = "Profile not found." });

        var hierarchy = await _service.GetHierarchyAsync(employee.Id);
        return Ok(hierarchy);
    }

    // ── MANAGER ───────────────────────────────────────────────────────────────

    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("my-team")]
    public async Task<IActionResult> GetMyTeam()
    {
        var manager = await _service.GetByUserIdAsync(GetCurrentUserId());
        if (manager == null)
            return NotFound(new { message = "Manager profile not found." });

        var team = await _service.GetMyTeamAsync(manager.Id);
        return Ok(team);
    }

    // ── EMPLOYEE SELF-SERVICE ─────────────────────────────────────────────────

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto dto)
    {
        await _service.UpdateMyProfileAsync(GetCurrentUserId(), dto);
        return Ok(new { message = "Profile updated successfully." });
    }

    // ── INTERNAL (cross-service) ──────────────────────────────────────────────

    [HttpGet("internal/exists/{userId}")]
    public async Task<IActionResult> ExistsByUserId(int userId)
    {
        var exists = await _service.ExistsByUserIdAsync(userId);
        return Ok(new { exists });
    }

    [HttpGet("internal/manager-userid/{employeeUserId}")]
    public async Task<IActionResult> GetManagerUserId(int employeeUserId)
    {
        var managerUserId = await _service.GetManagerUserIdAsync(employeeUserId);
        return Ok(new { managerUserId });
    }

    [HttpGet("internal/gender/{userId}")]
    public async Task<IActionResult> GetGenderByUserId(int userId)
    {
        var gender = await _service.GetGenderByUserIdAsync(userId);
        return Ok(new { gender });
    }
}