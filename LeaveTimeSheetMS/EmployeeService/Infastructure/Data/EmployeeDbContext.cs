using EmployeeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastructure.Data;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.EmployeeCode).IsUnique();

            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(20);
            e.Property(x => x.Designation).HasMaxLength(100);
            e.Property(x => x.EmploymentType).HasMaxLength(50);
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.ProfilePhotoUrl).HasMaxLength(500);

            // Self-referential: Employee → Manager
            e.HasOne(x => x.Manager)
             .WithMany(x => x.DirectReports)
             .HasForeignKey(x => x.ManagerId)
             .OnDelete(DeleteBehavior.Restrict);

            // Employee → Department
            e.HasOne(x => x.Department)
             .WithMany(x => x.Employees)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Department>(d =>
        {
            d.HasIndex(x => x.Name).IsUnique();
            d.Property(x => x.Name).IsRequired().HasMaxLength(100);
            d.Property(x => x.Description).HasMaxLength(500);
        });

        // Seed: default "Unassigned" department
        mb.Entity<Department>().HasData(new Department
        {
            Id = 1,
            Name = "Unassigned",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1)
        });
    }
}