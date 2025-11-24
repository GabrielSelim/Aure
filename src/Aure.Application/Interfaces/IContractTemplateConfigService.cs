using Aure.Application.DTOs.Contract;
using Aure.Domain.Common;

namespace Aure.Application.Interfaces
{
    public interface IContractTemplateConfigService
    {
        Task<Result<List<ContractTemplatePresetResponse>>> GetPresetsAsync();
        Task<Result<ContractTemplatePresetResponse?>> GetPresetByTipoAsync(string tipo);
        Task<Result<List<ContractTemplateConfigResponse>>> GetAllCompanyConfigsAsync(Guid userId);
        Task<Result<ContractTemplateConfigResponse?>> GetCompanyConfigByNomeAsync(Guid userId, string nomeConfig);
        Task<Result<ContractTemplateConfigResponse>> CreateOrUpdateConfigAsync(Guid userId, ContractTemplateConfigRequest request);
        Task<Result<ContractTemplateConfigResponse>> ClonarPresetAsync(Guid userId, string tipoPreset, string nomeConfig);
        Task<Result<string>> PreviewContractHtmlAsync(Guid userId, PreviewTemplateRequest request);
        Task<Result<Guid>> GerarContratoComConfigAsync(Guid userId, GerarContratoComConfigRequest request);
        Task<Result<bool>> DeleteCompanyConfigAsync(Guid userId, string nomeConfig);
    }
}
