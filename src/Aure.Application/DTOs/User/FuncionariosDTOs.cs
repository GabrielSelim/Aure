using Aure.Domain.Enums;

namespace Aure.Application.DTOs.User;

public class FuncionarioInternoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Rg { get; set; }
    public string? OrgaoExpedidorRG { get; set; }
    public string? Nacionalidade { get; set; }
    public string? EstadoCivil { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? TelefoneCelular { get; set; }
    public string? TelefoneFixo { get; set; }
    public EnderecoFuncionarioDto? Endereco { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class FuncionarioPJResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Rg { get; set; }
    public string? OrgaoExpedidorRG { get; set; }
    public string? Nacionalidade { get; set; }
    public string? EstadoCivil { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? TelefoneCelular { get; set; }
    public string? TelefoneFixo { get; set; }
    public EnderecoFuncionarioDto? Endereco { get; set; }
    public EmpresaPJDto? EmpresaPJ { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class EnderecoFuncionarioDto
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

public class EmpresaPJDto
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string CnpjFormatado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string ModeloNegocio { get; set; } = string.Empty;
}
