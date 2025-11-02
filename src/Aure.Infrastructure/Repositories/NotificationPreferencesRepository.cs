using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly AureDbContext _context;

    public NotificationPreferencesRepository(AureDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferences?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Set<NotificationPreferences>()
            .FirstOrDefaultAsync(np => np.UserId == userId && !np.IsDeleted);
    }

    public async Task AddAsync(NotificationPreferences preferences)
    {
        await _context.Set<NotificationPreferences>().AddAsync(preferences);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(NotificationPreferences preferences)
    {
        preferences.UpdateTimestamp();
        _context.Set<NotificationPreferences>().Update(preferences);
        await _context.SaveChangesAsync();
    }
}
