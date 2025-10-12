using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class SplitRule : BaseEntity
{
    public Guid ContractId { get; private set; }
    public string BeneficiaryRef { get; private set; } = string.Empty;
    public decimal Percentage { get; private set; }
    public decimal? FixedFee { get; private set; }
    public int Priority { get; private set; }

    public Contract Contract { get; private set; } = null!;

    private readonly List<SplitExecution> _splitExecutions = new();
    public IReadOnlyCollection<SplitExecution> SplitExecutions => _splitExecutions.AsReadOnly();

    private SplitRule() { }

    public SplitRule(Guid contractId, string beneficiaryRef, decimal percentage, decimal? fixedFee, int priority)
    {
        ContractId = contractId;
        BeneficiaryRef = beneficiaryRef;
        Percentage = percentage;
        FixedFee = fixedFee;
        Priority = priority;
    }

    public void UpdateSplitRule(decimal percentage, decimal? fixedFee, int priority)
    {
        Percentage = percentage;
        FixedFee = fixedFee;
        Priority = priority;
        UpdateTimestamp();
    }

    public decimal CalculateSplitAmount(decimal totalAmount)
    {
        var percentageAmount = totalAmount * (Percentage / 100);
        return FixedFee.HasValue ? percentageAmount + FixedFee.Value : percentageAmount;
    }

    public void AddSplitExecution(SplitExecution splitExecution)
    {
        _splitExecutions.Add(splitExecution);
    }
}