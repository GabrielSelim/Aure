namespace Aure.Application.DTOs.Common;

public record CnpjValidationResult(
    bool IsValid,
    string? CompanyName,
    string? TradeName,
    string? Status,
    string? ErrorMessage
);