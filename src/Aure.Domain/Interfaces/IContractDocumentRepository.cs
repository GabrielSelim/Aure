using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface IContractDocumentRepository
{
    Task<ContractDocument?> GetByIdAsync(Guid id);
    Task<List<ContractDocument>> GetByContractIdAsync(Guid contractId);
    Task<ContractDocument?> GetVersaoFinalAsync(Guid contractId);
    Task<ContractDocument?> GetUltimaVersaoAsync(Guid contractId);
    Task AddAsync(ContractDocument document);
    Task UpdateAsync(ContractDocument document);
}
