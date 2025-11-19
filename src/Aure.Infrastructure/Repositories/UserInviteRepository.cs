using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class UserInviteRepository : BaseRepository<UserInvite>, IUserInviteRepository
{
    public UserInviteRepository(AureDbContext context) : base(context)
    {
    }

    public async Task<UserInvite?> GetByTokenAsync(string token)
    {
        return await _context.UserInvites
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .FirstOrDefaultAsync(ui => ui.Token == token && !ui.IsDeleted);
    }

    public async Task<IEnumerable<UserInvite>> GetPendingByCompanyAsync(Guid companyId)
    {
        var now = DateTime.UtcNow;
        return await _context.UserInvites
            .Include(ui => ui.InvitedByUser)
            .Where(ui => ui.CompanyId == companyId && 
                        !ui.IsAccepted && 
                        ui.ExpiresAt > now && 
                        !ui.IsDeleted)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvite>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.UserInvites
            .Include(ui => ui.InvitedByUser)
            .Where(ui => ui.CompanyId == companyId && !ui.IsDeleted)
            .OrderByDescending(ui => ui.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> EmailHasPendingInviteAsync(string email)
    {
        var now = DateTime.UtcNow;
        return await _context.UserInvites
            .AnyAsync(ui => ui.InviteeEmail.ToLower() == email.ToLower() && 
                           !ui.IsAccepted && 
                           ui.ExpiresAt > now && 
                           !ui.IsDeleted);
    }

    public async Task<UserInvite?> GetPendingInviteByEmailAsync(string email)
    {
        var now = DateTime.UtcNow;
        return await _context.UserInvites
            .Include(ui => ui.Company)
            .Include(ui => ui.InvitedByUser)
            .FirstOrDefaultAsync(ui => ui.InviteeEmail.ToLower() == email.ToLower() && 
                                      !ui.IsAccepted && 
                                      ui.ExpiresAt > now && 
                                      !ui.IsDeleted);
    }

    public async Task ExpireInvitesAsync(IEnumerable<Guid> inviteIds)
    {
        var invites = await _context.UserInvites
            .Where(ui => inviteIds.Contains(ui.Id))
            .ToListAsync();

        foreach (var invite in invites)
        {
            invite.MarkAsDeleted(); // Marca como deletado ao invés de expirar
        }

        await _context.SaveChangesAsync();
    }
}