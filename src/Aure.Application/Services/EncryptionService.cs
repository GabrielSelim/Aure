using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Aure.Application.Interfaces;

namespace Aure.Application.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        // Chave e IV devem estar em variáveis de ambiente ou Azure Key Vault
        // NUNCA hardcoded no código
        var keyString = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption key not configured");
        var ivString = configuration["Encryption:IV"] 
            ?? throw new InvalidOperationException("Encryption IV not configured");

        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits)");
        
        if (_iv.Length != 16)
            throw new InvalidOperationException("Encryption IV must be 16 bytes (128 bits)");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        
        return Encoding.UTF8.GetString(plainBytes);
    }

    public string MaskCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return string.Empty;

        // Remove formatação
        var cleanCpf = cpf.Replace(".", "").Replace("-", "").Trim();

        if (cleanCpf.Length != 11)
            return "***.***.***-**";

        // Mascara: ***.***.123-45
        return $"***.***.{cleanCpf.Substring(6, 3)}-{cleanCpf.Substring(9, 2)}";
    }

    public string MaskRG(string rg)
    {
        if (string.IsNullOrWhiteSpace(rg))
            return string.Empty;

        // Remove formatação
        var cleanRg = rg.Replace(".", "").Replace("-", "").Trim();

        if (cleanRg.Length < 6)
            return new string('*', cleanRg.Length);

        // Mostra apenas os últimos 3 dígitos
        var visiblePart = cleanRg.Substring(cleanRg.Length - 3);
        var maskedPart = new string('*', cleanRg.Length - 3);
        
        return $"{maskedPart}{visiblePart}";
    }
}
