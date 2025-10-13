using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Aure.Application.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Infrastructure.Configuration;

namespace Aure.Infrastructure.Services;

public class SefazService : ISefazService
{
    private readonly SefazSettings _sefazSettings;
    private readonly InvoiceSettings _invoiceSettings;
    private readonly ILogger<SefazService> _logger;
    private readonly HttpClient _httpClient;
    private X509Certificate2? _certificate;

    public SefazService(
        IOptions<SefazSettings> sefazSettings,
        IOptions<InvoiceSettings> invoiceSettings,
        ILogger<SefazService> logger,
        HttpClient httpClient)
    {
        _sefazSettings = sefazSettings.Value;
        _invoiceSettings = invoiceSettings.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMilliseconds(_sefazSettings.WebServiceTimeout);
        
        LoadCertificate();
    }

    public async Task<SefazResponse> IssueInvoiceAsync(Invoice invoice)
    {
        try
        {
            _logger.LogInformation("Iniciando emissão da nota fiscal {InvoiceId} para SEFAZ", invoice.Id);

            // 1. Gerar XML da NFe
            var xmlContent = await GenerateXmlAsync(invoice);
            if (string.IsNullOrEmpty(xmlContent))
            {
                return new SefazResponse
                {
                    Success = false,
                    Message = "Erro ao gerar XML da nota fiscal",
                    ErrorCode = "XML_GENERATION_ERROR"
                };
            }

            // 2. Assinar o XML digitalmente
            var signedXml = SignXml(xmlContent);
            if (string.IsNullOrEmpty(signedXml))
            {
                return new SefazResponse
                {
                    Success = false,
                    Message = "Erro ao assinar digitalmente o XML",
                    ErrorCode = "DIGITAL_SIGNATURE_ERROR"
                };
            }

            // 3. Enviar para SEFAZ
            var response = await SendToSefazAsync(signedXml, "NFeAutorizacao");
            
            if (response.Success)
            {
                _logger.LogInformation("Nota fiscal {InvoiceId} emitida com sucesso. Protocolo: {Protocol}", 
                    invoice.Id, response.Protocol);
            }
            else
            {
                _logger.LogError("Erro ao emitir nota fiscal {InvoiceId}: {ErrorMessage}", 
                    invoice.Id, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao emitir nota fiscal {InvoiceId}", invoice.Id);
            return new SefazResponse
            {
                Success = false,
                Message = $"Erro inesperado: {ex.Message}",
                ErrorCode = "UNEXPECTED_ERROR"
            };
        }
    }

    public async Task<SefazResponse> CancelInvoiceAsync(string accessKey, string reason)
    {
        try
        {
            _logger.LogInformation("Iniciando cancelamento da nota fiscal {AccessKey}", accessKey);

            // Gerar XML de cancelamento
            var cancelXml = GenerateCancellationXml(accessKey, reason);
            var signedXml = SignXml(cancelXml);

            var response = await SendToSefazAsync(signedXml, "NFeRecepcaoEvento");
            
            if (response.Success)
            {
                _logger.LogInformation("Nota fiscal {AccessKey} cancelada com sucesso", accessKey);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal {AccessKey}", accessKey);
            return new SefazResponse
            {
                Success = false,
                Message = $"Erro ao cancelar: {ex.Message}",
                ErrorCode = "CANCELLATION_ERROR"
            };
        }
    }

    public async Task<SefazStatusResponse> CheckStatusAsync(string accessKey)
    {
        try
        {
            var consultaXml = GenerateConsultationXml(accessKey);
            var signedXml = SignXml(consultaXml);

            var response = await SendToSefazAsync(signedXml, "NFeConsultaProtocolo");
            
            return new SefazStatusResponse
            {
                Success = response.Success,
                Status = response.Success ? "Autorizada" : "Rejeitada",
                Protocol = response.Protocol,
                Message = response.Message,
                LastUpdate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da nota fiscal {AccessKey}", accessKey);
            return new SefazStatusResponse
            {
                Success = false,
                Status = "Erro",
                Message = ex.Message,
                LastUpdate = DateTime.UtcNow
            };
        }
    }

    public async Task<string> GenerateXmlAsync(Invoice invoice)
    {
        try
        {
            // Implementação simplificada - na prática você usaria uma biblioteca como ACBr.Net.NFe
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine($"<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            xml.AppendLine("  <infNFe versao=\"4.00\">");
            xml.AppendLine($"    <ide>");
            xml.AppendLine($"      <cUF>35</cUF>"); // São Paulo
            xml.AppendLine($"      <cNF>00000001</cNF>");
            xml.AppendLine($"      <natOp>{_invoiceSettings.DefaultNatureOperation}</natOp>");
            xml.AppendLine($"      <mod>55</mod>"); // NFe
            xml.AppendLine($"      <serie>{invoice.Series}</serie>");
            xml.AppendLine($"      <nNF>{invoice.InvoiceNumber}</nNF>");
            xml.AppendLine($"      <dhEmi>{DateTime.Now:yyyy-MM-ddTHH:mm:sszzz}</dhEmi>");
            xml.AppendLine($"      <tpNF>1</tpNF>"); // Saída
            xml.AppendLine($"      <idDest>1</idDest>"); // Operação interna
            xml.AppendLine($"      <cMunFG>3550308</cMunFG>"); // São Paulo
            xml.AppendLine($"      <tpImp>1</tpImp>"); // DANFE Retrato
            xml.AppendLine($"      <tpEmis>1</tpEmis>"); // Emissão normal
            xml.AppendLine($"      <tpAmb>{(_sefazSettings.Environment == "Producao" ? "1" : "2")}</tpAmb>");
            xml.AppendLine($"      <finNFe>1</finNFe>"); // Normal
            xml.AppendLine($"      <indFinal>1</indFinal>"); // Consumidor final
            xml.AppendLine($"      <indPres>0</indPres>"); // Não se aplica
            xml.AppendLine($"    </ide>");
            
            // Emitente (será preenchido com dados da empresa)
            xml.AppendLine($"    <emit>");
            xml.AppendLine($"      <CNPJ>00000000000000</CNPJ>"); // Será substituído pelos dados reais
            xml.AppendLine($"      <xNome>Empresa Emitente Ltda</xNome>");
            xml.AppendLine($"      <enderEmit>");
            xml.AppendLine($"        <xLgr>Rua Exemplo</xLgr>");
            xml.AppendLine($"        <nro>123</nro>");
            xml.AppendLine($"        <xBairro>Centro</xBairro>");
            xml.AppendLine($"        <cMun>3550308</cMun>");
            xml.AppendLine($"        <xMun>São Paulo</xMun>");
            xml.AppendLine($"        <UF>SP</UF>");
            xml.AppendLine($"        <CEP>01000000</CEP>");
            xml.AppendLine($"      </enderEmit>");
            xml.AppendLine($"    </emit>");

            // Total (simplificado)
            xml.AppendLine($"    <total>");
            xml.AppendLine($"      <ICMSTot>");
            xml.AppendLine($"        <vBC>0.00</vBC>");
            xml.AppendLine($"        <vICMS>0.00</vICMS>");
            xml.AppendLine($"        <vBCST>0.00</vBCST>");
            xml.AppendLine($"        <vST>0.00</vST>");
            xml.AppendLine($"        <vProd>{invoice.TotalAmount:F2}</vProd>");
            xml.AppendLine($"        <vFrete>0.00</vFrete>");
            xml.AppendLine($"        <vSeg>0.00</vSeg>");
            xml.AppendLine($"        <vDesc>0.00</vDesc>");
            xml.AppendLine($"        <vII>0.00</vII>");
            xml.AppendLine($"        <vIPI>0.00</vIPI>");
            xml.AppendLine($"        <vPIS>0.00</vPIS>");
            xml.AppendLine($"        <vCOFINS>0.00</vCOFINS>");
            xml.AppendLine($"        <vOutro>0.00</vOutro>");
            xml.AppendLine($"        <vNF>{invoice.TotalAmount:F2}</vNF>");
            xml.AppendLine($"      </ICMSTot>");
            xml.AppendLine($"    </total>");

            xml.AppendLine("  </infNFe>");
            xml.AppendLine("</NFe>");

            return xml.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar XML da nota fiscal {InvoiceId}", invoice.Id);
            return string.Empty;
        }
    }

    public async Task<string> GeneratePdfAsync(Invoice invoice)
    {
        // Implementação da geração do DANFE em PDF
        // Na prática, você usaria uma biblioteca como DanfeSharp ou similar
        await Task.Delay(100); // Simular processamento
        return $"pdf_{invoice.AccessKey}.pdf";
    }

    public async Task<bool> ValidateCertificateAsync()
    {
        try
        {
            if (_certificate == null)
            {
                LoadCertificate();
            }

            if (_certificate == null)
            {
                return false;
            }

            // Verificar se o certificado não expirou
            var isValid = _certificate.NotAfter > DateTime.Now;
            
            if (!isValid)
            {
                _logger.LogWarning("Certificado digital expirado em {ExpiryDate}", _certificate.NotAfter);
            }

            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar certificado digital");
            return false;
        }
    }

    private void LoadCertificate()
    {
        try
        {
            if (File.Exists(_sefazSettings.CertificatePath))
            {
                _certificate = new X509Certificate2(
                    _sefazSettings.CertificatePath, 
                    _sefazSettings.CertificatePassword,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                
                _logger.LogInformation("Certificado digital carregado com sucesso. Válido até: {ExpiryDate}", 
                    _certificate.NotAfter);
            }
            else
            {
                _logger.LogWarning("Arquivo de certificado não encontrado: {CertificatePath}", 
                    _sefazSettings.CertificatePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar certificado digital");
        }
    }

    private string SignXml(string xmlContent)
    {
        // Implementação da assinatura digital
        // Na prática, você usaria System.Security.Cryptography.Xml ou biblioteca especializada
        _logger.LogDebug("Assinando XML digitalmente");
        return xmlContent; // Simplificado para exemplo
    }

    private async Task<SefazResponse> SendToSefazAsync(string xmlContent, string operation)
    {
        try
        {
            var endpoints = _sefazSettings.StateEndpoints.GetValueOrDefault(_sefazSettings.State);
            if (endpoints == null)
            {
                return new SefazResponse
                {
                    Success = false,
                    Message = "Endpoints SEFAZ não configurados para o estado",
                    ErrorCode = "ENDPOINTS_NOT_CONFIGURED"
                };
            }

            var url = operation switch
            {
                "NFeAutorizacao" => endpoints.NFeAutorizacao,
                "NFeConsultaProtocolo" => endpoints.NFeConsultaProtocolo,
                "NFeRecepcaoEvento" => endpoints.NFeRecepcaoEvento,
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(url))
            {
                return new SefazResponse
                {
                    Success = false,
                    Message = $"URL não configurada para operação {operation}",
                    ErrorCode = "URL_NOT_CONFIGURED"
                };
            }

            var content = new StringContent(xmlContent, Encoding.UTF8, "application/soap+xml");
            
            _logger.LogDebug("Enviando requisição para SEFAZ: {Url}", url);
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return ParseSefazResponse(responseContent);
            }
            else
            {
                return new SefazResponse
                {
                    Success = false,
                    Message = $"Erro HTTP: {response.StatusCode}",
                    ErrorCode = "HTTP_ERROR"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comunicar com SEFAZ");
            return new SefazResponse
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = "COMMUNICATION_ERROR"
            };
        }
    }

    private SefazResponse ParseSefazResponse(string xmlResponse)
    {
        // Parse simplificado - na prática seria mais complexo
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Exemplo simplificado de parsing
            var success = xmlResponse.Contains("100"); // Código 100 = Autorizado
            var protocol = ExtractProtocol(xmlResponse);
            var message = success ? "Autorizado o uso da NF-e" : "Rejeitada";

            return new SefazResponse
            {
                Success = success,
                Protocol = protocol,
                Message = message,
                XmlContent = xmlResponse,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer parse da resposta SEFAZ");
            return new SefazResponse
            {
                Success = false,
                Message = "Erro ao processar resposta SEFAZ",
                ErrorCode = "PARSE_ERROR"
            };
        }
    }

    private string ExtractProtocol(string xmlResponse)
    {
        // Extração simplificada do protocolo
        // Na prática seria feita com XPath ou parsing estruturado
        return $"351{DateTime.Now:yyyyMMddHHmmss}000001";
    }

    private string GenerateCancellationXml(string accessKey, string reason)
    {
        // Gerar XML de cancelamento
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<envEvento xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <idLote>1</idLote>
    <evento>
        <infEvento>
            <chNFe>{accessKey}</chNFe>
            <tpAmb>{(_sefazSettings.Environment == "Producao" ? "1" : "2")}</tpAmb>
            <tpEvento>110111</tpEvento>
            <nSeqEvento>1</nSeqEvento>
            <verEvento>1.00</verEvento>
            <detEvento>
                <descEvento>Cancelamento</descEvento>
                <xJust>{reason}</xJust>
            </detEvento>
        </infEvento>
    </evento>
</envEvento>";
    }

    private string GenerateConsultationXml(string accessKey)
    {
        // Gerar XML de consulta
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<consSitNFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <tpAmb>{(_sefazSettings.Environment == "Producao" ? "1" : "2")}</tpAmb>
    <xServ>CONSULTAR</xServ>
    <chNFe>{accessKey}</chNFe>
</consSitNFe>";
    }
}