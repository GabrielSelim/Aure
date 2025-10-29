using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(AureDbContext context) : base(context) { }

    public async Task<Company?> GetByCnpjAsync(string cnpj)
    {
        var cleanCnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Cnpj == cleanCnpj);
    }

    public async Task<IEnumerable<Company>> GetByTypeAsync(CompanyType type)
    {
        return await _dbSet
            .Where(x => x.Type == type)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Company>> GetByKycStatusAsync(KycStatus status)
    {
        return await _dbSet
            .Where(x => x.KycStatus == status)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<bool> CnpjExistsAsync(string cnpj)
    {
        var cleanCnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
        return await _dbSet
            .AnyAsync(x => x.Cnpj == cleanCnpj)
            .ConfigureAwait(false);
    }

    public async Task<bool> CnpjExistsAsync(string cnpj, Guid excludeCompanyId)
    {
        var cleanCnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
        return await _dbSet
            .AnyAsync(x => x.Cnpj == cleanCnpj && x.Id != excludeCompanyId)
            .ConfigureAwait(false);
    }

    public override async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}