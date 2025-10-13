using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class KycRecordRepository : BaseRepository<KycRecord>, IKycRecordRepository
{
    public KycRecordRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<KycRecord>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.KycRecords
            .Where(k => !k.IsDeleted && k.CompanyId == companyId)
            .Include(k => k.Company)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task<KycRecord?> GetByDocumentHashAsync(string documentHash)
    {
        return await _context.KycRecords
            .Where(k => !k.IsDeleted && k.DocumentHash == documentHash)
            .Include(k => k.Company)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DocumentHashExistsAsync(string documentHash)
    {
        return await _context.KycRecords
            .AnyAsync(k => !k.IsDeleted && k.DocumentHash == documentHash);
    }
}