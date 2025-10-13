using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ITaxCalculationRepository : IBaseRepository<TaxCalculation>
{
    Task<IEnumerable<TaxCalculation>> GetByInvoiceIdAsync(Guid invoiceId);
    Task<IEnumerable<TaxCalculation>> GetByTaxTypeAsync(TaxType taxType);
    Task<IEnumerable<TaxCalculation>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<TaxCalculation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalTaxAmountAsync(Guid companyId, TaxType? taxType = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalTaxBaseAsync(Guid companyId, TaxType? taxType = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<TaxCalculation>> GetTaxSummaryByTypeAsync(Guid companyId, DateTime startDate, DateTime endDate);
}