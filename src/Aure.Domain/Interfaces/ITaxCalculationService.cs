using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ITaxCalculationService
{
    Task<IEnumerable<TaxCalculation>> CalculateInvoiceTaxesAsync(Invoice invoice);
    Task<TaxCalculation> CalculateSpecificTaxAsync(Invoice invoice, TaxType taxType);
    Task<decimal> CalculateTaxRateAsync(TaxType taxType, Company company, decimal baseValue);
    Task<bool> ValidateTaxCalculationAsync(TaxCalculation taxCalculation);
    Task<TaxSummary> GetTaxSummaryAsync(Guid companyId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TaxRegime>> GetApplicableTaxRegimesAsync(Company company);
}

public class TaxSummary
{
    public Guid CompanyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalTaxBase { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public IEnumerable<TaxTypeSummary> TaxesByType { get; set; } = new List<TaxTypeSummary>();
}

public class TaxTypeSummary
{
    public TaxType TaxType { get; set; }
    public decimal TotalBase { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageRate { get; set; }
    public int TransactionCount { get; set; }
}

public class TaxRegime
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<TaxTypeRate> ApplicableRates { get; set; } = new List<TaxTypeRate>();
}

public class TaxTypeRate
{
    public TaxType TaxType { get; set; }
    public decimal Rate { get; set; }
    public decimal MinimumBase { get; set; }
    public decimal MaximumBase { get; set; }
}