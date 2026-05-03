namespace TimesheetService.Domain.Entities;

/// <summary>
/// FR-TS-001: Project/Task that employees log hours against.
/// </summary>
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TimesheetEntry> Entries { get; set; }
        = new List<TimesheetEntry>();
}