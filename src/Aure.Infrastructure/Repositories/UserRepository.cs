using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AureDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _dbSet
            .Where(x => x.Role == role)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(x => x.Email.ToLower() == email.ToLower())
            .ConfigureAwait(false);
    }

    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _dbSet
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}