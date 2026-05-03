using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetService.Application.DTOs;
using TimesheetService.Application.DTOs.ProjectDTOs;
using TimesheetService.Application.Interfaces;

namespace TimesheetService.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _service;
    public ProjectController(IProjectService service) => _service = service;

    // All roles — for dropdown in timesheet entry form
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var projects = await _service.GetAllAsync(includeInactive);
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _service.GetByIdAsync(id);
        if (p == null)
            return NotFound(new { message = $"Project {id} not found." });
        return Ok(p);
    }

    // HR/Manager creates projects
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(201, result);
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(new { message = "Project deactivated." });
    }
}