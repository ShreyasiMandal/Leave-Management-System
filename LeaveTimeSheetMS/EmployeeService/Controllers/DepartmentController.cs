using EmployeeService.Application.DTOs;
using EmployeeService.Application.DTOs.DepartmentRequestDTOs;
using EmployeeService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var depts = await _service.GetAllAsync(includeInactive);
        return Ok(depts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dept = await _service.GetByIdAsync(id);
        if (dept == null)
            return NotFound(new { message = $"Department ID {id} not found." });
        return Ok(dept);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(201, result);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(new { message = "Department deactivated." });
    }
    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(int id)
    {
        await _service.ReactivateAsync(id);
        return Ok(new { message = "Department reactivated successfully." });
    }


}