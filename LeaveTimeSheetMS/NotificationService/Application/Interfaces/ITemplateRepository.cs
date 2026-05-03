using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface ITemplateRepository
{
    Task<NotificationTemplate?> GetByTypeAsync(string type);
    Task<IEnumerable<NotificationTemplate>> GetAllAsync();
    Task AddAsync(NotificationTemplate template);
    Task UpdateAsync(NotificationTemplate template);
}