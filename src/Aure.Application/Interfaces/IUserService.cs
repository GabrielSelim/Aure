using Aure.Application.DTOs.User;
using Aure.Application.DTOs.Auth;
using Aure.Domain.Common;

namespace Aure.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid id);
    Task<Result<UserResponse>> GetByEmailAsync(string email);
    Task<Result<IEnumerable<UserResponse>>> GetAllAsync();
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request);
    Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<Result> ChangePasswordAsync(Guid id, ChangePasswordRequest request);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<LoginWithUserResponse>> LoginAsync(LoginRequest request);
    Task<Result> LogoutAsync(Guid userId);
    
    // Registration methods
    Task<Result<UserResponse>> RegisterCompanyAdminAsync(RegisterCompanyAdminRequest request);
    Task<Result<InviteResponse>> InviteUserAsync(InviteUserRequest request, Guid currentUserId, string currentUserRole);
    Task<Result<UserResponse>> AcceptInviteAsync(string inviteToken, AcceptInviteRequest request);
    Task<Result<InviteResponse>> ResendInviteEmailAsync(Guid inviteId, Guid currentUserId);
    
    // Company-filtered methods
    Task<Result<IEnumerable<UserResponse>>> GetAllByCompanyAsync(Guid companyId);
    Task<Result<UserResponse>> GetByIdAndCompanyAsync(Guid id, Guid companyId);
    Task<Result<UserResponse>> GetByEmailAndCompanyAsync(string email, Guid companyId);
}