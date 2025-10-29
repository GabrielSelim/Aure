using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface IUserInvitationRepository
{
    Task<UserInvitation?> GetByIdAsync(Guid id);
    Task<UserInvitation?> GetByTokenAsync(string token);
    Task<UserInvitation?> GetByEmailAsync(string email);
    Task<IEnumerable<UserInvitation>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<UserInvitation>> GetPendingByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<UserInvitation>> GetByStatusAsync(InvitationStatus status);
    Task<IEnumerable<UserInvitation>> GetExpiredAsync();
    Task<bool> EmailHasPendingInvitationAsync(string email, Guid companyId);
    Task AddAsync(UserInvitation invitation);
    Task UpdateAsync(UserInvitation invitation);
    Task DeleteAsync(Guid id);
}
