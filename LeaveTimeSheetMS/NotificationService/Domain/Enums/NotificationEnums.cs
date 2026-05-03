namespace NotificationService.Domain.Enums;

public static class NotificationType
{
    public const string LeaveCreated = "LeaveCreated";
    public const string LeaveApproved = "LeaveApproved";
    public const string LeaveRejected = "LeaveRejected";
    public const string LeaveCancelled = "LeaveCancelled";
    public const string LeaveEscalated = "LeaveEscalatedToHR";
    public const string TimesheetSubmitted = "TimesheetSubmitted";
    public const string TimesheetApproved = "TimesheetApproved";
    public const string TimesheetRejected = "TimesheetRejected";
    public const string TimesheetOverdue = "TimesheetOverdue";
    public const string BalanceAlert = "BalanceAlert";
}

public static class UserRoles
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string HRAdmin = "HRAdmin";
    public const string SystemAdmin = "SystemAdmin";
}