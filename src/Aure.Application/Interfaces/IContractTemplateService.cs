using Aure.Application.DTOs.Contract;

namespace Aure.Application.Interfaces;

public interface IContractTemplateService
{
    Task<ContractTemplateResponse> CreateTemplateAsync(CreateContractTemplateRequest request, Guid companyId, Guid userId);
    Task<ContractTemplateResponse> UpdateTemplateAsync(Guid templateId, UpdateContractTemplateRequest request, Guid userId);
    Task<ContractTemplateResponse> GetTemplateByIdAsync(Guid templateId, Guid companyId);
    Task<List<ContractTemplateListResponse>> GetAllTemplatesAsync(Guid companyId, bool apenasAtivos = true);
    Task<ContractTemplateResponse> GetTemplatePadraoAsync(Guid companyId, string tipo);
    Task DefinirComoPadraoAsync(Guid templateId, Guid companyId, Guid userId);
    Task RemoverPadraoAsync(Guid templateId, Guid companyId, Guid userId);
    Task AtivarTemplateAsync(Guid templateId, Guid companyId, Guid userId);
    Task DesativarTemplateAsync(Guid templateId, string motivo, Guid companyId, Guid userId);
    Task<AvailableVariablesResponse> GetVariaveisDisponiveisAsync();
    Task<ContractDocumentResponse> GerarContratoDeTemplateAsync(GenerateContractFromTemplateRequest request, Guid userId);
    Task<ContractDocumentResponse> UploadContratoPersonalizadoAsync(UploadCustomContractRequest request, Guid userId);
    Task<List<ContractDocumentResponse>> GetDocumentosByContractIdAsync(Guid contractId, Guid companyId);
    Task<ContractDocumentResponse> GetVersaoFinalAsync(Guid contractId, Guid companyId);
    Task DefinirComoVersaoFinalAsync(Guid documentId, Guid companyId, Guid userId);
}
