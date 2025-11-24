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
    
    // Employee listing
    Task<PagedResult<EmployeeListItemResponse>> GetEmployeesAsync(Guid requestingUserId, EmployeeListRequest request);
    
    // LGPD
    Task<UserDataExportResponse> ExportUserDataAsync(Guid userId);
    Task<AccountDeletionResponse> RequestAccountDeletionAsync(Guid userId);
    
    // Invitation Management
    Task<Result<IEnumerable<UserInvitationListResponse>>> GetInvitationsAsync(Guid requestingUserId);
    Task<Result<UserInvitationListResponse>> GetInvitationByIdAsync(Guid invitationId, Guid requestingUserId);
    Task<Result<UpdateInvitationResponse>> UpdateInvitationAsync(Guid invitationId, UpdateInvitationRequest request, Guid requestingUserId);
    Task<Result<CancelInvitationResponse>> CancelInvitationAsync(Guid invitationId, Guid requestingUserId);
    Task<Result<ResendInvitationResponse>> ResendInvitationAsync(Guid invitationId, Guid requestingUserId);
    
    // Password Recovery
    Task<Result<bool>> RequestPasswordResetAsync(string email);
    Task<Result<PasswordResetResponse>> ResetPasswordAsync(ResetPasswordRequest request);
    
    // Position Management
    Task<Result<UserResponse>> UpdateEmployeePositionAsync(Guid employeeId, string newPosition, Guid requestingUserId);
    
    // Funcionários para Contratos
    Task<Result<IEnumerable<FuncionarioInternoResponse>>> GetFuncionariosInternosAsync(Guid requestingUserId);
    Task<Result<IEnumerable<FuncionarioPJResponse>>> GetFuncionariosPJAsync(Guid requestingUserId);
}