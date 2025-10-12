namespace Aure.Application.Interfaces;

/// <summary>
/// Interface para processamento em background de validações pesadas
/// </summary>
public interface IBackgroundValidationService
{
    /// <summary>
    /// Processa validação de CNPJ em background
    /// </summary>
    Task<string> QueueCnpjValidationAsync(string cnpj, string companyName, Guid registrationId);
    
    /// <summary>
    /// Verifica status de validação em background
    /// </summary>
    Task<BackgroundValidationStatus> GetValidationStatusAsync(string jobId);
}

public record BackgroundValidationStatus(
    bool IsCompleted,
    bool? IsValid,
    string? ErrorMessage,
    DateTime? CompletedAt
);