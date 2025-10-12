using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class UserInvite : BaseEntity
{
    public string InviterName { get; private set; }
    public string InviteeEmail { get; private set; }
    public string InviteeName { get; private set; }
    public UserRole Role { get; private set; }
    public BusinessModel? BusinessModel { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Cnpj { get; private set; }
    public CompanyType? CompanyType { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsAccepted { get; private set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public InviteType InviteType { get; private set; }

    // Navigation properties
    public virtual Company Company { get; private set; } = null!;
    public virtual User InvitedByUser { get; private set; } = null!;

    protected UserInvite() 
    {
        InviterName = string.Empty;
        InviteeEmail = string.Empty;
        InviteeName = string.Empty;
        Token = string.Empty;
    } // EF Core

    public UserInvite(
        string inviterName,
        string inviteeEmail, 
        string inviteeName,
        UserRole role,
        Guid companyId,
        Guid invitedByUserId,
        InviteType inviteType,
        BusinessModel? businessModel = null,
        string? companyName = null,
        string? cnpj = null,
        CompanyType? companyType = null,
        int expirationHours = 168) // 7 days default
    {
        InviterName = inviterName;
        InviteeEmail = inviteeEmail;
        InviteeName = inviteeName;
        Role = role;
        CompanyId = companyId;
        InvitedByUserId = invitedByUserId;
        InviteType = inviteType;
        BusinessModel = businessModel;
        CompanyName = companyName;
        Cnpj = cnpj;
        CompanyType = companyType;
        Token = GenerateInviteToken();
        ExpiresAt = DateTime.UtcNow.AddHours(expirationHours);
        IsAccepted = false;
    }

    public void MarkAsAccepted()
    {
        IsAccepted = true;
    }

    private static string GenerateInviteToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}