using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Company? Company { get; private set; }

    private readonly List<Session> _sessions = new();
    private readonly List<Signature> _signatures = new();
    private readonly List<AuditLog> _auditLogs = new();

    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    public IReadOnlyCollection<Signature> Signatures => _signatures.AsReadOnly();
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    private User() { }

    public User(string name, string email, string passwordHash, UserRole role, Guid? companyId = null)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CompanyId = companyId;
    }

    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;
        UpdateTimestamp();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdateTimestamp();
    }

    public void AddSession(Session session)
    {
        _sessions.Add(session);
    }

    public void AddSignature(Signature signature)
    {
        _signatures.Add(signature);
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        _auditLogs.Add(auditLog);
    }
}