using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ISplitRuleRepository
{
    Task<SplitRule?> GetByIdAsync(Guid id);
    Task<IEnumerable<SplitRule>> GetAllAsync();
    Task<IEnumerable<SplitRule>> GetByContractIdAsync(Guid contractId);
    Task AddAsync(SplitRule entity);
    Task UpdateAsync(SplitRule entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<decimal> GetTotalPercentageByContractAsync(Guid contractId);
}