using Aure.Application.DTOs.Contract;
using Aure.Domain.Common;

namespace Aure.Application.Interfaces
{
    public interface IContractTemplateConfigService
    {
        Task<Result<List<ContractTemplatePresetResponse>>> GetPresetsAsync();
        Task<Result<ContractTemplatePresetResponse?>> GetPresetByTipoAsync(string tipo);
        Task<Result<ContractTemplateConfigResponse?>> GetCompanyConfigAsync(Guid userId);
        Task<Result<ContractTemplateConfigResponse>> CreateOrUpdateConfigAsync(Guid userId, ContractTemplateConfigRequest request);
        Task<Result<string>> PreviewContractHtmlAsync(Guid userId, PreviewTemplateRequest request);
        Task<Result<bool>> DeleteCompanyConfigAsync(Guid userId);
    }
}
