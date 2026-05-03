using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using System.Reflection.Emit;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Notification>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            e.Property(x => x.Type).IsRequired().HasMaxLength(50);
            e.HasIndex(x => new { x.UserId, x.IsRead });
        });

        mb.Entity<NotificationTemplate>(e =>
        {
            e.HasIndex(x => x.Type).IsUnique();
            e.Property(x => x.Type).IsRequired().HasMaxLength(50);
            e.Property(x => x.Subject).IsRequired().HasMaxLength(200);
            e.Property(x => x.Body).IsRequired();
        });

        // ── SEED DEFAULT TEMPLATES ───────────────────────────────────────────
        mb.Entity<NotificationTemplate>().HasData(
            new NotificationTemplate
            {
                Id = 1,
                Type = NotificationType.LeaveCreated,
                IsActive = true,
                Subject = "Leave Request Submitted — {EmployeeName}",
                Body = "Hi,<br><br>{EmployeeName} has submitted a {LeaveType} request " +
                          "from {StartDate} to {EndDate} ({Days} day(s)).<br>" +
                          "Please review and take action.",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new NotificationTemplate
            {
                Id = 2,
                Type = NotificationType.LeaveApproved,
                IsActive = true,
                Subject = "Your Leave Request has been Approved",
                Body = "Hi {EmployeeName},<br><br>Your {LeaveType} request " +
                          "from {StartDate} to {EndDate} has been <b>Approved</b>.",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new NotificationTemplate
            {
                Id = 3,
                Type = NotificationType.LeaveRejected,
                IsActive = true,
                Subject = "Your Leave Request has been Rejected",
                Body = "Hi {EmployeeName},<br><br>Your {LeaveType} request " +
                          "has been <b>Rejected</b>.<br>Reason: {Comment}",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new NotificationTemplate
            {
                Id = 4,
                Type = NotificationType.TimesheetSubmitted,
                IsActive = true,
                Subject = "Timesheet Submitted for Approval — {EmployeeName}",
                Body = "Hi,<br><br>{EmployeeName} has submitted their timesheet " +
                          "for week starting {WeekStart} ({TotalHours} hours).<br>" +
                          "Please review and approve.",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new NotificationTemplate
            {
                Id = 5,
                Type = NotificationType.TimesheetApproved,
                IsActive = true,
                Subject = "Your Timesheet has been Approved",
                Body = "Hi {EmployeeName},<br><br>Your timesheet for week " +
                          "starting {WeekStart} has been <b>Approved</b>.",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new NotificationTemplate
            {
                Id = 6,
                Type = NotificationType.TimesheetRejected,
                IsActive = true,
                Subject = "Your Timesheet has been Rejected",
                Body = "Hi {EmployeeName},<br><br>Your timesheet for week " +
                          "starting {WeekStart} has been <b>Rejected</b>.<br>" +
                          "Reason: {Comment}",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new NotificationTemplate
            {
                Id = 7,
                Type = NotificationType.TimesheetOverdue,
                IsActive = true,
                Subject = "Overdue Timesheet Reminder",
                Body = "Hi {EmployeeName},<br><br>Your timesheet for week " +
                          "starting {WeekStart} has not been submitted yet. " +
                          "Please submit as soon as possible.",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}