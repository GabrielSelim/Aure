using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class CompanyRelationshipRepository : ICompanyRelationshipRepository
{
    private readonly AureDbContext _context;

    public CompanyRelationshipRepository(AureDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyRelationship?> GetByIdAsync(Guid id)
    {
        return await _context.CompanyRelationships
            .Where(r => !r.IsDeleted && r.Id == id)
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CompanyRelationship>> GetAllAsync()
    {
        return await _context.CompanyRelationships
            .Where(r => !r.IsDeleted)
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(CompanyRelationship entity)
    {
        await _context.CompanyRelationships.AddAsync(entity);
    }

    public async Task UpdateAsync(CompanyRelationship entity)
    {
        _context.CompanyRelationships.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.MarkAsDeleted();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.CompanyRelationships
            .AnyAsync(r => !r.IsDeleted && r.Id == id);
    }

    public async Task<IEnumerable<CompanyRelationship>> GetByClientCompanyIdAsync(Guid clientCompanyId)
    {
        return await _context.CompanyRelationships
            .Where(r => !r.IsDeleted && r.ClientCompanyId == clientCompanyId)
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CompanyRelationship>> GetByProviderCompanyIdAsync(Guid providerCompanyId)
    {
        return await _context.CompanyRelationships
            .Where(r => !r.IsDeleted && r.ProviderCompanyId == providerCompanyId)
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CompanyRelationship>> GetActiveRelationshipsByCompanyIdAsync(Guid companyId)
    {
        return await _context.CompanyRelationships
            .Where(r => !r.IsDeleted && 
                       r.Status == RelationshipStatus.Active &&
                       (r.ClientCompanyId == companyId || r.ProviderCompanyId == companyId))
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CompanyRelationship>> GetRelationshipsByCompanyIdAndStatusAsync(Guid companyId, RelationshipStatus? status = null)
    {
        var query = _context.CompanyRelationships
            .Where(r => !r.IsDeleted && 
                       (r.ClientCompanyId == companyId || r.ProviderCompanyId == companyId));

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<CompanyRelationship?> GetRelationshipAsync(Guid clientCompanyId, Guid providerCompanyId, RelationshipType type)
    {
        return await _context.CompanyRelationships
            .Where(r => !r.IsDeleted && 
                       r.ClientCompanyId == clientCompanyId && 
                       r.ProviderCompanyId == providerCompanyId &&
                       r.Type == type)
            .Include(r => r.ClientCompany)
            .Include(r => r.ProviderCompany)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasActiveRelationshipAsync(Guid clientCompanyId, Guid providerCompanyId, RelationshipType type)
    {
        return await _context.CompanyRelationships
            .AnyAsync(r => !r.IsDeleted && 
                          r.Status == RelationshipStatus.Active &&
                          r.ClientCompanyId == clientCompanyId && 
                          r.ProviderCompanyId == providerCompanyId &&
                          r.Type == type);
    }
}