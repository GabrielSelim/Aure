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

    [HttpPost("company-admin")]
    public async Task<IActionResult> RegisterCompanyAdmin([FromBody] RegisterCompanyAdminRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.RegisterCompanyAdminAsync(request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao registrar admin da empresa: {Error}", result.Error);
            return BadRequest(new { Error = result.Error });
        }

        return CreatedAtAction(nameof(UsersController.GetUserById), "Users", new { id = result.Data!.Id }, result.Data);
    }

    [HttpPost("invite-user")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> InviteUser([FromBody] InviteUserRequest request)
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
            _logger.LogError("Failed to invite user: {Error}", result.Error);
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("accept-invite/{inviteToken}")]
    public async Task<IActionResult> AcceptInvite(string inviteToken, [FromBody] AcceptInviteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.AcceptInviteAsync(inviteToken, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Failed to accept invite: {Error}", result.Error);
            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpGet("invites")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetPendingInvites()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var currentUser = await _userService.GetByIdAsync(currentUserId);
        
        if (currentUser.IsFailure || currentUser.Data?.CompanyId == null)
        {
            return BadRequest(new { Error = "User not associated with company" });
        }

        var invites = await _unitOfWork.UserInvites.GetPendingByCompanyAsync(currentUser.Data.CompanyId.Value);
        var inviteResponses = invites.Select(invite => new UserInviteResponse(
            Id: invite.Id,
            InviterName: invite.InviterName,
            InviteeEmail: invite.InviteeEmail,
            InviteeName: invite.InviteeName,
            Role: invite.Role,
            InviteType: invite.InviteType,
            CompanyName: invite.CompanyName,
            Cnpj: invite.Cnpj,
            CompanyType: invite.CompanyType,
            BusinessModel: invite.BusinessModel,
            Token: invite.Token,
            ExpiresAt: invite.ExpiresAt,
            CreatedAt: invite.CreatedAt,
            IsExpired: DateTime.UtcNow > invite.ExpiresAt
        ));

        return Ok(inviteResponses);
    }

    [HttpPost("cancel-invite/{inviteId}")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> CancelInvite(Guid inviteId)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var currentUser = await _userService.GetByIdAsync(currentUserId);
        
        if (currentUser.IsFailure || currentUser.Data?.CompanyId == null)
        {
            return BadRequest(new { Error = "User not associated with company" });
        }

        var invite = await _unitOfWork.UserInvites.GetByIdAsync(inviteId);
        if (invite == null || invite.CompanyId != currentUser.Data.CompanyId)
        {
            return NotFound(new { Error = "Invite not found" });
        }

        if (invite.IsAccepted)
        {
            return BadRequest(new { Error = "Cannot cancel accepted invite" });
        }

        await _unitOfWork.UserInvites.DeleteAsync(inviteId);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { Message = "Invite cancelled successfully" });
    }
}