using Aure.Application.DTOs.User;

namespace Aure.Application.DTOs.Auth;

public record LoginWithUserResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserResponse User,
    Domain.Entities.User UserEntity
);