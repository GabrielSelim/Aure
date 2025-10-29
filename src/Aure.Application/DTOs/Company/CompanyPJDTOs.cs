using Aure.Domain.Enums;

namespace Aure.Application.DTOs.Company;

public class UpdateCompanyPJRequest
{
    public string? RazaoSocial { get; set; }
    public string? Cnpj { get; set; }
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoComplemento { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoEstado { get; set; }
    public string? EnderecoPais { get; set; }
    public string? EnderecoCep { get; set; }
    public CompanyType? CompanyType { get; set; }
    public bool ConfirmarDivergenciaRazaoSocial { get; set; } = false;
}

public class UpdateCompanyPJResponse
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public CompanyPJData? Empresa { get; set; }
    public bool DivergenciaRazaoSocial { get; set; }
    public string? RazaoSocialReceita { get; set; }
    public string? RazaoSocialInformada { get; set; }
    public bool RequerConfirmacao { get; set; }
}

public class CompanyPJData
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoComplemento { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoEstado { get; set; }
    public string? EnderecoPais { get; set; }
    public string? EnderecoCep { get; set; }
    public CompanyType CompanyType { get; set; }
}
