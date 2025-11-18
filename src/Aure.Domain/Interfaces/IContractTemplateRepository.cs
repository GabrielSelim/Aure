using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface IContractTemplateRepository
{
    Task<ContractTemplate?> GetByIdAsync(Guid id);
    Task<List<ContractTemplate>> GetAllByCompanyIdAsync(Guid companyId, bool apenasAtivos = true);
    Task<List<ContractTemplate>> GetTemplatesSistemaAsync(bool apenasAtivos = true);
    Task<ContractTemplate?> GetTemplatePadraoAsync(Guid companyId, ContractTemplateType tipo);
    Task<List<ContractTemplate>> GetByTipoAsync(Guid companyId, ContractTemplateType tipo);
    Task AddAsync(ContractTemplate template);
    Task UpdateAsync(ContractTemplate template);
    Task<bool> ExisteTemplatePadraoAsync(Guid companyId, ContractTemplateType tipo, Guid? excluirTemplateId = null);
}
