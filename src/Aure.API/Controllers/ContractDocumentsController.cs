using Aure.Application.DTOs.Contract;
using Aure.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractDocumentsController : ControllerBase
{
    private readonly IContractTemplateService _templateService;
    private readonly ILogger<ContractDocumentsController> _logger;

    public ContractDocumentsController(
        IContractTemplateService templateService,
        ILogger<ContractDocumentsController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContractDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDocumentResponse>> ObterDocumentoPorId(Guid id)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        return Ok();
    }

    [HttpGet("contract/{contractId}")]
    [ProducesResponseType(typeof(List<ContractDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContractDocumentResponse>>> ListarDocumentosPorContrato(Guid contractId)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        var result = await _templateService.GetDocumentosByContractIdAsync(contractId, companyId);

        return Ok(result);
    }

    [HttpGet("contract/{contractId}/versao-final")]
    [ProducesResponseType(typeof(ContractDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDocumentResponse>> ObterVersaoFinal(Guid contractId)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        var result = await _templateService.GetVersaoFinalAsync(contractId, companyId);

        return Ok(result);
    }

    [HttpPost("{id}/definir-versao-final")]
    [Authorize(Roles = "DonoEmpresaPai")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DefinirComoVersaoFinal(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        await _templateService.DefinirComoVersaoFinalAsync(id, companyId, userId);

        return Ok(new { mensagem = "Documento definido como versão final com sucesso" });
    }
}
