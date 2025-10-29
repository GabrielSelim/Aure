using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class UserInvitation : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? Cargo { get; private set; }
    public Guid CompanyId { get; private set; }
    public Company? Company { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public User? InvitedByUser { get; private set; }
    public InvitationStatus Status { get; private set; }
    public string InvitationToken { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public User? AcceptedByUser { get; private set; }

    private UserInvitation() { }

    public UserInvitation(
        string name, 
        string email, 
        UserRole role, 
        string? cargo,
        Guid companyId, 
        Guid invitedByUserId,
        string invitationToken,
        int expirationDays = 7)
    {
        Name = name;
        Email = email;
        Role = role;
        Cargo = cargo;
        CompanyId = companyId;
        InvitedByUserId = invitedByUserId;
        InvitationToken = invitationToken;
        Status = InvitationStatus.Pending;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
    }

    public void UpdateInvitationInfo(string name, string email, UserRole role, string? cargo)
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Apenas convites pendentes podem ser editados");

        Name = name;
        Email = email;
        Role = role;
        Cargo = cargo;
        UpdateTimestamp();
    }

    public void MarkAsAccepted(Guid acceptedByUserId)
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Convite já foi processado");

        if (DateTime.UtcNow > ExpiresAt)
            throw new InvalidOperationException("Convite expirado");

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        AcceptedByUserId = acceptedByUserId;
        UpdateTimestamp();
    }

    public void MarkAsExpired()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Convite já foi processado");

        Status = InvitationStatus.Expired;
        UpdateTimestamp();
    }

    public void MarkAsCancelled()
    {
        if (Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Apenas convites pendentes podem ser cancelados");

        Status = InvitationStatus.Cancelled;
        UpdateTimestamp();
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt && Status == InvitationStatus.Pending;

    public bool CanBeEdited() => Status == InvitationStatus.Pending && !IsExpired();
}
