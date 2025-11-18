using Aure.Application.DTOs.Contract;
using Aure.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractTemplatesController : ControllerBase
{
    private readonly IContractTemplateService _templateService;
    private readonly ILogger<ContractTemplatesController> _logger;

    public ContractTemplatesController(
        IContractTemplateService templateService,
        ILogger<ContractTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [ProducesResponseType(typeof(ContractTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContractTemplateResponse>> CriarTemplate([FromBody] CreateContractTemplateRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        var result = await _templateService.CreateTemplateAsync(request, companyId, userId);

        return CreatedAtAction(nameof(ObterTemplatePorId), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [ProducesResponseType(typeof(ContractTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractTemplateResponse>> AtualizarTemplate(
        Guid id,
        [FromBody] UpdateContractTemplateRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var result = await _templateService.UpdateTemplateAsync(id, request, userId);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContractTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractTemplateResponse>> ObterTemplatePorId(Guid id)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        var result = await _templateService.GetTemplateByIdAsync(id, companyId);

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ContractTemplateListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContractTemplateListResponse>>> ListarTemplates(
        [FromQuery] bool apenasAtivos = true)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        var result = await _templateService.GetAllTemplatesAsync(companyId, apenasAtivos);

        return Ok(result);
    }

    [HttpGet("padrao/{tipo}")]
    [ProducesResponseType(typeof(ContractTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractTemplateResponse>> ObterTemplatePadrao(string tipo)
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        var result = await _templateService.GetTemplatePadraoAsync(companyId, tipo);

        return Ok(result);
    }

    [HttpPost("{id}/definir-padrao")]
    [Authorize(Roles = "DonoEmpresaPai")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DefinirComoPadrao(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        await _templateService.DefinirComoPadraoAsync(id, companyId, userId);

        return Ok(new { mensagem = "Template definido como padrão com sucesso" });
    }

    [HttpPost("{id}/remover-padrao")]
    [Authorize(Roles = "DonoEmpresaPai")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RemoverPadrao(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        await _templateService.RemoverPadraoAsync(id, companyId, userId);

        return Ok(new { mensagem = "Template removido como padrão com sucesso" });
    }

    [HttpPost("{id}/ativar")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> AtivarTemplate(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        await _templateService.AtivarTemplateAsync(id, companyId, userId);

        return Ok(new { mensagem = "Template ativado com sucesso" });
    }

    [HttpPost("{id}/desativar")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DesativarTemplate(Guid id, [FromBody] string motivo)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")?.Value!);

        await _templateService.DesativarTemplateAsync(id, motivo, companyId, userId);

        return Ok(new { mensagem = "Template desativado com sucesso" });
    }

    [HttpGet("variaveis-disponiveis")]
    [ProducesResponseType(typeof(AvailableVariablesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AvailableVariablesResponse>> ObterVariaveisDisponiveis()
    {
        var result = await _templateService.GetVariaveisDisponiveisAsync();

        return Ok(result);
    }

    [HttpPost("gerar-contrato")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [ProducesResponseType(typeof(ContractDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDocumentResponse>> GerarContratoDeTemplate(
        [FromBody] GenerateContractFromTemplateRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var result = await _templateService.GerarContratoDeTemplateAsync(request, userId);

        return CreatedAtAction(
            nameof(ContractDocumentsController.ObterDocumentoPorId),
            "ContractDocuments",
            new { id = result.Id },
            result);
    }

    [HttpPost("upload-personalizado")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [ProducesResponseType(typeof(ContractDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDocumentResponse>> UploadContratoPersonalizado(
        [FromBody] UploadCustomContractRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        var result = await _templateService.UploadContratoPersonalizadoAsync(request, userId);

        return CreatedAtAction(
            nameof(ContractDocumentsController.ObterDocumentoPorId),
            "ContractDocuments",
            new { id = result.Id },
            result);
    }
}
