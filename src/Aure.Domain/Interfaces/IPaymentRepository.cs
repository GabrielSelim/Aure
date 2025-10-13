using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetByContractIdAsync(Guid contractId);
    Task<IEnumerable<Payment>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
    Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<decimal> GetTotalAmountByContractAsync(Guid contractId);
}