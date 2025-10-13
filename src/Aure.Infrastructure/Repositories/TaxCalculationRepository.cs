using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class TaxCalculationRepository : BaseRepository<TaxCalculation>, ITaxCalculationRepository
{
    public TaxCalculationRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<TaxCalculation>> GetByInvoiceIdAsync(Guid invoiceId)
    {
        return await _context.TaxCalculations
            .Include(tc => tc.Invoice)
            .Where(tc => tc.InvoiceId == invoiceId)
            .OrderBy(tc => tc.TaxType)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaxCalculation>> GetByTaxTypeAsync(TaxType taxType)
    {
        return await _context.TaxCalculations
            .Include(tc => tc.Invoice)
            .Where(tc => tc.TaxType == taxType)
            .OrderByDescending(tc => tc.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaxCalculation>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.TaxCalculations
            .Include(tc => tc.Invoice)
                .ThenInclude(i => i.Contract)
            .Where(tc => tc.Invoice.Contract.ClientId == companyId || tc.Invoice.Contract.ProviderId == companyId)
            .OrderByDescending(tc => tc.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaxCalculation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TaxCalculations
            .Include(tc => tc.Invoice)
            .Where(tc => tc.CreatedAt >= startDate && tc.CreatedAt <= endDate)
            .OrderByDescending(tc => tc.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalTaxAmountAsync(Guid companyId, TaxType? taxType = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.TaxCalculations
            .Include(tc => tc.Invoice)
                .ThenInclude(i => i.Contract)
            .Where(tc => tc.Invoice.Contract.ClientId == companyId || tc.Invoice.Contract.ProviderId == companyId);

        if (taxType.HasValue)
            query = query.Where(tc => tc.TaxType == taxType.Value);

        if (startDate.HasValue)
            query = query.Where(tc => tc.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(tc => tc.CreatedAt <= endDate.Value);

        return await query.SumAsync(tc => tc.TaxAmount);
    }

    public async Task<decimal> GetTotalTaxBaseAsync(Guid companyId, TaxType? taxType = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.TaxCalculations
            .Include(tc => tc.Invoice)
                .ThenInclude(i => i.Contract)
            .Where(tc => tc.Invoice.Contract.ClientId == companyId || tc.Invoice.Contract.ProviderId == companyId);

        if (taxType.HasValue)
            query = query.Where(tc => tc.TaxType == taxType.Value);

        if (startDate.HasValue)
            query = query.Where(tc => tc.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(tc => tc.CreatedAt <= endDate.Value);

        return await query.SumAsync(tc => tc.TaxBase);
    }

    public async Task<IEnumerable<TaxCalculation>> GetTaxSummaryByTypeAsync(Guid companyId, DateTime startDate, DateTime endDate)
    {
        return await _context.TaxCalculations
            .Include(tc => tc.Invoice)
                .ThenInclude(i => i.Contract)
            .Where(tc => (tc.Invoice.Contract.ClientId == companyId || tc.Invoice.Contract.ProviderId == companyId) &&
                        tc.CreatedAt >= startDate && tc.CreatedAt <= endDate)
            .GroupBy(tc => tc.TaxType)
            .Select(g => new TaxCalculation(
                g.First().InvoiceId,
                g.Key,
                g.Average(tc => tc.TaxRate),
                g.Sum(tc => tc.TaxBase)
            ))
            .ToListAsync();
    }
}
