using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Signature : BaseEntity
{
    public Guid ContractId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime SignedAt { get; private set; }
    public string SignatureHash { get; private set; } = string.Empty;
    public SignatureMethod Method { get; private set; }

    public Contract Contract { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private Signature() { }

    public Signature(Guid contractId, Guid userId, string signatureHash, SignatureMethod method)
    {
        ContractId = contractId;
        UserId = userId;
        SignedAt = DateTime.UtcNow;
        SignatureHash = signatureHash;
        Method = method;
    }

    public bool IsValidSignature(string dataToVerify)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dataToVerify));
        var computedHash = Convert.ToBase64String(hash);
        return computedHash == SignatureHash;
    }
}