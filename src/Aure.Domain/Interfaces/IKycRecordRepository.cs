using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface IKycRecordRepository
{
    Task<KycRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<KycRecord>> GetAllAsync();
    Task<IEnumerable<KycRecord>> GetByCompanyIdAsync(Guid companyId);
    Task<KycRecord?> GetByDocumentHashAsync(string documentHash);
    Task AddAsync(KycRecord entity);
    Task UpdateAsync(KycRecord entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> DocumentHashExistsAsync(string documentHash);
}