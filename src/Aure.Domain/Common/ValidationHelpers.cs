using System.Text.RegularExpressions;

namespace Aure.Domain.Common;

public static class ValidationHelpers
{
    /// <summary>
    /// Valida se o email tem formato correto
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida formato de CNPJ (14 dígitos com ou sem formatação)
    /// </summary>
    public static bool IsValidCnpjFormat(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove formatação
        var cleanCnpj = RemoveCnpjFormatting(cnpj);
        
        // Deve ter exatamente 14 dígitos
        return cleanCnpj.Length == 14 && cleanCnpj.All(char.IsDigit);
    }

    /// <summary>
    /// Valida CNPJ usando algoritmo oficial da Receita Federal
    /// </summary>
    public static bool IsValidCnpj(string cnpj)
    {
        if (!IsValidCnpjFormat(cnpj))
            return false;

        var cleanCnpj = RemoveCnpjFormatting(cnpj);
        
        // CNPJs inválidos conhecidos (todos os dígitos iguais)
        if (new string(cleanCnpj[0], 14) == cleanCnpj)
            return false;

        // Cálculo do primeiro dígito verificador
        var multiplicador1 = new int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma1 = 0;
        for (int i = 0; i < 12; i++)
        {
            soma1 += int.Parse(cleanCnpj[i].ToString()) * multiplicador1[i];
        }
        var resto1 = soma1 % 11;
        var digito1 = resto1 < 2 ? 0 : 11 - resto1;

        // Cálculo do segundo dígito verificador
        var multiplicador2 = new int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma2 = 0;
        for (int i = 0; i < 13; i++)
        {
            soma2 += int.Parse(cleanCnpj[i].ToString()) * multiplicador2[i];
        }
        var resto2 = soma2 % 11;
        var digito2 = resto2 < 2 ? 0 : 11 - resto2;

        // Verifica se os dígitos calculados conferem
        return int.Parse(cleanCnpj[12].ToString()) == digito1 && 
               int.Parse(cleanCnpj[13].ToString()) == digito2;
    }

    /// <summary>
    /// Remove formatação do CNPJ (pontos, barras, hífens)
    /// </summary>
    public static string RemoveCnpjFormatting(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return string.Empty;

        return Regex.Replace(cnpj, @"[^\d]", "");
    }

    /// <summary>
    /// Formata CNPJ para o padrão XX.XXX.XXX/XXXX-XX
    /// </summary>
    public static string FormatCnpj(string cnpj)
    {
        var cleanCnpj = RemoveCnpjFormatting(cnpj);
        
        if (cleanCnpj.Length != 14)
            return cnpj;

        return $"{cleanCnpj.Substring(0, 2)}.{cleanCnpj.Substring(2, 3)}.{cleanCnpj.Substring(5, 3)}/{cleanCnpj.Substring(8, 4)}-{cleanCnpj.Substring(12, 2)}";
    }

    /// <summary>
    /// Valida se a senha atende aos critérios mínimos de segurança
    /// </summary>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        // Mínimo 8 caracteres, pelo menos uma maiúscula, uma minúscula, um número e um caractere especial
        var hasMinLength = password.Length >= 8;
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasMinLength && hasUpper && hasLower && hasDigit && hasSpecial;
    }

    /// <summary>
    /// Valida se o nome da empresa tem formato válido
    /// </summary>
    public static bool IsValidCompanyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Mínimo 2 caracteres, máximo 200, apenas letras, números, espaços e alguns caracteres especiais
        var isValidLength = name.Trim().Length >= 2 && name.Trim().Length <= 200;
        var hasValidChars = Regex.IsMatch(name, @"^[a-zA-ZÀ-ÿ0-9\s\-\.\&\(\)]+$");
        
        return isValidLength && hasValidChars;
    }

    /// <summary>
    /// Valida se o nome da pessoa tem formato válido
    /// </summary>
    public static bool IsValidPersonName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Mínimo 2 caracteres, máximo 100, apenas letras, espaços e acentos
        var isValidLength = name.Trim().Length >= 2 && name.Trim().Length <= 100;
        var hasValidChars = Regex.IsMatch(name, @"^[a-zA-ZÀ-ÿ\s]+$");
        
        return isValidLength && hasValidChars;
    }
}