using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.HolidayDtos;
using LeaveService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveService.Controllers;

[ApiController]
[Route("api/holidays")]
[Authorize]
public class HolidayController : ControllerBase
{
    private readonly IHolidayService _service;
    public HolidayController(IHolidayService service) => _service = service;

    // GET /api/holidays — FR-HC-004: All employees view holidays
    [HttpGet]
    public async Task<IActionResult> GetByYear([FromQuery] int year = 0)
    {
        var y = year == 0 ? DateTime.UtcNow.Year : year;
        var list = await _service.GetByYearAsync(y);
        return Ok(list);
    }

    // POST /api/holidays — FR-HC-001: HR creates holiday
    [Authorize(Policy = "HROrAbove")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHolidayDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(201, result);
    }

    // DELETE /api/holidays/{id} — HR deletes holiday
    [Authorize(Policy = "HROrAbove")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { message = "Holiday deleted." });
    }

    // POST /api/holidays/copy — FR-HC-003: Copy calendar year to year
    [Authorize(Policy = "HROrAbove")]
    [HttpPost("copy")]
    public async Task<IActionResult> CopyCalendar([FromBody] CopyCalendarDto dto)
    {
        var count = await _service.CopyCalendarAsync(dto);
        return Ok(new { message = $"{count} holiday(s) copied to {dto.ToYear}." });
    }
}