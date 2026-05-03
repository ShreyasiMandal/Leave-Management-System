using NotificationService.Application.DTOs;
using NotificationService.Application.DTOs.NotificationDTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ITemplateRepository _templateRepo;

    public NotificationService(
        INotificationRepository repo,
        ITemplateRepository templateRepo)
    {
        _repo = repo;
        _templateRepo = templateRepo;
    }

    // FR-NOTIF-001: Create in-app notification
    public async Task CreateAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            EntityId = dto.EntityId,
            EntityType = dto.EntityType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(notification);
    }

    // FR-NOTIF-005: Get notifications
    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId)
    {
        var list = await _repo.GetByUserIdAsync(userId);
        return list.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            EntityId = n.EntityId,
            EntityType = n.EntityType,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _repo.GetUnreadCountAsync(userId);

    public async Task MarkAsReadAsync(int notificationId, int userId)
        => await _repo.MarkAsReadAsync(notificationId, userId);

    // FR-NOTIF-005: Bulk mark all as read
    public async Task MarkAllAsReadAsync(int userId)
        => await _repo.MarkAllAsReadAsync(userId);

    // FR-NOTIF-003: Template management
    public async Task<IEnumerable<TemplateDto>> GetTemplatesAsync()
    {
        var templates = await _templateRepo.GetAllAsync();
        return templates.Select(t => new TemplateDto
        {
            Id = t.Id,
            Type = t.Type,
            Subject = t.Subject,
            Body = t.Body,
            IsActive = t.IsActive
        });
    }

    public async Task<TemplateDto> UpdateTemplateAsync(int id, UpdateTemplateDto dto)
    {
        var template = await _templateRepo.GetByTypeAsync(string.Empty);
        var all = await _templateRepo.GetAllAsync();
        var found = all.FirstOrDefault(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Template {id} not found.");

        found.Subject = dto.Subject;
        found.Body = dto.Body;
        found.UpdatedAt = DateTime.UtcNow;
        await _templateRepo.UpdateAsync(found);

        return new TemplateDto
        {
            Id = found.Id,
            Type = found.Type,
            Subject = found.Subject,
            Body = found.Body,
            IsActive = found.IsActive
        };
    }

    // FR-NOTIF-003: Replace placeholders in template
    public async Task<(string Subject, string Body)> ResolveTemplateAsync(
        string type, Dictionary<string, string> placeholders)
    {
        var template = await _templateRepo.GetByTypeAsync(type);
        if (template == null)
            return (type, string.Join(", ",
                placeholders.Select(p => $"{p.Key}: {p.Value}")));

        var subject = template.Subject;
        var body = template.Body;

        foreach (var p in placeholders)
        {
            subject = subject.Replace($"{{{p.Key}}}", p.Value);
            body = body.Replace($"{{{p.Key}}}", p.Value);
        }

        return (subject, body);
    }
}