using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class SplitExecution : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public Guid SplitId { get; private set; }
    public decimal Value { get; private set; }
    public DateTime ExecutedAt { get; private set; }
    public string? TxHash { get; private set; }
    public SplitExecutionStatus Status { get; private set; }

    public Payment Payment { get; private set; } = null!;
    public SplitRule SplitRule { get; private set; } = null!;

    private SplitExecution() { }

    public SplitExecution(Guid paymentId, Guid splitId, decimal value)
    {
        PaymentId = paymentId;
        SplitId = splitId;
        Value = value;
        ExecutedAt = DateTime.UtcNow;
        Status = SplitExecutionStatus.Pending;
    }

    public void CompleteExecution(string? txHash = null)
    {
        Status = SplitExecutionStatus.Completed;
        TxHash = txHash;
        UpdateTimestamp();
    }

    public void FailExecution()
    {
        Status = SplitExecutionStatus.Failed;
        UpdateTimestamp();
    }
}