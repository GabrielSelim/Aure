using Aure.Application.DTOs.Contract;
using Aure.Application.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Exceptions;
using Aure.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aure.Application.Services;

public class ContractTemplateService : IContractTemplateService
{
    private readonly IContractDocumentRepository _documentRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ContractTemplateService> _logger;

    public ContractTemplateService(
        IContractDocumentRepository documentRepository,
        IContractRepository contractRepository,
        IUserRepository userRepository,
        ILogger<ContractTemplateService> logger)
    {
        _documentRepository = documentRepository;
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<ContractDocumentResponse>> GetDocumentosByContractIdAsync(Guid contractId, Guid companyId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null || (contract.ClientId != companyId && contract.ProviderId != companyId))
        {
            throw new NotFoundException("Contrato não encontrado");
        }

        var documents = await _documentRepository.GetByContractIdAsync(contractId);

        return documents.Select(d => MapToDocumentResponse(d, d.Template?.Nome)).ToList();
    }

    public async Task<ContractDocumentResponse> GetVersaoFinalAsync(Guid contractId, Guid companyId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null || (contract.ClientId != companyId && contract.ProviderId != companyId))
        {
            throw new NotFoundException("Contrato não encontrado");
        }

        var document = await _documentRepository.GetVersaoFinalAsync(contractId);
        if (document == null)
        {
            throw new NotFoundException("Nenhuma versão final encontrada para este contrato");
        }

        return MapToDocumentResponse(document, document.Template?.Nome);
    }

    public async Task DefinirComoVersaoFinalAsync(Guid documentId, Guid companyId, Guid userId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new NotFoundException("Documento não encontrado");
        }

        var contract = await _contractRepository.GetByIdAsync(document.ContractId);
        if (contract == null || (contract.ClientId != companyId && contract.ProviderId != companyId))
        {
            throw new UnauthorizedException("Você não tem permissão para esta ação");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.DonoEmpresaPai)
        {
            throw new UnauthorizedException("Apenas o Dono da Empresa pode definir a versão final");
        }

        document.DefinirComoVersaoFinal();
        await _documentRepository.UpdateAsync(document);

        _logger.LogInformation(
            "Documento {DocumentId} definido como versão final por {UserName}",
            documentId, user.Name);
    }

    private ContractDocumentResponse MapToDocumentResponse(ContractDocument document, string? nomeTemplate)
    {
        return new ContractDocumentResponse
        {
            Id = document.Id,
            ContractId = document.ContractId,
            TemplateId = document.TemplateId,
            NomeTemplate = nomeTemplate,
            ConteudoHtml = document.ConteudoHtml,
            ConteudoPdfBase64 = document.ConteudoPdf,
            ConteudoDocxBase64 = document.ConteudoDocx,
            Versao = document.GetVersao(),
            DataGeracao = document.DataGeracao,
            GeradoPorUsuarioNome = document.GeradoPorUsuario?.Name,
            DadosPreenchidos = document.DadosPreenchidos,
            EhVersaoFinal = document.EhVersaoFinal,
            HashDocumento = document.HashDocumento
        };
    }
}
