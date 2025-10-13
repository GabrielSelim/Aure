using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class SplitRuleRepository : BaseRepository<SplitRule>, ISplitRuleRepository
{
    public SplitRuleRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<SplitRule>> GetByContractIdAsync(Guid contractId)
    {
        return await _context.SplitRules
            .Where(s => !s.IsDeleted && s.ContractId == contractId)
            .Include(s => s.Contract)
            .OrderBy(s => s.Priority)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalPercentageByContractAsync(Guid contractId)
    {
        return await _context.SplitRules
            .Where(s => !s.IsDeleted && s.ContractId == contractId)
            .SumAsync(s => s.Percentage);
    }
}