using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id);
    Task<IEnumerable<Contract>> GetAllAsync();
    Task<IEnumerable<Contract>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<Contract>> GetByProviderIdAsync(Guid providerId);
    Task<IEnumerable<Contract>> GetByStatusAsync(ContractStatus status);
    Task<IEnumerable<Contract>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(Contract contract);
    Task UpdateAsync(Contract contract);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<decimal> GetTotalValueByCompanyAsync(Guid companyId);
}