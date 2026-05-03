namespace LeaveService.Domain.Enums;

public static class LeaveStatus
{
    public const string Draft = "Draft";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Withdrawn = "Withdrawn";
}

public static class HalfDaySession
{
    public const string Full = "Full";
    public const string Morning = "Morning";
    public const string Afternoon = "Afternoon";
}

public static class AccrualFrequency
{
    public const string Monthly = "Monthly";
    public const string Annually = "Annually";
    public const string None = "None";
}

public static class UserRoles
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string HRAdmin = "HRAdmin";
    public const string SystemAdmin = "SystemAdmin";
}