using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Body", "CreatedAt", "IsActive", "Subject", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Hi,<br><br>{EmployeeName} has submitted a {LeaveType} request from {StartDate} to {EndDate} ({Days} day(s)).<br>Please review and take action.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Leave Request Submitted — {EmployeeName}", "LeaveCreated", null },
                    { 2, "Hi {EmployeeName},<br><br>Your {LeaveType} request from {StartDate} to {EndDate} has been <b>Approved</b>.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Your Leave Request has been Approved", "LeaveApproved", null },
                    { 3, "Hi {EmployeeName},<br><br>Your {LeaveType} request has been <b>Rejected</b>.<br>Reason: {Comment}", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Your Leave Request has been Rejected", "LeaveRejected", null },
                    { 4, "Hi,<br><br>{EmployeeName} has submitted their timesheet for week starting {WeekStart} ({TotalHours} hours).<br>Please review and approve.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Timesheet Submitted for Approval — {EmployeeName}", "TimesheetSubmitted", null },
                    { 5, "Hi {EmployeeName},<br><br>Your timesheet for week starting {WeekStart} has been <b>Approved</b>.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Your Timesheet has been Approved", "TimesheetApproved", null },
                    { 6, "Hi {EmployeeName},<br><br>Your timesheet for week starting {WeekStart} has been <b>Rejected</b>.<br>Reason: {Comment}", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Your Timesheet has been Rejected", "TimesheetRejected", null },
                    { 7, "Hi {EmployeeName},<br><br>Your timesheet for week starting {WeekStart} has not been submitted yet. Please submit as soon as possible.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Overdue Timesheet Reminder", "TimesheetOverdue", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Type",
                table: "Templates",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Templates");
        }
    }
}
