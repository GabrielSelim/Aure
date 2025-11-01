using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public AuditAction Action { get; set; }
    public Guid? PerformedBy { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public int StatusCode { get; set; }
    public double Duration { get; set; }
    public bool Success { get; set; }
    public string HashChain { get; set; } = string.Empty;

    public User? User { get; set; }

    public AuditLog()
    {
        Timestamp = DateTime.UtcNow;
    }

    public static string GenerateHashChain(string previousHash, string currentData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var input = $"{previousHash}{currentData}{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}";
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}