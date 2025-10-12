using Aure.Domain.Entities;

namespace Aure.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string? ValidateToken(string token);
}