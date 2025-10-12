using Microsoft.Extensions.Logging;
using Aure.Application.Interfaces;
using Aure.Application.DTOs.Common;
using Aure.Domain.Common;

namespace Aure.Application.Services;

public class CnpjValidationService : ICnpjValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CnpjValidationService> _logger;

    public CnpjValidationService(HttpClient httpClient, ILogger<CnpjValidationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CnpjValidationResult> ValidateAsync(string cnpj)
    {
        try
        {
            // Primeiro, validar algoritmo do CNPJ
            if (!ValidationHelpers.IsValidCnpj(cnpj))
            {
                return new CnpjValidationResult(false, null, null, null, "CNPJ inválido");
            }

            // Para agora, retornar validação básica
            // TODO: Implementar chamada para API externa (ReceitaWS/BrasilAPI)
            _logger.LogInformation("CNPJ validation requested for {Cnpj}", cnpj);
            
            return new CnpjValidationResult(true, "Empresa Teste", "Nome Fantasia", "ATIVA", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CNPJ {Cnpj}", cnpj);
            return new CnpjValidationResult(false, null, null, null, "Erro ao validar CNPJ");
        }
    }
}