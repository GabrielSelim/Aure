using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários em contexto multi-tenant B2B
/// 
/// FLUXO DE USUÁRIOS:
/// 1. Admin registra empresa via /api/registration/company-admin
/// 2. Admin convida usuários via /api/registration/invite (sem senha)
/// 3. Usuário aceita convite via /api/registration/accept-invite/{token} (define senha)
/// 4. Usuário faz login via /api/auth/login
/// 
/// ENDPOINTS DISPONÍVEIS:
/// - GET /api/users - Lista usuários da empresa (qualquer usuário autenticado)
/// - GET /api/users/{id} - Busca usuário específico da empresa
/// - GET /api/users/email/{email} - Busca usuário por email na empresa
/// - PUT /api/users/{id} - Atualiza dados do usuário (nome, email)
/// - PATCH /api/users/{id}/password - Usuario troca própria senha
/// - DELETE /api/users/{id} - Admin remove usuário da empresa
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("User not associated with any company");
            return BadRequest(new { Error = "User not associated with any company" });
        }

        var result = await _userService.GetAllByCompanyAsync(companyId.Value);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to retrieve users: {Error}", result.Error);
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("User not associated with any company");
            return BadRequest(new { Error = "User not associated with any company" });
        }

        var result = await _userService.GetByIdAndCompanyAsync(id, companyId.Value);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("User not found with ID {UserId}", id);
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("User not associated with any company");
            return BadRequest(new { Error = "User not associated with any company" });
        }

        var result = await _userService.GetByEmailAndCompanyAsync(email, companyId.Value);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("User not found with email {Email}", email);
            return NotFound(new { Error = result.Error });
        }

        return Ok(result.Data);
    }

    // REMOVIDO: POST /api/users
    // Para criar usuários, use o fluxo de convite:
    // 1. POST /api/registration/invite (Admin convida usuário)
    // 2. POST /api/registration/accept-invite/{token} (Usuário aceita e define senha)

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("User not associated with any company");
            return BadRequest(new { Error = "User not associated with any company" });
        }

        // Verificar se o usuário a ser atualizado pertence à mesma empresa
        var userCheck = await _userService.GetByIdAndCompanyAsync(id, companyId.Value);
        if (userCheck.IsFailure)
        {
            _logger.LogWarning("User {UserId} not found in company {CompanyId}", id, companyId.Value);
            return NotFound(new { Error = "User not found" });
        }

        var result = await _userService.UpdateAsync(id, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to update user {UserId}: {Error}", id, result.Error);
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPatch("{id:guid}/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        
        // Usuários só podem trocar a própria senha
        if (id != currentUserId)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to change password for user {TargetUserId}", 
                              currentUserId, id);
            return Forbid("You can only change your own password");
        }

        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("User not associated with any company");
            return BadRequest(new { Error = "User not associated with any company" });
        }

        var result = await _userService.ChangePasswordAsync(id, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to change password for user {UserId}: {Error}", id, result.Error);
            return BadRequest(new { Error = result.Error });
        }

        _logger.LogInformation("Password changed successfully for user {UserId}", id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveUserFromCompany(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (id == currentUserId)
        {
            _logger.LogWarning("Admin cannot delete their own account");
            return BadRequest(new { Error = "Cannot delete your own account" });
        }

        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("Admin user not associated with any company");
            return BadRequest(new { Error = "Admin user not associated with any company" });
        }

        // Verificar se o usuário a ser removido pertence à mesma empresa
        var userCheck = await _userService.GetByIdAndCompanyAsync(id, companyId.Value);
        if (userCheck.IsFailure)
        {
            _logger.LogWarning("User {UserId} not found in company {CompanyId}", id, companyId.Value);
            return NotFound(new { Error = "User not found" });
        }

        var result = await _userService.DeleteAsync(id);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to remove user {UserId}: {Error}", id, result.Error);
            return BadRequest(new { Error = result.Error });
        }

        _logger.LogInformation("User {UserId} removed from company {CompanyId} by admin {AdminId}", 
                              id, companyId.Value, currentUserId);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        if (id == currentUserId)
        {
            _logger.LogWarning("Admin cannot deactivate their own account");
            return BadRequest(new { Error = "Cannot deactivate your own account" });
        }

        var companyId = await GetCurrentUserCompanyIdAsync();
        if (companyId == null)
        {
            _logger.LogWarning("Admin user not associated with any company");
            return BadRequest(new { Error = "Admin user not associated with any company" });
        }

        // Verificar se o usuário pertence à mesma empresa
        var userCheck = await _userService.GetByIdAndCompanyAsync(id, companyId.Value);
        if (userCheck.IsFailure)
        {
            _logger.LogWarning("User {UserId} not found in company {CompanyId}", id, companyId.Value);
            return NotFound(new { Error = "User not found" });
        }

        // TODO: Implementar lógica de desativação (soft delete mais elegante)
        var result = await _userService.DeleteAsync(id);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to deactivate user {UserId}: {Error}", id, result.Error);
            return BadRequest(new { Error = result.Error });
        }

        _logger.LogInformation("User {UserId} deactivated in company {CompanyId} by admin {AdminId}", 
                              id, companyId.Value, currentUserId);
        return NoContent();
    }
}