namespace Aure.Infrastructure.Configuration;

public class SefazSettings
{
    public string Environment { get; set; } = "Homologacao"; // Homologacao ou Producao
    public string State { get; set; } = "SP"; // Estado do emitente
    public string CertificatePath { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
    public int WebServiceTimeout { get; set; } = 30000;
    public string ProxyUrl { get; set; } = string.Empty;
    public string ProxyUser { get; set; } = string.Empty;
    public string ProxyPassword { get; set; } = string.Empty;
    
    // URLs dos webservices por estado (será carregado automaticamente)
    public Dictionary<string, SefazEndpoints> StateEndpoints { get; set; } = new();
}

public class SefazEndpoints
{
    public string NFeAutorizacao { get; set; } = string.Empty;
    public string NFeRetAutorizacao { get; set; } = string.Empty;
    public string NFeConsultaProtocolo { get; set; } = string.Empty;
    public string NFeStatusServico { get; set; } = string.Empty;
    public string NFeRecepcaoEvento { get; set; } = string.Empty;
    public string NFeDistribuicaoDFe { get; set; } = string.Empty;
}

public class InvoiceSettings
{
    public string DefaultSeries { get; set; } = "1";
    public string InvoiceNumberSequence { get; set; } = "auto";
    public string DefaultCfop { get; set; } = "5102"; // Venda de mercadoria
    public string CompanyLogo { get; set; } = "logo.png";
    public int XmlRetentionDays { get; set; } = 1825; // 5 anos
    public bool EnableContingencyMode { get; set; } = true;
    public string DefaultNatureOperation { get; set; } = "Prestação de serviços";
}