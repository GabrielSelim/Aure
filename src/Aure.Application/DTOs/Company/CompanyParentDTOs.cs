using Aure.Domain.Enums;

namespace Aure.Application.DTOs.Company;

public class CompanyInfoResponse
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public CompanyType CompanyType { get; set; }
    public BusinessModel BusinessModel { get; set; }
    public string? EnderecoCompleto { get; set; }
    public int TotalFuncionarios { get; set; }
    public int ContratosAtivos { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class UpdateCompanyParentRequest
{
    public string? RazaoSocial { get; set; }
    public string? Cnpj { get; set; }
    public bool ConfirmarDivergenciaRazaoSocial { get; set; } = false;
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoComplemento { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoEstado { get; set; }
    public string? EnderecoPais { get; set; }
    public string? EnderecoCep { get; set; }
}

public class UpdateCompanyParentResponse
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public CompanyInfoResponse? Empresa { get; set; }
    public bool DivergenciaRazaoSocial { get; set; }
    public string? RazaoSocialReceita { get; set; }
    public string? RazaoSocialInformada { get; set; }
    public bool RequerConfirmacao { get; set; }
}
