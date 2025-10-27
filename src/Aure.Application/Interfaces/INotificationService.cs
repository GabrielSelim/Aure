namespace Aure.Application.Interfaces;

public interface INotificationService
{
    Task SendPaymentNotificationToPJAsync(Guid paymentId);
    Task SendPaymentProcessedToManagersAsync(Guid paymentId);
    
    Task SendContractCreatedToPJAsync(Guid contractId);
    Task SendContractSignedToManagersAsync(Guid contractId);
    Task SendContractExpirationAlertsAsync();
    
    Task SendNewEmployeeNotificationAsync(Guid userId);
    
    Task<bool> ProcessPendingNotificationsAsync();
    Task ScheduleRecurringNotificationsAsync();
}