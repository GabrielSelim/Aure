using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public int ItemSequence { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string NcmCode { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitValue { get; private set; }
    public decimal TotalValue { get; private set; }
    public string TaxClassification { get; private set; } = string.Empty;

    public Invoice Invoice { get; private set; } = null!;

    private InvoiceItem() { }

    public InvoiceItem(Guid invoiceId, int itemSequence, string description, string ncmCode, decimal quantity, decimal unitValue, string taxClassification)
    {
        InvoiceId = invoiceId;
        ItemSequence = itemSequence;
        Description = description;
        NcmCode = ncmCode;
        Quantity = quantity;
        UnitValue = unitValue;
        TotalValue = quantity * unitValue;
        TaxClassification = taxClassification;
    }

    public void UpdateItem(string description, decimal quantity, decimal unitValue)
    {
        Description = description;
        Quantity = quantity;
        UnitValue = unitValue;
        TotalValue = quantity * unitValue;
        UpdateTimestamp();
    }

    public void UpdateTaxClassification(string taxClassification, string ncmCode)
    {
        TaxClassification = taxClassification;
        NcmCode = ncmCode;
        UpdateTimestamp();
    }
}