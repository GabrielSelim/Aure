using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<IEnumerable<Notification>> GetByContractIdAsync(Guid contractId);
    Task<IEnumerable<Notification>> GetByPaymentIdAsync(Guid paymentId);
    Task<IEnumerable<Notification>> GetByRecipientEmailAsync(string recipientEmail);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync();
    Task AddAsync(Notification entity);
    Task UpdateAsync(Notification entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}