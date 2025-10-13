using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ISplitExecutionRepository : IBaseRepository<SplitExecution>
{
    Task<IEnumerable<SplitExecution>> GetByPaymentIdAsync(Guid paymentId);
    Task<IEnumerable<SplitExecution>> GetBySplitRuleIdAsync(Guid splitRuleId);
    Task<IEnumerable<SplitExecution>> GetByStatusAsync(SplitExecutionStatus status);
    Task<IEnumerable<SplitExecution>> GetPendingExecutionsAsync();
    Task<IEnumerable<SplitExecution>> GetFailedExecutionsAsync();
    Task<decimal> GetTotalExecutedValueAsync(Guid paymentId);
    Task<IEnumerable<SplitExecution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}