using LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LeaveService.Infrastructure.Data;

public class LeaveDbContext : DbContext
{
    public LeaveDbContext(DbContextOptions<LeaveDbContext> options)
        : base(options) { }

    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<Holiday> Holidays { get; set; }

    public DbSet<LeaveApprovalSagaState> LeaveApprovalSagas { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<LeaveType>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Code).IsRequired().HasMaxLength(10);
        });

        mb.Entity<LeaveRequest>(e =>
        {
            e.Property(x => x.Days).HasColumnType("decimal(4,1)");
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.Property(x => x.Reason).HasMaxLength(1000);
            e.Property(x => x.ManagerComment).HasMaxLength(500);
            e.Property(x => x.HrComment).HasMaxLength(500);
            e.HasOne(x => x.LeaveType).WithMany(x => x.LeaveRequests)
             .HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.UserId, x.Status });
        });

        mb.Entity<LeaveBalance>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.LeaveTypeId, x.Year }).IsUnique();
            e.Property(x => x.Entitled).HasColumnType("decimal(6,2)");
            e.Property(x => x.Used).HasColumnType("decimal(6,2)");
            e.Property(x => x.Pending).HasColumnType("decimal(6,2)");
            e.Property(x => x.Carried).HasColumnType("decimal(6,2)");
            e.Ignore(x => x.Available); // computed — not stored
            e.HasOne(x => x.LeaveType).WithMany(x => x.LeaveBalances)
             .HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Holiday>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Applicability).HasMaxLength(20);
        });

        // Seed default leave types
        mb.Entity<LeaveType>().HasData(
            new LeaveType { Id = 1, Name = "Annual Leave", Code = "AL", MaxDaysPerYear = 21, IsPaid = true, AccrualFrequency = "Monthly", CarryForwardMax = 5, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new LeaveType { Id = 2, Name = "Sick Leave", Code = "SL", MaxDaysPerYear = 10, IsPaid = true, AccrualFrequency = "Annually", CarryForwardMax = 0, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new LeaveType { Id = 3, Name = "Maternity Leave", Code = "ML", MaxDaysPerYear = 90, IsPaid = true, AccrualFrequency = "None", CarryForwardMax = 0, GenderApplicability = "Female", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new LeaveType { Id = 4, Name = "Compensatory Off", Code = "CO", MaxDaysPerYear = 12, IsPaid = true, AccrualFrequency = "None", CarryForwardMax = 0, IsAutoApprove = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new LeaveType { Id = 5, Name = "Unpaid Leave", Code = "UPL", MaxDaysPerYear = 30, IsPaid = false, AccrualFrequency = "None", CarryForwardMax = 0, IsActive = true, CreatedAt = new DateTime(2026, 1, 1) }
        );

        mb.Entity<LeaveApprovalSagaState>(e =>
        {
            e.HasKey(x => x.CorrelationId);
            e.Property(x => x.CurrentState).HasMaxLength(64);
            e.Property(x => x.LeaveTypeName).HasMaxLength(100);
            e.ToTable("LeaveApprovalSagas");
        });
    }
}