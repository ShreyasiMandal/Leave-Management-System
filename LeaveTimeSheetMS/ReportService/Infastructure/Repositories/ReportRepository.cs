using Dapper;
using Microsoft.Data.SqlClient;
using ReportService.Application.Interfaces;
using ReportService.Domain.Models;

namespace ReportService.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IConfiguration _config;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(
        IConfiguration config,
        ILogger<ReportRepository> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── FR-REP-001: Leave Report ──────────────────────────────────────────────
    public async Task<IEnumerable<LeaveReportModel>> GetLeaveReportAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? userId,
        int? departmentId,
        string? leaveType,
        string? status)
    {
        try
        {
            var connStr = _config.GetConnectionString("LeaveDB");
            using var conn = new SqlConnection(connStr);

            var sql = @"
                SELECT
                    lr.Id,
                    lr.UserId,
                    CAST('Employee ' + CAST(lr.UserId AS VARCHAR(10))
                        AS NVARCHAR(150))        AS EmployeeName,
                    CAST('' AS NVARCHAR(100))    AS Department,
                    lt.Name                      AS LeaveType,
                    lr.StartDate,
                    lr.EndDate,
                    lr.Days,
                    lr.Status,
                    lr.CreatedAt                 AS AppliedOn
                FROM LeaveRequests lr
                INNER JOIN LeaveTypes lt ON lt.Id = lr.LeaveTypeId
                WHERE 1 = 1
                    AND (@StartDate  IS NULL OR lr.StartDate  >= @StartDate)
                    AND (@EndDate    IS NULL OR lr.EndDate    <= @EndDate)
                    AND (@UserId     IS NULL OR lr.UserId     = @UserId)
                    AND (@LeaveType  IS NULL OR lt.Name       = @LeaveType)
                    AND (@Status     IS NULL OR lr.Status     = @Status)
                ORDER BY lr.CreatedAt DESC";

            var result = await conn.QueryAsync<LeaveReportModel>(sql, new
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                LeaveType = leaveType,
                Status = status
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching leave report from LeaveDB");
            throw new InvalidOperationException(
                $"Failed to fetch leave report: {ex.Message}");
        }
    }

    // ── FR-REP-002: Timesheet Report ──────────────────────────────────────────
    public async Task<IEnumerable<TimesheetReportModel>> GetTimesheetReportAsync(
        DateTime? startDate,
        DateTime? endDate,
        int? userId,
        int? projectId)
    {
        try
        {
            var connStr = _config.GetConnectionString("TimesheetDB");
            using var conn = new SqlConnection(connStr);

            var sql = @"
                SELECT
                    te.UserId,
                    CAST('Employee ' + CAST(te.UserId AS VARCHAR(10))
                        AS NVARCHAR(150))        AS EmployeeName,
                    p.Name                       AS ProjectName,
                    te.WeekStart,
                    SUM(te.Hours)                AS TotalHours,
                    te.Category,
                    te.Status
                FROM TimesheetEntries te
                INNER JOIN Projects p ON p.Id = te.ProjectId
                WHERE 1 = 1
                    AND (@StartDate IS NULL OR te.WeekStart >= @StartDate)
                    AND (@EndDate   IS NULL OR te.WeekStart <= @EndDate)
                    AND (@UserId    IS NULL OR te.UserId    = @UserId)
                    AND (@ProjectId IS NULL OR te.ProjectId = @ProjectId)
                GROUP BY
                    te.UserId,
                    p.Name,
                    te.WeekStart,
                    te.Category,
                    te.Status
                ORDER BY te.WeekStart DESC";

            var result = await conn.QueryAsync<TimesheetReportModel>(sql, new
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId,
                ProjectId = projectId
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching timesheet report from TimesheetDB");
            throw new InvalidOperationException(
                $"Failed to fetch timesheet report: {ex.Message}");
        }
    }

    // ── FR-REP-003: Attendance Summary ────────────────────────────────────────
    public async Task<AttendanceSummaryModel> GetAttendanceSummaryAsync(
        DateTime date)
    {
        try
        {
            var connStr = _config.GetConnectionString("LeaveDB");
            using var conn = new SqlConnection(connStr);

            var sql = @"
                SELECT
                    @Date AS Date,
                    COUNT(CASE
                        WHEN lr.Status = 'Approved'
                             AND lr.StartDate <= @Date
                             AND lr.EndDate   >= @Date
                        THEN 1
                    END) AS OnLeaveCount,
                    0    AS PresentCount,
                    0    AS AbsentCount,
                    0    AS TotalCount
                FROM LeaveRequests lr
                WHERE lr.StartDate <= @Date
                  AND lr.EndDate   >= @Date";

            var result = await conn.QueryFirstOrDefaultAsync<AttendanceSummaryModel>(
                sql, new { Date = date.Date });

            return result ?? new AttendanceSummaryModel
            {
                Date = date.Date,
                OnLeaveCount = 0,
                PresentCount = 0,
                AbsentCount = 0,
                TotalCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attendance summary");
            throw new InvalidOperationException(
                $"Failed to fetch attendance: {ex.Message}");
        }
    }
}