using Aure.Application.DTOs.Contract;

namespace Aure.Application.Interfaces;

public interface IContractTemplateService
{
    Task<List<ContractDocumentResponse>> GetDocumentosByContractIdAsync(Guid contractId, Guid companyId);
    Task<ContractDocumentResponse> GetVersaoFinalAsync(Guid contractId, Guid companyId);
    Task DefinirComoVersaoFinalAsync(Guid documentId, Guid companyId, Guid userId);
}
