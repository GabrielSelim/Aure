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
    string Name,
    string Email,
    string Password
);

public record InviteUserRequest(
    string Name,
    string Email,
    UserRole Role,
    Guid? CompanyId = null
);

public record AcceptInviteRequest(
    string Password
);

public record InviteResponse(
    Guid InviteId,
    string Message
);