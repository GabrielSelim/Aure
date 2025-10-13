using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface ISignatureRepository
{
    Task<Signature?> GetByIdAsync(Guid id);
    Task<IEnumerable<Signature>> GetAllAsync();
    Task<IEnumerable<Signature>> GetByContractIdAsync(Guid contractId);
    Task<IEnumerable<Signature>> GetByUserIdAsync(Guid userId);
    Task<Signature?> GetByContractAndUserAsync(Guid contractId, Guid userId);
    Task AddAsync(Signature entity);
    Task UpdateAsync(Signature entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> IsContractFullySignedAsync(Guid contractId);
}