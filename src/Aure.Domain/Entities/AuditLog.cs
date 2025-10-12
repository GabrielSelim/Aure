using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public Guid PerformedBy { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public string HashChain { get; private set; } = string.Empty;

    public User User { get; private set; } = null!;

    private AuditLog() { }

    public AuditLog(string entityName, Guid entityId, AuditAction action, Guid performedBy, string ipAddress, string hashChain)
    {
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        PerformedBy = performedBy;
        IpAddress = ipAddress;
        Timestamp = DateTime.UtcNow;
        HashChain = hashChain;
    }

    public static string GenerateHashChain(string previousHash, string currentData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var input = $"{previousHash}{currentData}{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}";
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}