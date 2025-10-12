using Aure.Domain.Enums;

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
    string Email,
    string Password
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User
);

public record RegisterCompanyAdminRequest(
    string CompanyName,
    string CompanyCnpj,
    CompanyType CompanyType,
    BusinessModel BusinessModel,
    string Name,
    string Email,
    string Password
);

public record InviteUserRequest(
    string Name,
    string Email,
    UserRole? Role,                    // Opcional - será Provider para ContractedPJ
    InviteType InviteType,
    string? CompanyName = null,         // Para PJ contratado
    string? Cnpj = null,               // Para PJ contratado
    CompanyType? CompanyType = null,   // Para PJ contratado
    BusinessModel? BusinessModel = null // Para PJ contratado
);

public record AcceptInviteRequest(
    string Password
);

public record InviteResponse(
    Guid InviteId,
    string Message,
    string InviteToken,
    DateTime ExpiresAt,
    InviteType InviteType
);

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