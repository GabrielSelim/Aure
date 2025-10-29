namespace Aure.Application.DTOs.User;

public class UserDataExportResponse
{
    public DadosPessoais DadosPessoais { get; set; } = new();
    public DadosEmpresa? DadosEmpresa { get; set; }
    public List<ContratoInfo> Contratos { get; set; } = new();
    public List<PagamentoInfo> Pagamentos { get; set; } = new();
    public NotificationPreferencesDTO PreferenciasNotificacao { get; set; } = new();
    public DateTime DataExportacao { get; set; }
}

public class DadosPessoais
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? TelefoneCelular { get; set; }
    public string? TelefoneFixo { get; set; }
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Cargo { get; set; }
    public EnderecoInfo? Endereco { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool AceitouTermosUso { get; set; }
    public DateTime? DataAceiteTermosUso { get; set; }
    public bool AceitouPoliticaPrivacidade { get; set; }
    public DateTime? DataAceitePoliticaPrivacidade { get; set; }
}

public class EnderecoInfo
{
    public string? Rua { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Pais { get; set; }
    public string? Cep { get; set; }
}

public class DadosEmpresa
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

public class ContratoInfo
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal? ValorMensal { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataExpiracao { get; set; }
    public DateTime? DataAssinatura { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PagamentoInfo
{
    public Guid Id { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public class AccountDeletionResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? Aviso { get; set; }
}
