using Aure.Domain.Entities;

namespace Aure.Application.Interfaces;

public interface ISefazService
{
    Task<SefazResponse> IssueInvoiceAsync(Invoice invoice);
    Task<SefazResponse> CancelInvoiceAsync(string accessKey, string reason);
    Task<SefazStatusResponse> CheckStatusAsync(string accessKey);
    Task<string> GenerateXmlAsync(Invoice invoice);
    Task<string> GeneratePdfAsync(Invoice invoice);
    Task<bool> ValidateCertificateAsync();
}

public class SefazResponse
{
    public bool Success { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

public class SefazStatusResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
}