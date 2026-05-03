using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesheetService.Application.DTOs;
using TimesheetService.Application.DTOs.TimesheetDTOs;
using TimesheetService.Application.Interfaces;
using TimesheetService.Domain.Enums;

namespace TimesheetService.Controllers;

[ApiController]
[Route("api/timesheets")]
[Authorize]
public class TimesheetController : ControllerBase
{
    private readonly ITimesheetService _service;

    public TimesheetController(ITimesheetService service)
        => _service = service;

    private int GetUserId() =>
        int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : 0;
    private string GetRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // ── EMPLOYEE ENDPOINTS ────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/timesheets
    /// FR-TS-001: Log daily timesheet entry.
    /// FR-TS-003: No future dates (enforced in service).
    /// FR-TS-005: Returns warnings if hours > 12 or below threshold.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> LogEntry(
        [FromBody] CreateTimesheetEntryDto dto)
    {
        var result = await _service.LogEntryAsync(GetUserId(), dto);
        return StatusCode(201, result);
    }

    /// <summary>
    /// PUT /api/timesheets/{id}
    /// Update a draft entry.
    /// FR-TS-008: Cannot edit Approved/Locked entries.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntry(int id,
        [FromBody] CreateTimesheetEntryDto dto)
    {
        var result = await _service.UpdateEntryAsync(GetUserId(), id, dto);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/timesheets/week?date=2026-03-31
    /// FR-TS-002: Weekly view with totals and warnings.
    /// FR-TA-004: Flags overdue weeks.
    /// </summary>
    [HttpGet("week")]
    public async Task<IActionResult> GetWeekly(
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var weekly = await _service.GetWeeklyAsync(GetUserId(), targetDate);
        return Ok(weekly);
    }

    /// <summary>
    /// GET /api/timesheets/history
    /// Employee views all past timesheet entries.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _service.GetMyHistoryAsync(GetUserId());
        return Ok(history);
    }

    /// <summary>
    /// POST /api/timesheets/submit?weekStart=2026-03-24
    /// FR-TS-006: Submit entire week for approval.
    /// FR-TA-004: Flags late submission.
    /// Publishes TimesheetSubmitted event → RabbitMQ → Notification Service.
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitWeek(
        [FromQuery] DateTime? weekStart = null)
    {
        var ws = weekStart ?? DateTime.UtcNow;
        var result = await _service.SubmitWeekAsync(GetUserId(), ws);
        return Ok(result);
    }

    // ── MANAGER ENDPOINTS ─────────────────────────────────────────────────────

    /// <summary>
    /// GET /api/timesheets/pending
    /// FR-TA-001: Manager views all submitted entries pending approval.
    /// </summary>
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var list = await _service.GetPendingApprovalAsync();
        return Ok(list);
    }

    /// <summary>
    /// GET /api/timesheets/team?weekStart=2026-03-24
    /// FR-TA-003: Consolidated team timesheet summary for the week.
    /// </summary>
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("team")]
    public async Task<IActionResult> GetTeamSummary(
        [FromQuery] DateTime? weekStart = null)
    {
        var ws = weekStart ?? DateTime.UtcNow;
        var summary = await _service.GetTeamSummaryAsync(GetUserId(), ws);
        return Ok(summary);
    }

    /// <summary>
    /// PUT /api/timesheets/{id}/approve
    /// FR-TA-002: Manager approves single entry.
    /// FR-TS-008: Entry becomes Locked after approval.
    /// Publishes TimesheetApproved event → RabbitMQ → Notification Service.
    /// </summary>
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        await _service.ApproveAsync(id, GetUserId());
        return Ok(new { message = "Timesheet entry approved and locked." });
    }

    /// <summary>
    /// PUT /api/timesheets/{id}/reject
    /// FR-TA-002: Manager rejects with mandatory comment.
    /// Entry returns to Draft for employee revision.
    /// </summary>
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id,
        [FromBody] RejectTimesheetDto dto)
    {
        await _service.RejectAsync(id, GetUserId(), dto.Comment);
        return Ok(new { message = "Timesheet entry rejected." });
    }
}