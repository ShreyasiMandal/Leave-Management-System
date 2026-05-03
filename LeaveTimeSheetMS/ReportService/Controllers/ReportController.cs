using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.Application.DTOs;
using ReportService.Application.Interfaces;
using ReportService.Domain.Models;

namespace ReportService.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;

    public ReportController(
        IReportService reportService,
        IExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    // ── FR-REP-001: Leave Summary Report ─────────────────────────────────────

    // GET /api/reports/leave
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("leave")]
    public async Task<IActionResult> GetLeaveReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] string? leaveType = null,
        [FromQuery] string? status = null)
    {
        var filter = new LeaveReportFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            DepartmentId = departmentId,
            LeaveType = leaveType,
            Status = status
        };

        var data = await _reportService.GetLeaveReportAsync(filter);
        return Ok(data);
    }

    // ── FR-REP-002: Timesheet Report ──────────────────────────────────────────

    // GET /api/reports/timesheet
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("timesheet")]
    public async Task<IActionResult> GetTimesheetReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? projectId = null)
    {
        var filter = new TimesheetReportFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            ProjectId = projectId
        };

        var data = await _reportService.GetTimesheetReportAsync(filter);
        return Ok(data);
    }

    // ── FR-REP-003: Attendance Dashboard ─────────────────────────────────────

    // GET /api/reports/attendance?date=2026-04-01
    [Authorize(Policy = "HROrAbove")]
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceSummary(
        [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var data = await _reportService.GetAttendanceSummaryAsync(targetDate);
        return Ok(data);
    }

    // ── FR-REP-004: Export ────────────────────────────────────────────────────

    // GET /api/reports/export?type=leave&format=xlsx
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string type = "leave",
        [FromQuery] string format = "xlsx",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? userId = null)
    {
        byte[] fileBytes;
        string fileName;
        string contentType = format == "pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        if (type == "leave")
        {
            var filter = new LeaveReportFilterDto
            { StartDate = startDate, EndDate = endDate, UserId = userId };
            var data = await _reportService.GetLeaveReportAsync(filter);

            fileBytes = format == "pdf"
                ? _exportService.ExportToPdf(data, "Leave Report")
                : _exportService.ExportToExcel(data, "Leave Report");

            fileName = $"LeaveReport_{DateTime.UtcNow:yyyyMMdd}.{format}";
        }
        else
        {
            var filter = new TimesheetReportFilterDto
            { StartDate = startDate, EndDate = endDate, UserId = userId };
            var data = await _reportService.GetTimesheetReportAsync(filter);

            fileBytes = format == "pdf"
                ? _exportService.ExportToPdf(data, "Timesheet Report")
                : _exportService.ExportToExcel(data, "Timesheet Report");

            fileName = $"TimesheetReport_{DateTime.UtcNow:yyyyMMdd}.{format}";
        }

        return File(fileBytes, contentType, fileName);
    }
}