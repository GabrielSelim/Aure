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

            _logger.LogInformation("CNPJ validation requested for {Cnpj}", cnpj);

            // Tentar validar com API da BrasilAPI
            var brasilApiResult = await ValidateWithBrasilApiAsync(cnpj);
            if (brasilApiResult != null)
            {
                return brasilApiResult;
            }

            // Se BrasilAPI falhar, tentar ReceitaWS
            var receitaWsResult = await ValidateWithReceitaWsAsync(cnpj);
            if (receitaWsResult != null)
            {
                return receitaWsResult;
            }

            // Se ambas as APIs falharem, retornar erro
            _logger.LogError("Both CNPJ validation APIs failed for {Cnpj}. Cannot validate company data", cnpj);
            return new CnpjValidationResult(false, null, null, null, "Serviço de validação de CNPJ temporariamente indisponível. Tente novamente em alguns minutos.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CNPJ {Cnpj}", cnpj);
            return new CnpjValidationResult(false, null, null, null, "Erro ao validar CNPJ");
        }
    }

    private async Task<CnpjValidationResult?> ValidateWithBrasilApiAsync(string cnpj)
    {
        try
        {
            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            var url = $"https://brasilapi.com.br/api/cnpj/v1/{cleanCnpj}";
            
            _logger.LogInformation("Calling BrasilAPI for CNPJ {Cnpj} at URL: {Url}", cnpj, url);
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BrasilAPI returned {StatusCode} for CNPJ {Cnpj}. Response: {Response}", 
                    response.StatusCode, cnpj, await response.Content.ReadAsStringAsync());
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("BrasilAPI response JSON for CNPJ {Cnpj}: {Json}", cnpj, json);
            
            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            };
            
            var data = System.Text.Json.JsonSerializer.Deserialize<BrasilApiResponse>(json, options);

            if (data == null) 
            {
                _logger.LogWarning("Failed to deserialize BrasilAPI response for CNPJ {Cnpj}", cnpj);
                return null;
            }

            _logger.LogInformation("CNPJ {Cnpj} validated successfully with BrasilAPI. Company: {CompanyName}, Status: {Status}", 
                cnpj, data.RazaoSocial, data.DescricaoSituacaoCadastral);

            return new CnpjValidationResult(
                data.DescricaoSituacaoCadastral?.ToUpper().Contains("ATIVA") == true,
                data.RazaoSocial,
                data.NomeFantasia,
                data.DescricaoSituacaoCadastral,
                data.DescricaoSituacaoCadastral?.ToUpper().Contains("ATIVA") != true ? "Empresa não está ativa" : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate CNPJ {Cnpj} with BrasilAPI", cnpj);
            return null;
        }
    }

    private async Task<CnpjValidationResult?> ValidateWithReceitaWsAsync(string cnpj)
    {
        try
        {
            var cleanCnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
            var url = $"https://www.receitaws.com.br/v1/cnpj/{cleanCnpj}";
            
            _logger.LogInformation("Calling ReceitaWS for CNPJ {Cnpj} at URL: {Url}", cnpj, url);
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ReceitaWS returned {StatusCode} for CNPJ {Cnpj}. Response: {Response}", 
                    response.StatusCode, cnpj, await response.Content.ReadAsStringAsync());
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("ReceitaWS response JSON for CNPJ {Cnpj}: {Json}", cnpj, json);
            
            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true
            };
            
            var data = System.Text.Json.JsonSerializer.Deserialize<ReceitaWsResponse>(json, options);

            if (data == null || data.Status == "ERROR") 
            {
                _logger.LogWarning("ReceitaWS returned error or null data for CNPJ {Cnpj}. Status: {Status}", cnpj, data?.Status);
                return null;
            }

            _logger.LogInformation("CNPJ {Cnpj} validated successfully with ReceitaWS. Company: {CompanyName}, Status: {Status}", 
                cnpj, data.Nome, data.Situacao);

            return new CnpjValidationResult(
                data.Situacao?.ToUpper() == "ATIVA",
                data.Nome,
                data.Fantasia,
                data.Situacao,
                data.Situacao?.ToUpper() != "ATIVA" ? "Empresa não está ativa" : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate CNPJ {Cnpj} with ReceitaWS", cnpj);
            return null;
        }
    }

    // Classes para deserialização das APIs
    private class BrasilApiResponse
    {
        public string? RazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public string? DescricaoSituacaoCadastral { get; set; }
    }

    private class ReceitaWsResponse
    {
        public string? Status { get; set; }
        public string? Nome { get; set; }
        public string? Fantasia { get; set; }
        public string? Situacao { get; set; }
    }
}