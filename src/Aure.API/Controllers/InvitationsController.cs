using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvitationsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<InvitationsController> _logger;

    public InvitationsController(IUserService userService, ILogger<InvitationsController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar todos os convites da empresa",
        Description = @"Lista todos os convites (pendentes, aceitos, expirados, cancelados) da empresa do usuário logado.

**Permissões:**
- DonoEmpresaPai: Visualiza todos os convites
- Financeiro: Visualiza todos os convites
- Juridico: Visualiza todos os convites

**Informações Retornadas:**
- Nome e email do convidado
- Role e cargo
- Status do convite (Pending, Accepted, Expired, Cancelled)
- Datas de criação, expiração e aceitação
- Nome de quem convidou e quem aceitou
- Flag indicando se está expirado
- Flag indicando se pode ser editado"
    )]
    [SwaggerResponse(200, "Lista de convites", typeof(IEnumerable<UserInvitationListResponse>))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Sem permissão para visualizar convites")]
    [ProducesResponseType(typeof(IEnumerable<UserInvitationListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.GetInvitationsAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar convites");
            return StatusCode(500, new { message = "Erro ao listar convites" });
        }
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter detalhes de um convite específico",
        Description = @"Retorna os detalhes completos de um convite específico.

**Validações:**
- Convite deve pertencer à empresa do usuário logado
- Usuário deve ter permissão para visualizar convites"
    )]
    [SwaggerResponse(200, "Detalhes do convite", typeof(UserInvitationListResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Convite não pertence à sua empresa")]
    [SwaggerResponse(404, "Convite não encontrado")]
    [ProducesResponseType(typeof(UserInvitationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitationById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.GetInvitationByIdAsync(id, userId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter convite {InvitationId}", id);
            return StatusCode(500, new { message = "Erro ao obter convite" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    [SwaggerOperation(
        Summary = "Editar um convite pendente",
        Description = @"Permite editar os dados de um convite que ainda está pendente.

**Permissões:**
- Apenas DonoEmpresaPai e Financeiro podem editar

**Campos Editáveis:**
- Nome do convidado
- Email do convidado
- Role (tipo de usuário)
- Cargo

**Regras:**
- Apenas convites com status 'Pending' podem ser editados
- Convites expirados não podem ser editados
- Convites já aceitos não podem ser editados
- Novo email não pode estar cadastrado no sistema
- Novo email não pode ter outro convite pendente

**Caso de Uso:**
Útil quando você digitou o email errado, escreveu o nome incorreto ou precisa alterar o cargo/role antes da pessoa aceitar o convite."
    )]
    [SwaggerResponse(200, "Convite atualizado com sucesso", typeof(UpdateInvitationResponse))]
    [SwaggerResponse(400, "Dados inválidos ou convite não pode ser editado")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Sem permissão para editar convites")]
    [SwaggerResponse(404, "Convite não encontrado")]
    [ProducesResponseType(typeof(UpdateInvitationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvitation(Guid id, [FromBody] UpdateInvitationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UpdateInvitationAsync(id, request, userId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar convite {InvitationId}", id);
            return StatusCode(500, new { message = "Erro ao atualizar convite" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    [SwaggerOperation(
        Summary = "Cancelar um convite pendente",
        Description = @"Cancela um convite que ainda está pendente.

**Permissões:**
- Apenas DonoEmpresaPai e Financeiro podem cancelar

**Regras:**
- Apenas convites com status 'Pending' podem ser cancelados
- Convite cancelado muda para status 'Cancelled' (não é deletado)
- Histórico do convite é mantido para auditoria

**Caso de Uso:**
Útil quando o convite foi enviado por engano ou a pessoa não vai mais trabalhar na empresa."
    )]
    [SwaggerResponse(200, "Convite cancelado com sucesso", typeof(CancelInvitationResponse))]
    [SwaggerResponse(400, "Convite não pode ser cancelado")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Sem permissão para cancelar convites")]
    [SwaggerResponse(404, "Convite não encontrado")]
    [ProducesResponseType(typeof(CancelInvitationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.CancelInvitationAsync(id, userId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar convite {InvitationId}", id);
            return StatusCode(500, new { message = "Erro ao cancelar convite" });
        }
    }
}
