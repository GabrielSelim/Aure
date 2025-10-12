using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class LedgerEntry : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public Guid ContractId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public DateTime Timestamp { get; private set; }
    public string? Note { get; private set; }

    public Payment Payment { get; private set; } = null!;
    public Contract Contract { get; private set; } = null!;

    private LedgerEntry() { }

    public LedgerEntry(Guid paymentId, Guid contractId, decimal debit, decimal credit, string? note = null)
    {
        PaymentId = paymentId;
        ContractId = contractId;
        Debit = debit;
        Credit = credit;
        Timestamp = DateTime.UtcNow;
        Note = note;
    }

    public void UpdateNote(string note)
    {
        Note = note;
        UpdateTimestamp();
    }

    public decimal GetBalance()
    {
        return Credit - Debit;
    }
}