using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class CompanyRelationship : BaseEntity
{
    public Guid ClientCompanyId { get; private set; }
    public Guid ProviderCompanyId { get; private set; }
    public RelationshipType Type { get; private set; }
    public RelationshipStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public virtual Company ClientCompany { get; private set; } = null!;
    public virtual Company ProviderCompany { get; private set; } = null!;

    protected CompanyRelationship() { } // EF Core

    public CompanyRelationship(
        Guid clientCompanyId,
        Guid providerCompanyId,
        RelationshipType type,
        DateTime? startDate = null,
        string? notes = null)
    {
        ClientCompanyId = clientCompanyId;
        ProviderCompanyId = providerCompanyId;
        Type = type;
        Status = RelationshipStatus.Active;
        StartDate = startDate ?? DateTime.UtcNow;
        Notes = notes;
    }

    public void UpdateStatus(RelationshipStatus status)
    {
        Status = status;
        if (status == RelationshipStatus.Terminated)
        {
            EndDate = DateTime.UtcNow;
        }
        UpdateTimestamp();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        UpdateTimestamp();
    }

    public bool IsActive => Status == RelationshipStatus.Active && (EndDate == null || EndDate > DateTime.UtcNow);
}