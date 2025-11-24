namespace Aure.Application.DTOs.User;

public class UpdateFullProfileRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public string? OrgaoExpedidorRG { get; set; }
    public string? Nacionalidade { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Cargo { get; set; }
    
    public string? TelefoneCelular { get; set; }
    public string? TelefoneFixo { get; set; }
    
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoComplemento { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoEstado { get; set; }
    public string? EnderecoPais { get; set; }
    public string? EnderecoCep { get; set; }
    
    public string? SenhaAtual { get; set; }
    public string? NovaSenha { get; set; }
}

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleDescricao { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? CPFMascarado { get; set; }
    public string? CPF { get; set; }
    public string? RG { get; set; }
    public string? OrgaoExpedidorRG { get; set; }
    public string? Nacionalidade { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Cargo { get; set; }
    public string? TelefoneCelular { get; set; }
    public string? TelefoneFixo { get; set; }
    
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoComplemento { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoEstado { get; set; }
    public string? EnderecoPais { get; set; }
    public string? EnderecoCep { get; set; }
    public string? EnderecoCompleto { get; set; }
    
    public bool AceitouTermosUso { get; set; }
    public DateTime? DataAceiteTermosUso { get; set; }
    public string? VersaoTermosUsoAceita { get; set; }
    
    public bool AceitouPoliticaPrivacidade { get; set; }
    public DateTime? DataAceitePoliticaPrivacidade { get; set; }
    public string? VersaoPoliticaPrivacidadeAceita { get; set; }
}

public class NotificationPreferencesDTO
{
    public bool ReceberEmailNovoContrato { get; set; }
    public bool ReceberEmailContratoAssinado { get; set; }
    public bool ReceberEmailContratoVencendo { get; set; }
    public bool ReceberEmailPagamentoProcessado { get; set; }
    public bool ReceberEmailPagamentoRecebido { get; set; }
    public bool ReceberEmailNovoFuncionario { get; set; }
    public bool ReceberEmailAlertasFinanceiros { get; set; }
    public bool ReceberEmailAtualizacoesSistema { get; set; }
}

public class AcceptTermsRequest
{
    public string VersaoTermosUso { get; set; } = string.Empty;
    public string VersaoPoliticaPrivacidade { get; set; } = string.Empty;
    public bool AceitouTermosUso { get; set; }
    public bool AceitouPoliticaPrivacidade { get; set; }
}

public class TermsVersionsResponse
{
    public string VersaoTermosUsoAtual { get; set; } = "1.0.0";
    public string VersaoPoliticaPrivacidadeAtual { get; set; } = "1.0.0";
    public bool UsuarioPrecisaAceitar { get; set; }
}
