using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface ILedgerEntryRepository : IBaseRepository<LedgerEntry>
{
    Task<IEnumerable<LedgerEntry>> GetByPaymentIdAsync(Guid paymentId);
    Task<IEnumerable<LedgerEntry>> GetByContractIdAsync(Guid contractId);
    Task<IEnumerable<LedgerEntry>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<LedgerEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalDebitAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalCreditAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetBalanceAsync(Guid companyId, DateTime? asOfDate = null);
}