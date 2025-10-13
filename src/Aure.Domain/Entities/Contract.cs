using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Contract : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal ValueTotal { get; private set; }
    public decimal? MonthlyValue { get; private set; }
    public DateTime? SignedDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public string? IpfsCid { get; private set; }
    public string Sha256Hash { get; private set; } = string.Empty;
    public ContractStatus Status { get; private set; }

    public Company Client { get; private set; } = null!;
    public Company Provider { get; private set; } = null!;
    public TokenizedAsset? TokenizedAsset { get; private set; }

    private readonly List<Signature> _signatures = new();
    private readonly List<Payment> _payments = new();
    private readonly List<SplitRule> _splitRules = new();
    private readonly List<Notification> _notifications = new();

    public IReadOnlyCollection<Signature> Signatures => _signatures.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<SplitRule> SplitRules => _splitRules.AsReadOnly();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    private Contract() { }

    public Contract(Guid clientId, Guid providerId, string title, string description, decimal valueTotal, 
                   decimal? monthlyValue, DateTime startDate, DateTime? expirationDate, string sha256Hash)
    {
        ClientId = clientId;
        ProviderId = providerId;
        Title = title;
        Description = description;
        ValueTotal = valueTotal;
        MonthlyValue = monthlyValue;
        StartDate = startDate;
        ExpirationDate = expirationDate;
        Sha256Hash = sha256Hash;
        Status = ContractStatus.Draft;
    }

    public void UpdateContractDetails(string title, decimal valueTotal)
    {
        Title = title;
        ValueTotal = valueTotal;
        UpdateTimestamp();
    }

    public void SetIpfsCid(string ipfsCid)
    {
        IpfsCid = ipfsCid;
        UpdateTimestamp();
    }

    public void ActivateContract()
    {
        Status = ContractStatus.Active;
        if (!SignedDate.HasValue)
        {
            SignedDate = DateTime.UtcNow;
        }
        UpdateTimestamp();
    }

    public void SignContract()
    {
        if (!SignedDate.HasValue)
        {
            SignedDate = DateTime.UtcNow;
        }
        UpdateTimestamp();
    }

    public bool IsExpired => ExpirationDate.HasValue && DateTime.UtcNow > ExpirationDate.Value;
    
    public bool IsActive => Status == ContractStatus.Active && !IsExpired;

    public bool CanBeModified()
    {
        return Status == ContractStatus.Draft;
    }

    public void UpdateStatus(ContractStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }

    public void CompleteContract()
    {
        Status = ContractStatus.Completed;
        UpdateTimestamp();
    }

    public void CancelContract()
    {
        Status = ContractStatus.Cancelled;
        UpdateTimestamp();
    }

    public void AddSignature(Signature signature)
    {
        _signatures.Add(signature);
    }

    public void AddPayment(Payment payment)
    {
        _payments.Add(payment);
    }

    public void AddSplitRule(SplitRule splitRule)
    {
        _splitRules.Add(splitRule);
    }

    public void AddNotification(Notification notification)
    {
        _notifications.Add(notification);
    }

    public bool IsFullySigned => _signatures.Count >= 2;
}