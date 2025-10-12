using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ICompanyRelationshipRepository
{
    Task<CompanyRelationship?> GetByIdAsync(Guid id);
    Task<IEnumerable<CompanyRelationship>> GetAllAsync();
    Task AddAsync(CompanyRelationship entity);
    Task UpdateAsync(CompanyRelationship entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    
    Task<IEnumerable<CompanyRelationship>> GetByClientCompanyIdAsync(Guid clientCompanyId);
    Task<IEnumerable<CompanyRelationship>> GetByProviderCompanyIdAsync(Guid providerCompanyId);
    Task<IEnumerable<CompanyRelationship>> GetActiveRelationshipsByCompanyIdAsync(Guid companyId);
    Task<CompanyRelationship?> GetRelationshipAsync(Guid clientCompanyId, Guid providerCompanyId, RelationshipType type);
    Task<bool> HasActiveRelationshipAsync(Guid clientCompanyId, Guid providerCompanyId, RelationshipType type);
}