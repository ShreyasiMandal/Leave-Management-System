namespace TimesheetService.Domain.Enums;

public static class TimesheetStatus
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Locked = "Locked";   // FR-TS-008
}

public static class TimesheetCategory
{
    public const string Regular = "Regular";
    public const string Overtime = "Overtime";
    public const string OnCall = "On-Call";
}

public static class UserRoles
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string HRAdmin = "HRAdmin";
    public const string SystemAdmin = "SystemAdmin";
}