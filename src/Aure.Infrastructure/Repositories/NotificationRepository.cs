using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByContractIdAsync(Guid contractId)
    {
        return await _context.Notifications
            .Where(n => !n.IsDeleted && n.ContractId == contractId)
            .Include(n => n.Contract)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByPaymentIdAsync(Guid paymentId)
    {
        return await _context.Notifications
            .Where(n => !n.IsDeleted && n.PaymentId == paymentId)
            .Include(n => n.Payment)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByRecipientEmailAsync(string recipientEmail)
    {
        return await _context.Notifications
            .Where(n => !n.IsDeleted && n.RecipientEmail == recipientEmail)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync()
    {
        return await _context.Notifications
            .Where(n => !n.IsDeleted && n.Status == Domain.Enums.NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }
}