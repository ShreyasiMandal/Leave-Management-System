namespace NotificationService.Application.DTOs.NotificationDTOs
{
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? EntityType { get; set; }
    }
}
