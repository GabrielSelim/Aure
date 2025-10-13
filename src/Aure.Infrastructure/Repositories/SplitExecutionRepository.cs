using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class SplitExecutionRepository : BaseRepository<SplitExecution>, ISplitExecutionRepository
{
    public SplitExecutionRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<SplitExecution>> GetByPaymentIdAsync(Guid paymentId)
    {
        return await _context.SplitExecutions
            .Include(se => se.Payment)
            .Include(se => se.SplitRule)
            .Where(se => se.PaymentId == paymentId)
            .OrderBy(se => se.SplitRule.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<SplitExecution>> GetBySplitRuleIdAsync(Guid splitRuleId)
    {
        return await _context.SplitExecutions
            .Include(se => se.Payment)
            .Include(se => se.SplitRule)
            .Where(se => se.SplitId == splitRuleId)
            .OrderByDescending(se => se.ExecutedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SplitExecution>> GetByStatusAsync(SplitExecutionStatus status)
    {
        return await _context.SplitExecutions
            .Include(se => se.Payment)
            .Include(se => se.SplitRule)
            .Where(se => se.Status == status)
            .OrderByDescending(se => se.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SplitExecution>> GetPendingExecutionsAsync()
    {
        return await GetByStatusAsync(SplitExecutionStatus.Pending);
    }

    public async Task<IEnumerable<SplitExecution>> GetFailedExecutionsAsync()
    {
        return await GetByStatusAsync(SplitExecutionStatus.Failed);
    }

    public async Task<decimal> GetTotalExecutedValueAsync(Guid paymentId)
    {
        return await _context.SplitExecutions
            .Where(se => se.PaymentId == paymentId && se.Status == SplitExecutionStatus.Completed)
            .SumAsync(se => se.Value);
    }

    public async Task<IEnumerable<SplitExecution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.SplitExecutions
            .Include(se => se.Payment)
            .Include(se => se.SplitRule)
            .Where(se => se.ExecutedAt >= startDate && 
                        se.ExecutedAt <= endDate)
            .OrderByDescending(se => se.ExecutedAt)
            .ToListAsync();
    }
}
