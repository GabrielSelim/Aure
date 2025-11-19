using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface IUserInviteRepository : IBaseRepository<UserInvite>
{
    Task<UserInvite?> GetByTokenAsync(string token);
    Task<IEnumerable<UserInvite>> GetPendingByCompanyAsync(Guid companyId);
    Task<IEnumerable<UserInvite>> GetByCompanyIdAsync(Guid companyId);
    Task<bool> EmailHasPendingInviteAsync(string email);
    Task<UserInvite?> GetPendingInviteByEmailAsync(string email);
    Task ExpireInvitesAsync(IEnumerable<Guid> inviteIds);
}