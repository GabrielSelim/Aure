using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("entrar")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.LoginAsync(request);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            return Unauthorized(new { mensagem = result.Error });
        }

        var accessToken = _jwtService.GenerateAccessToken(result.Data!.UserEntity);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var response = new
        {
            tokenAcesso = accessToken,
            tokenRenovacao = refreshToken,
            expiraEm = expiresAt,
            usuario = result.Data.User
        };

        _logger.LogInformation("Successful login for user {UserId}", result.Data.User.Id);
        return Ok(response);
    }

    [HttpPost("sair")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var result = await _userService.LogoutAsync(request.UserId);
        
        if (result.IsFailure)
        {
            return BadRequest(new { Erro = result.Error });
        }

        return NoContent();
    }

    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> GetPerfil()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Erro = "Token inválido" });
        }

        var result = await _userService.GetByIdAsync(userId);
        
        if (result.IsFailure)
        {
            return NotFound(new { Erro = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("renovar-token")]
    public Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        return Task.FromResult<IActionResult>(Ok(new { Mensagem = "Endpoint de renovação de token - Implementação JWT necessária" }));
    }
}

public record LogoutRequest(Guid UserId);
public record RefreshTokenRequest(string RefreshToken);