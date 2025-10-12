using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid ContractId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string Series { get; private set; } = string.Empty;
    public string AccessKey { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string XmlContent { get; private set; } = string.Empty;
    public string? PdfUrl { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? SefazProtocol { get; private set; }

    public Contract Contract { get; private set; } = null!;
    public Payment? Payment { get; private set; }

    private readonly List<InvoiceItem> _items = new();
    private readonly List<TaxCalculation> _taxCalculations = new();

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<TaxCalculation> TaxCalculations => _taxCalculations.AsReadOnly();

    private Invoice() { }

    public Invoice(Guid contractId, string invoiceNumber, string series, string accessKey, decimal totalAmount, decimal taxAmount, string xmlContent)
    {
        ContractId = contractId;
        InvoiceNumber = invoiceNumber;
        Series = series;
        AccessKey = accessKey;
        IssueDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        XmlContent = xmlContent;
        Status = InvoiceStatus.Draft;
    }

    public void IssueInvoice(string sefazProtocol)
    {
        Status = InvoiceStatus.Issued;
        SefazProtocol = sefazProtocol;
        UpdateTimestamp();
    }

    public void MarkAsSent(string pdfUrl)
    {
        Status = InvoiceStatus.Sent;
        PdfUrl = pdfUrl;
        UpdateTimestamp();
    }

    public void CancelInvoice(string cancellationReason)
    {
        Status = InvoiceStatus.Cancelled;
        CancellationReason = cancellationReason;
        UpdateTimestamp();
    }

    public void MarkAsError()
    {
        Status = InvoiceStatus.Error;
        UpdateTimestamp();
    }

    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        UpdateTimestamp();
    }

    public void AssociatePayment(Guid paymentId)
    {
        PaymentId = paymentId;
        UpdateTimestamp();
    }

    public void AddItem(InvoiceItem item)
    {
        _items.Add(item);
    }

    public void AddTaxCalculation(TaxCalculation taxCalculation)
    {
        _taxCalculations.Add(taxCalculation);
    }
}