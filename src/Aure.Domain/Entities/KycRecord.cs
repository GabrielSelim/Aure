using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class KycRecord : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentHash { get; private set; } = string.Empty;
    public DateTime? VerifiedAt { get; private set; }
    public KycStatus Status { get; private set; }
    public string? ProviderRef { get; private set; }

    public Company Company { get; private set; } = null!;

    private KycRecord() { }

    public KycRecord(Guid companyId, DocumentType documentType, string documentHash, string? providerRef = null)
    {
        CompanyId = companyId;
        DocumentType = documentType;
        DocumentHash = documentHash;
        Status = KycStatus.Pending;
        ProviderRef = providerRef;
    }

    public void VerifyDocument()
    {
        Status = KycStatus.Approved;
        VerifiedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void RejectDocument()
    {
        Status = KycStatus.Rejected;
        UpdateTimestamp();
    }

    public void UpdateProviderRef(string providerRef)
    {
        ProviderRef = providerRef;
        UpdateTimestamp();
    }
}