using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces
{
    public interface IContractTemplateConfigRepository
    {
        Task<ContractTemplateConfig?> GetByIdAsync(Guid id);
        Task<ContractTemplateConfig?> GetByCompanyIdAndNomeAsync(Guid companyId, string nomeConfig);
        Task<IEnumerable<ContractTemplateConfig>> GetAllByCompanyIdAsync(Guid companyId);
        Task<IEnumerable<ContractTemplateConfig>> GetAllAsync();
        Task AddAsync(ContractTemplateConfig config);
        Task UpdateAsync(ContractTemplateConfig config);
        Task DeleteAsync(Guid id);
    }
}
