using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using Aure.Domain.Enums;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(IUserService userService, ILogger<RegistrationController> logger)
    {
        _userService = userService;
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

        return Ok(new { Message = "User invitation sent successfully", InviteId = result.Data });
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
}