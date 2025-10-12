namespace Aure.Application.DTOs.Common;

public record CnpjValidationResult(
    bool IsValid,
    string? CompanyName,
    string? TradeName,
    string? Status,
    string? ErrorMessage
)
{
    public string CleanCnpj => System.Text.RegularExpressions.Regex.Replace(
        ErrorMessage ?? "", @"\D", ""); // Para agora, vou implementar depois
};