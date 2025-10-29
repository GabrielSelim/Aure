using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class UserInvitationRepository : BaseRepository<UserInvitation>, IUserInvitationRepository
{
    public UserInvitationRepository(AureDbContext context) : base(context) { }

    public async Task<UserInvitation?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(x => x.Company)
            .Include(x => x.InvitedByUser)
            .Include(x => x.AcceptedByUser)
            .FirstOrDefaultAsync(x => x.InvitationToken == token);
    }

    public async Task<UserInvitation?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(x => x.Company)
            .Include(x => x.InvitedByUser)
            .Where(x => x.Email.ToLower() == email.ToLower())
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _dbSet
            .Include(x => x.InvitedByUser)
            .Include(x => x.AcceptedByUser)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingByCompanyIdAsync(Guid companyId)
    {
        return await _dbSet
            .Include(x => x.InvitedByUser)
            .Where(x => x.CompanyId == companyId && x.Status == InvitationStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetByStatusAsync(InvitationStatus status)
    {
        return await _dbSet
            .Include(x => x.Company)
            .Include(x => x.InvitedByUser)
            .Include(x => x.AcceptedByUser)
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetExpiredAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(x => x.Status == InvitationStatus.Pending && x.ExpiresAt < now)
            .ToListAsync();
    }

    public async Task<bool> EmailHasPendingInvitationAsync(string email, Guid companyId)
    {
        return await _dbSet
            .AnyAsync(x => x.Email.ToLower() == email.ToLower() 
                        && x.CompanyId == companyId 
                        && x.Status == InvitationStatus.Pending);
    }

    public override async Task<UserInvitation?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(x => x.Company)
            .Include(x => x.InvitedByUser)
            .Include(x => x.AcceptedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
