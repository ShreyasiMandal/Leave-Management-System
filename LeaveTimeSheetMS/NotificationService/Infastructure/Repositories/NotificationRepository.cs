using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _ctx;
    public NotificationRepository(NotificationDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        => await _ctx.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _ctx.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task AddAsync(Notification notification)
    {
        await _ctx.Notifications.AddAsync(notification);
        await _ctx.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var n = await _ctx.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n == null) return;
        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    // FR-NOTIF-005: Bulk mark all as read
    public async Task MarkAllAsReadAsync(int userId)
    {
        var unread = await _ctx.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _ctx.SaveChangesAsync();
    }
}