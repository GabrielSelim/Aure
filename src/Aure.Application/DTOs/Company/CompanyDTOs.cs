using Aure.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Aure.Application.DTOs.Company;

public record CreateCompanyRequest(
    string Name,
    string Cnpj,
    CompanyType Type,
    BusinessModel BusinessModel = BusinessModel.Standard
);

public record UpdateCompanyRequest(
    string Name,
    CompanyType Type,
    BusinessModel BusinessModel
);

public record CompanyResponse(
    Guid Id,
    string Name,
    string Cnpj,
    CompanyType Type,
    BusinessModel BusinessModel,
    KycStatus KycStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CompanyListResponse(
    IEnumerable<CompanyResponse> Companies,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public class UserCompanyInfoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string CnpjFormatado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string ModeloNegocio { get; set; } = string.Empty;
    public string? Rua { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Pais { get; set; }
    public string? Cep { get; set; }
    public string? EnderecoCompleto { get; set; }
    public EnderecoEmpresaDto? Endereco { get; set; }
    public string? TelefoneFixo { get; set; }
    public string? TelefoneCelular { get; set; }
}

public class EnderecoEmpresaDto
{
    public string Rua { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string EnderecoCompleto { get; set; } = string.Empty;
}

public record UpdateUserCompanyInfoRequest(
    [Required(ErrorMessage = "Nome da empresa é obrigatório")]
    [Description("Nome da empresa")]
    string Nome,

    [Required(ErrorMessage = "Telefone celular é obrigatório")]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone celular deve conter 10 ou 11 dígitos")]
    [Description("Telefone celular (apenas números, com DDD)")]
    string TelefoneCelular,

    [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefone fixo deve conter 10 dígitos")]
    [Description("Telefone fixo (apenas números, com DDD) - opcional")]
    string? TelefoneFixo,

    [Required(ErrorMessage = "Rua é obrigatória")]
    [Description("Rua")]
    string Rua,

    [Required(ErrorMessage = "Número é obrigatório")]
    [Description("Número")]
    string Numero,

    [Description("Complemento - opcional")]
    string? Complemento,

    [Required(ErrorMessage = "Bairro é obrigatório")]
    [Description("Bairro")]
    string Bairro,

    [Required(ErrorMessage = "Cidade é obrigatória")]
    [Description("Cidade")]
    string Cidade,

    [Required(ErrorMessage = "Estado é obrigatório")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
    [Description("Estado (sigla com 2 letras)")]
    string Estado,

    [Required(ErrorMessage = "País é obrigatório")]
    [Description("País")]
    string Pais,

    [Required(ErrorMessage = "CEP é obrigatório")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "CEP deve conter 8 dígitos")]
    [Description("CEP (apenas números)")]
    string Cep
);