using Microsoft.EntityFrameworkCore;
using ReportService.Domain.Models;

namespace ReportService.Infrastructure.Data;

/// <summary>
/// Report Service uses read-only views/queries against LeaveDB and TimesheetDB.
/// No entities owned by this service — purely for reporting queries.
/// Uses raw SQL / Dapper-style for cross-DB aggregation.
/// </summary>
public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options)
        : base(options) { }

    // No DbSets — this context is used for raw SQL reporting queries only
}