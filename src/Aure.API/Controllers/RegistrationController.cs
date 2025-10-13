using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(IUserService userService, IUnitOfWork unitOfWork, ILogger<RegistrationController> logger)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost("admin-empresa")]
    public async Task<IActionResult> RegistrarAdminEmpresa([FromBody] RegisterCompanyAdminRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.RegisterCompanyAdminAsync(request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao registrar admin da empresa: {Error}", result.Error);
            return BadRequest(new { erro = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("convidar-usuario")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> ConvidarUsuario([FromBody] InviteUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value!;
        
        var result = await _userService.InviteUserAsync(request, currentUserId, currentUserRole);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao convidar usuário: {Error}", result.Error);
            return BadRequest(new { erro = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("aceitar-convite/{inviteToken}")]
    public async Task<IActionResult> AceitarConvite(string inviteToken, [FromBody] AcceptInviteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.AcceptInviteAsync(inviteToken, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao aceitar convite: {Error}", result.Error);
            return BadRequest(new { erro = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpGet("convites")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> ObterConvitesPendentes()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var currentUser = await _userService.GetByIdAsync(currentUserId);
        
        if (currentUser.IsFailure || currentUser.Data?.CompanyId == null)
        {
            return BadRequest(new { erro = "Usuário não associado a uma empresa" });
        }

        var invites = await _unitOfWork.UserInvites.GetPendingByCompanyAsync(currentUser.Data.CompanyId.Value);
        var respostasConvites = invites.Select(invite => new
        {
            Id = invite.Id,
            NomeConvidador = invite.InviterName,
            EmailConvidado = invite.InviteeEmail,
            NomeConvidado = invite.InviteeName,
            Funcao = invite.Role.ToString(),
            TipoConvite = invite.InviteType.ToString(),
            NomeEmpresa = invite.CompanyName,
            Cnpj = invite.Cnpj,
            TipoEmpresa = invite.CompanyType?.ToString(),
            ModeloNegocio = invite.BusinessModel?.ToString(),
            Token = invite.Token,
            ExpiraEm = invite.ExpiresAt,
            CriadoEm = invite.CreatedAt,
            EstaExpirado = DateTime.UtcNow > invite.ExpiresAt
        });

        return Ok(respostasConvites);
    }

    [HttpPost("cancelar-convite/{inviteId}")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> CancelarConvite(Guid inviteId)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var currentUser = await _userService.GetByIdAsync(currentUserId);
        
        if (currentUser.IsFailure || currentUser.Data?.CompanyId == null)
        {
            return BadRequest(new { erro = "Usuário não associado a uma empresa" });
        }

        var invite = await _unitOfWork.UserInvites.GetByIdAsync(inviteId);
        if (invite == null || invite.CompanyId != currentUser.Data.CompanyId)
        {
            return NotFound(new { erro = "Convite não encontrado" });
        }

        if (invite.IsAccepted)
        {
            return BadRequest(new { erro = "Não é possível cancelar um convite já aceito" });
        }

        await _unitOfWork.UserInvites.DeleteAsync(inviteId);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { mensagem = "Convite cancelado com sucesso" });
    }
}