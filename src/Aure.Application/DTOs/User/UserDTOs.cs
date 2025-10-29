using Aure.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Aure.Application.DTOs.User;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    UserRole Role,
    Guid CompanyId
);

public record UpdateUserRequest(
    string Name,
    string Email
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    Guid? CompanyId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record LoginRequest(
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    string Email,
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    string Password
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User
);

/// <summary>
/// Registro do primeiro usuário da empresa (Dono da Empresa Pai)
/// </summary>
public record RegisterCompanyAdminRequest(
    [Required(ErrorMessage = "Nome da empresa é obrigatório")]
    [Description("Nome fantasia ou razão social da empresa")]
    string CompanyName,
    
    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter 14 dígitos")]
    [Description("CNPJ da empresa (apenas números)")]
    string CompanyCnpj,
    
    [Required]
    [Description("Tipo da empresa. Valores aceitos: Client (empresa cliente)")]
    CompanyType CompanyType,
    
    [Required]
    [Description("Modelo de negócio. Valores aceitos: Direct (empresa direta/principal)")]
    BusinessModel BusinessModel,
    
    [Required(ErrorMessage = "Nome do usuário é obrigatório")]
    [Description("Nome completo do responsável pela empresa")]
    string Name,
    
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Description("Email que será usado para login")]
    string Email,
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    [Description("Senha de acesso (mínimo 6 caracteres)")]
    string Password
);

/// <summary>
/// Convite para usuário interno (Financeiro/Jurídico) ou funcionário PJ
/// </summary>
public record InviteUserRequest(
    [Required(ErrorMessage = "Nome é obrigatório")]
    [Description("Nome completo do usuário")]
    string Name,
    
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Description("Email para envio do convite")]
    string Email,
    
    [Description(@"Role do usuário. 
    - Para InviteType=Internal: Financeiro ou Juridico
    - Para InviteType=ContractedPJ: FuncionarioPJ (automático)
    Valores: DonoEmpresaPai, Financeiro, Juridico, FuncionarioPJ")]
    UserRole? Role,
    
    [Required]
    [Description(@"Tipo de convite:
    - Internal: Usuário interno da empresa (Financeiro/Jurídico)
    - ContractedPJ: Funcionário PJ que prestará serviços")]
    InviteType InviteType,
    
    [Description("Cargo do usuário (ex: Desenvolvedor, Designer, Gerente)")]
    string? Cargo = null,
    
    [Description("Nome da empresa PJ (obrigatório para ContractedPJ)")]
    string? CompanyName = null,
    
    [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter 14 dígitos")]
    [Description("CNPJ da empresa PJ - apenas números (obrigatório para ContractedPJ)")]
    string? Cnpj = null,
    
    [Description("Tipo da empresa PJ. Para PJ sempre usar: Provider")]
    CompanyType? CompanyType = null,
    
    [Description("Modelo de negócio da PJ. Para PJ sempre usar: ContractedPJ")]
    BusinessModel? BusinessModel = null
);

public record AcceptInviteRequest(
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    [Description("Senha para acesso ao sistema")]
    string Password
);

public class InviteResponse
{
    public Guid Id { get; set; }
    public string InviterName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string InviteeName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid CompanyId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public InviteType InviteType { get; set; }
    public BusinessModel? BusinessModel { get; set; }
    public string? CompanyName { get; set; }
    public string? Cnpj { get; set; }
    public CompanyType? CompanyType { get; set; }
}

public record UserInviteResponse(
    Guid Id,
    string InviterName,
    string InviteeEmail,
    string InviteeName,
    UserRole Role,
    InviteType InviteType,
    string? CompanyName,
    string? Cnpj,
    CompanyType? CompanyType,
    BusinessModel? BusinessModel,
    string Token,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    bool IsExpired
);