using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface ISplitService
{
    Task<IEnumerable<SplitExecution>> ProcessPaymentSplitAsync(Payment payment);
    Task<SplitExecution> ExecuteSplitRuleAsync(Payment payment, SplitRule splitRule);
    Task<bool> ValidateSplitRulesAsync(IEnumerable<SplitRule> splitRules);
    Task<decimal> CalculateTotalSplitValueAsync(decimal paymentAmount, IEnumerable<SplitRule> splitRules);
    Task<SplitExecutionSummary> GetSplitExecutionSummaryAsync(Guid paymentId);
    Task<bool> RetrySplitExecutionAsync(Guid splitExecutionId);
}

public class SplitExecutionSummary
{
    public Guid PaymentId { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public decimal TotalExecutedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int TotalRules { get; set; }
    public int ExecutedRules { get; set; }
    public int PendingRules { get; set; }
    public int FailedRules { get; set; }
    public IEnumerable<SplitExecution> Executions { get; set; } = new List<SplitExecution>();
}