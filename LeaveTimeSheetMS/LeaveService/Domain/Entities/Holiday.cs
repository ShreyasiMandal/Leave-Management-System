namespace LeaveService.Domain.Entities;

// FR-HC-001 to FR-HC-004
public class Holiday
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Year { get; set; }
    public string Applicability { get; set; } = "Global"; // Global | Department
    public int? DepartmentId { get; set; } // null = Global
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}