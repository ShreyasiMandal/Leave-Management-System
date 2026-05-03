namespace NotificationService.Application.DTOs.NotificationDTOs
{
    public class TemplateDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
