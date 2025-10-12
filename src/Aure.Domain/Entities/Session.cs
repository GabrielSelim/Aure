using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Session : BaseEntity
{
    public Guid UserId { get; private set; }
    public string JwtHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }

    public User User { get; private set; } = null!;

    private Session() { }

    public Session(Guid userId, string jwtHash, DateTime expiresAt)
    {
        UserId = userId;
        JwtHash = jwtHash;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public void ExtendSession(DateTime newExpiresAt)
    {
        ExpiresAt = newExpiresAt;
        UpdateTimestamp();
    }

    public void InvalidateSession()
    {
        ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        UpdateTimestamp();
    }
}