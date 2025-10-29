using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task<Company?> GetByCnpjAsync(string cnpj);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<IEnumerable<Company>> GetByTypeAsync(CompanyType type);
    Task<IEnumerable<Company>> GetByKycStatusAsync(KycStatus status);
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CnpjExistsAsync(string cnpj);
    Task<bool> CnpjExistsAsync(string cnpj, Guid excludeCompanyId);
}