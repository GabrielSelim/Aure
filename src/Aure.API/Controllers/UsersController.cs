using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

/// <summary>
/// Controller para gerenciamento pessoal de usuários
/// 
/// FLUXO DE USUÁRIOS:
/// 1. Admin registra empresa via /api/registration/company-admin
/// 2. Admin convida usuários via /api/registration/invite (sem senha)
/// 3. Usuário aceita convite via /api/registration/accept-invite/{token} (define senha)
/// 4. Usuário faz login via /api/auth/login
/// 
/// ENDPOINTS DISPONÍVEIS:
/// - GET /api/users/{id} - Busca usuário específico (próprio usuário ou da rede de relacionamentos)
/// - PUT /api/users/profile - Usuário atualiza seus próprios dados (nome, email)
/// - PATCH /api/users/password - Usuário troca própria senha
/// 
/// NOTA: Para listar usuários da empresa, use /api/CompanyRelationships
/// NOTA: Para gerenciar PJs, use os endpoints de CompanyRelationships
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private async Task<Guid?> GetCurrentUserCompanyIdAsync()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty) return null;

        var userResult = await _userService.GetByIdAsync(currentUserId);
        return userResult.IsSuccess ? userResult.Data?.CompanyId : null;
    }

    /// <summary>
    /// Usuário atualiza seus próprios dados (nome, email)
    /// </summary>
    [HttpPut("perfil")]
    [Authorize]
    public async Task<IActionResult> UpdateOwnProfile([FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            _logger.LogWarning("ID de usuário inválido do token");
            return BadRequest(new { Erro = "Autenticação de usuário inválida" });
        }

        var result = await _userService.UpdateAsync(currentUserId, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao atualizar perfil do usuário {UserId}: {Error}", currentUserId, result.Error);
            return BadRequest(new { Erro = result.Error });
        }

        _logger.LogInformation("Perfil atualizado com sucesso para o usuário {UserId}", currentUserId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Usuário troca sua própria senha
    /// </summary>
    [HttpPatch("senha")]
    [Authorize]
    public async Task<IActionResult> ChangeOwnPassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            _logger.LogWarning("ID de usuário inválido do token");
            return BadRequest(new { Erro = "Autenticação de usuário inválida" });
        }

        var result = await _userService.ChangePasswordAsync(currentUserId, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao trocar senha do usuário {UserId}: {Error}", currentUserId, result.Error);
            return BadRequest(new { Erro = result.Error });
        }

        _logger.LogInformation("Senha trocada com sucesso para o usuário {UserId}", currentUserId);
        return NoContent();
    }
}
