using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class LedgerEntryRepository : BaseRepository<LedgerEntry>, ILedgerEntryRepository
{
    public LedgerEntryRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<LedgerEntry>> GetByPaymentIdAsync(Guid paymentId)
    {
        return await _context.LedgerEntries
            .Include(le => le.Payment)
            .Include(le => le.Contract)
            .Where(le => le.PaymentId == paymentId)
            .OrderByDescending(le => le.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<LedgerEntry>> GetByContractIdAsync(Guid contractId)
    {
        return await _context.LedgerEntries
            .Include(le => le.Payment)
            .Include(le => le.Contract)
            .Where(le => le.ContractId == contractId)
            .OrderByDescending(le => le.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<LedgerEntry>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.LedgerEntries
            .Include(le => le.Payment)
            .Include(le => le.Contract)
            .Where(le => le.Contract.ClientId == companyId || le.Contract.ProviderId == companyId)
            .OrderByDescending(le => le.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<LedgerEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.LedgerEntries
            .Include(le => le.Payment)
            .Include(le => le.Contract)
            .Where(le => le.Timestamp >= startDate && le.Timestamp <= endDate)
            .OrderByDescending(le => le.Timestamp)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalDebitAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.LedgerEntries
            .Where(le => le.Contract.ClientId == companyId || le.Contract.ProviderId == companyId);

        if (startDate.HasValue)
            query = query.Where(le => le.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(le => le.Timestamp <= endDate.Value);

        return await query.SumAsync(le => le.Debit);
    }

    public async Task<decimal> GetTotalCreditAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.LedgerEntries
            .Where(le => le.Contract.ClientId == companyId || le.Contract.ProviderId == companyId);

        if (startDate.HasValue)
            query = query.Where(le => le.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(le => le.Timestamp <= endDate.Value);

        return await query.SumAsync(le => le.Credit);
    }

    public async Task<decimal> GetBalanceAsync(Guid companyId, DateTime? asOfDate = null)
    {
        var query = _context.LedgerEntries
            .Where(le => le.Contract.ClientId == companyId || le.Contract.ProviderId == companyId);

        if (asOfDate.HasValue)
            query = query.Where(le => le.Timestamp <= asOfDate.Value);

        var totalCredit = await query.SumAsync(le => le.Credit);
        var totalDebit = await query.SumAsync(le => le.Debit);

        return totalCredit - totalDebit;
    }
}
