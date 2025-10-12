using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class TaxCalculation : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public TaxType TaxType { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxBase { get; private set; }
    public decimal TaxAmount { get; private set; }

    public Invoice Invoice { get; private set; } = null!;

    private TaxCalculation() { }

    public TaxCalculation(Guid invoiceId, TaxType taxType, decimal taxRate, decimal taxBase)
    {
        InvoiceId = invoiceId;
        TaxType = taxType;
        TaxRate = taxRate;
        TaxBase = taxBase;
        TaxAmount = taxBase * (taxRate / 100);
    }

    public void RecalculateTax(decimal newTaxRate, decimal newTaxBase)
    {
        TaxRate = newTaxRate;
        TaxBase = newTaxBase;
        TaxAmount = newTaxBase * (newTaxRate / 100);
        UpdateTimestamp();
    }

    public decimal GetEffectiveTaxRate()
    {
        return TaxBase > 0 ? (TaxAmount / TaxBase) * 100 : 0;
    }
}