using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid ContractId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? PaymentDate { get; private set; }

    public Contract Contract { get; private set; } = null!;

    private readonly List<SplitExecution> _splitExecutions = new();
    private readonly List<LedgerEntry> _ledgerEntries = new();
    private readonly List<Notification> _notifications = new();

    public IReadOnlyCollection<SplitExecution> SplitExecutions => _splitExecutions.AsReadOnly();
    public IReadOnlyCollection<LedgerEntry> LedgerEntries => _ledgerEntries.AsReadOnly();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    private Payment() { }

    public Payment(Guid contractId, decimal amount, PaymentMethod method)
    {
        ContractId = contractId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
    }

    public void ProcessPayment()
    {
        Status = PaymentStatus.Completed;
        PaymentDate = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void FailPayment()
    {
        Status = PaymentStatus.Failed;
        UpdateTimestamp();
    }

    public void CancelPayment()
    {
        Status = PaymentStatus.Cancelled;
        UpdateTimestamp();
    }

    public void AddSplitExecution(SplitExecution splitExecution)
    {
        _splitExecutions.Add(splitExecution);
    }

    public void AddLedgerEntry(LedgerEntry ledgerEntry)
    {
        _ledgerEntries.Add(ledgerEntry);
    }

    public void AddNotification(Notification notification)
    {
        _notifications.Add(notification);
    }
}