using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class User : BaseEntity
{
    // Identificação
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Company? Company { get; private set; }

    // Perfil
    public string? AvatarUrl { get; private set; }
    public DateTime? DataNascimento { get; private set; }
    public string? Cargo { get; private set; }

    // Documentos (Criptografados)
    public string? CPFEncrypted { get; private set; }
    public string? RGEncrypted { get; private set; }

    // Contatos
    public string? TelefoneCelular { get; private set; }
    public string? TelefoneFixo { get; private set; }

    // Endereço
    public string? EnderecoRua { get; private set; }
    public string? EnderecoNumero { get; private set; }
    public string? EnderecoComplemento { get; private set; }
    public string? EnderecoBairro { get; private set; }
    public string? EnderecoCidade { get; private set; }
    public string? EnderecoEstado { get; private set; }
    public string? EnderecoPais { get; private set; }
    public string? EnderecoCep { get; private set; }

    // Termos (Separados)
    public bool AceitouTermosUso { get; private set; }
    public DateTime? DataAceiteTermosUso { get; private set; }
    public string? VersaoTermosUsoAceita { get; private set; }
    
    public bool AceitouPoliticaPrivacidade { get; private set; }
    public DateTime? DataAceitePoliticaPrivacidade { get; private set; }
    public string? VersaoPoliticaPrivacidadeAceita { get; private set; }

    // Preferências de Notificação
    public NotificationPreferences? NotificationPreferences { get; private set; }

    private readonly List<Session> _sessions = new();
    private readonly List<Signature> _signatures = new();
    private readonly List<AuditLog> _auditLogs = new();

    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    public IReadOnlyCollection<Signature> Signatures => _signatures.AsReadOnly();
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    private User() { }

    public User(string name, string email, string passwordHash, UserRole role, Guid? companyId = null)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CompanyId = companyId;
    }

    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;
        UpdateTimestamp();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdateTimestamp();
    }

    public void AddSession(Session session)
    {
        _sessions.Add(session);
    }

    public void AddSignature(Signature signature)
    {
        _signatures.Add(signature);
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        _auditLogs.Add(auditLog);
    }

    public void UpdateProfile(
        string? name = null,
        string? email = null,
        string? cargo = null,
        string? telefoneCelular = null,
        string? telefoneFixo = null,
        DateTime? dataNascimento = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;
            
        if (!string.IsNullOrWhiteSpace(email))
            Email = email;
            
        if (cargo != null)
            Cargo = cargo;
            
        if (telefoneCelular != null)
            TelefoneCelular = telefoneCelular;
            
        if (telefoneFixo != null)
            TelefoneFixo = telefoneFixo;
            
        if (dataNascimento.HasValue)
            DataNascimento = dataNascimento;

        UpdateTimestamp();
    }

    public void UpdateAddress(
        string? rua,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? estado,
        string? pais,
        string? cep)
    {
        EnderecoRua = rua;
        EnderecoNumero = numero;
        EnderecoComplemento = complemento;
        EnderecoBairro = bairro;
        EnderecoCidade = cidade;
        EnderecoEstado = estado;
        EnderecoPais = pais;
        EnderecoCep = cep;
        UpdateTimestamp();
    }

    public void UpdateDocuments(string? cpfEncrypted, string? rgEncrypted)
    {
        if (cpfEncrypted != null)
            CPFEncrypted = cpfEncrypted;
            
        if (rgEncrypted != null)
            RGEncrypted = rgEncrypted;
            
        UpdateTimestamp();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdateTimestamp();
    }

    public void AcceptTermsOfUse(string version)
    {
        AceitouTermosUso = true;
        DataAceiteTermosUso = DateTime.UtcNow;
        VersaoTermosUsoAceita = version;
        UpdateTimestamp();
    }

    public void AcceptPrivacyPolicy(string version)
    {
        AceitouPoliticaPrivacidade = true;
        DataAceitePoliticaPrivacidade = DateTime.UtcNow;
        VersaoPoliticaPrivacidadeAceita = version;
        UpdateTimestamp();
    }

    public void SetNotificationPreferences(NotificationPreferences preferences)
    {
        NotificationPreferences = preferences;
    }

    public string GetEnderecoCompleto()
    {
        if (string.IsNullOrWhiteSpace(EnderecoRua))
            return string.Empty;

        var partes = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(EnderecoRua))
            partes.Add(EnderecoRua);
            
        if (!string.IsNullOrWhiteSpace(EnderecoNumero))
            partes.Add(EnderecoNumero);
            
        if (!string.IsNullOrWhiteSpace(EnderecoComplemento))
            partes.Add(EnderecoComplemento);
            
        if (!string.IsNullOrWhiteSpace(EnderecoBairro))
            partes.Add(EnderecoBairro);
            
        if (!string.IsNullOrWhiteSpace(EnderecoCidade))
            partes.Add(EnderecoCidade);
            
        if (!string.IsNullOrWhiteSpace(EnderecoEstado))
            partes.Add(EnderecoEstado);
            
        if (!string.IsNullOrWhiteSpace(EnderecoCep))
            partes.Add($"CEP {EnderecoCep}");

        return string.Join(", ", partes);
    }

    public void AnonymizeForDeletion(string anonymizedName, string anonymizedEmail)
    {
        Name = anonymizedName;
        Email = anonymizedEmail;
        TelefoneCelular = null;
        TelefoneFixo = null;
        EnderecoRua = null;
        EnderecoNumero = null;
        EnderecoComplemento = null;
        EnderecoBairro = null;
        EnderecoCidade = null;
        EnderecoEstado = null;
        EnderecoPais = null;
        EnderecoCep = null;
        AvatarUrl = null;
        Cargo = null;
        MarkAsDeleted();
        UpdateTimestamp();
    }
}