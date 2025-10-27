using Aure.Domain.Entities;

namespace Aure.Application.Interfaces;

public interface INotificationTemplateService
{
    Task<string> GeneratePaymentReceivedTemplateAsync(Payment payment, User recipient);
    Task<string> GenerateContractCreatedTemplateAsync(Contract contract, User recipient);
    Task<string> GenerateContractSignedTemplateAsync(Contract contract, User recipient);
    Task<string> GenerateContractExpirationTemplateAsync(Contract contract, User recipient, int daysUntilExpiration);
    Task<string> GenerateNewEmployeeTemplateAsync(User newEmployee, User recipient);
    Task<string> GeneratePaymentProcessedTemplateAsync(Payment payment, User recipient);
}