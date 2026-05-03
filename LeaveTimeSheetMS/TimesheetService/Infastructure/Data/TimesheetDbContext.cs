using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TimesheetService.Domain.Entities;

namespace TimesheetService.Infrastructure.Data;

public class TimesheetDbContext : DbContext
{
    public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options)
        : base(options) { }

    public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── PROJECT ──────────────────────────────────────────────────────────
        mb.Entity<Project>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.ClientName).HasMaxLength(150);
        });

        // ── TIMESHEET ENTRY ──────────────────────────────────────────────────
        mb.Entity<TimesheetEntry>(e =>
        {
            e.Property(x => x.Hours).HasColumnType("decimal(4,1)");
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.Property(x => x.Category).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.ApproverComment).HasMaxLength(500);

            // Index for fast weekly lookups
            e.HasIndex(x => new { x.UserId, x.WeekStart });
            e.HasIndex(x => new { x.UserId, x.Date });

            e.HasOne(x => x.Project)
             .WithMany(x => x.Entries)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SEED — Default Projects ──────────────────────────────────────────
        mb.Entity<Project>().HasData(
            new Project { Id = 1, Name = "General", Code = "GEN", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new Project { Id = 2, Name = "Internal Work", Code = "INT", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new Project { Id = 3, Name = "Training", Code = "TRN", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
        );
    }
}