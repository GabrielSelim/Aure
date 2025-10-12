using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class ContractRepository : BaseRepository<Contract>, IContractRepository
{
    public ContractRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<Contract>> GetByClientIdAsync(Guid clientId)
    {
        return await _dbSet
            .Where(x => x.ClientId == clientId)
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetByProviderIdAsync(Guid providerId)
    {
        return await _dbSet
            .Where(x => x.ProviderId == providerId)
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetByStatusAsync(ContractStatus status)
    {
        return await _dbSet
            .Where(x => x.Status == status)
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contract>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalValueByCompanyAsync(Guid companyId)
    {
        return await _dbSet
            .Where(x => (x.ClientId == companyId || x.ProviderId == companyId) && 
                       x.Status == ContractStatus.Active)
            .SumAsync(x => x.ValueTotal);
    }

    public override async Task<Contract?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .Include(x => x.Signatures)
            .Include(x => x.TokenizedAsset)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<IEnumerable<Contract>> GetAllAsync()
    {
        return await _dbSet
            .Include(x => x.Client)
            .Include(x => x.Provider)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}