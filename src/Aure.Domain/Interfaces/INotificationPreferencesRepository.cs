using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByUserIdAsync(Guid userId);
    Task AddAsync(NotificationPreferences preferences);
    Task UpdateAsync(NotificationPreferences preferences);
}
