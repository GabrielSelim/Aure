namespace Aure.Application.Interfaces;

/// <summary>
/// Serviço para criptografia e descriptografia de dados sensíveis (CPF, RG)
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Criptografa um texto usando AES-256
    /// </summary>
    /// <param name="plainText">Texto a ser criptografado</param>
    /// <returns>Texto criptografado em Base64</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Descriptografa um texto criptografado
    /// </summary>
    /// <param name="cipherText">Texto criptografado em Base64</param>
    /// <returns>Texto descriptografado</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Mascara um CPF para exibição (***.***.123-45)
    /// </summary>
    /// <param name="cpf">CPF a ser mascarado</param>
    /// <returns>CPF mascarado</returns>
    string MaskCPF(string cpf);

    /// <summary>
    /// Mascara um RG para exibição (******789)
    /// </summary>
    /// <param name="rg">RG a ser mascarado</param>
    /// <returns>RG mascarado</returns>
    string MaskRG(string rg);
}
