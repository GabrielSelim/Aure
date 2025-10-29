using Aure.Domain.Enums;

namespace Aure.Application.DTOs.User;

public class UserInvitationListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Cargo { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string InvitedByName { get; set; } = string.Empty;
    public string? AcceptedByName { get; set; }
    public bool IsExpired { get; set; }
    public bool CanBeEdited { get; set; }
}

public class UpdateInvitationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Cargo { get; set; }
}

public class UpdateInvitationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserInvitationListResponse? Invitation { get; set; }
}

public class CancelInvitationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
