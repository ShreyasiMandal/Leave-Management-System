using NotificationService.Application.DTOs;
using NotificationService.Application.DTOs.NotificationDTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    // FR-NOTIF-001: Create in-app notification
    Task CreateAsync(CreateNotificationDto dto);

    // FR-NOTIF-005: Read/unread management
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);

    // FR-NOTIF-003: Template management (HR Admin)
    Task<IEnumerable<TemplateDto>> GetTemplatesAsync();
    Task<TemplateDto> UpdateTemplateAsync(int id, UpdateTemplateDto dto);

    // Internal: resolve template with placeholders
    Task<(string Subject, string Body)> ResolveTemplateAsync(
        string type, Dictionary<string, string> placeholders);
}