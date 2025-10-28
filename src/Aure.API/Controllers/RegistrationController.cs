using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
    [SwaggerOperation(
        Summary = "Registrar primeiro usuário (Dono da Empresa Pai)",
        Description = "Cria a primeira conta da empresa, que será automaticamente o Dono da Empresa Pai com todos os privilégios administrativos."
    )]
    [SwaggerResponse(200, "Empresa e usuário criados com sucesso", typeof(UserResponse))]
    [SwaggerResponse(400, "Dados inválidos ou CNPJ já cadastrado")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarAdminEmpresa(
        [FromBody]
        [SwaggerRequestBody("Dados para registro da empresa e primeiro usuário", Required = true)]
        RegisterCompanyAdminRequest request)
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
    [Authorize(Roles = "DonoEmpresaPai")]
    [SwaggerOperation(
        Summary = "Convidar usuário interno ou funcionário PJ",
        Description = @"Permite ao Dono da Empresa Pai convidar:
        
**Usuários Internos (InviteType: Internal):**
- Role: Financeiro ou Juridico
- Não requer dados de empresa PJ
        
**Funcionários PJ (InviteType: ContractedPJ):**
- Role: Automaticamente será FuncionarioPJ
- Requer: companyName, cnpj, companyType (Provider), businessModel (ContractedPJ)
- Criará uma empresa PJ vinculada à empresa pai"
    )]
    [SwaggerResponse(200, "Convite enviado com sucesso", typeof(UserResponse))]
    [SwaggerResponse(400, "Dados inválidos ou usuário já cadastrado")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Apenas Dono da Empresa Pai pode convidar usuários")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConvidarUsuario(
        [FromBody]
        [SwaggerRequestBody(@"Exemplo para Funcionário PJ:
{
  ""name"": ""João Silva"",
  ""email"": ""joao.silva@empresa.com"",
  ""role"": ""FuncionarioPJ"",
  ""inviteType"": ""ContractedPJ"",
  ""companyName"": ""João Silva Consultoria ME"",
  ""cnpj"": ""12345678000190"",
  ""companyType"": ""Provider"",
  ""businessModel"": ""ContractedPJ""
}

Exemplo para Usuário Interno (Financeiro):
{
  ""name"": ""Maria Financeira"",
  ""email"": ""maria@empresa.com"",
  ""role"": ""Financeiro"",
  ""inviteType"": ""Internal""
}", Required = true)]
        InviteUserRequest request)
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
    [SwaggerOperation(
        Summary = "Aceitar convite e definir senha",
        Description = "Permite que o usuário convidado aceite o convite e defina sua senha. O token é enviado por email."
    )]
    [SwaggerResponse(200, "Convite aceito e usuário ativado", typeof(UserResponse))]
    [SwaggerResponse(400, "Token inválido ou expirado")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AceitarConvite(
        [SwaggerParameter("Token de convite recebido por email", Required = true)]
        string inviteToken,
        [FromBody]
        [SwaggerRequestBody(@"Exemplo:
{
  ""password"": ""SenhaSegura@123""
}", Required = true)]
        AcceptInviteRequest request)
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
    [Authorize(Roles = "DonoEmpresaPai")]
    [SwaggerOperation(
        Summary = "Listar convites pendentes",
        Description = "Retorna todos os convites pendentes enviados pela empresa. Apenas Dono da Empresa Pai tem acesso."
    )]
    [SwaggerResponse(200, "Lista de convites pendentes")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Acesso negado")]
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
    [Authorize(Roles = "DonoEmpresaPai")]
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

    [HttpPost("reenviar-convite/{inviteId}")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
    [SwaggerOperation(
        Summary = "Reenviar convite pendente",
        Description = "Reenvia o email de convite para um convite pendente que ainda não foi aceito. Gera novo token e estende validade por 7 dias."
    )]
    [SwaggerResponse(200, "Convite reenviado com sucesso", typeof(InviteResponse))]
    [SwaggerResponse(400, "Convite já aceito ou inválido")]
    [SwaggerResponse(404, "Convite não encontrado")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Acesso negado")]
    public async Task<IActionResult> ReenviarConvite(Guid inviteId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { erro = "Usuário não autenticado" });
        }

        var result = await _userService.ResendInviteEmailAsync(inviteId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { erro = result.Error });
        }

        return Ok(new 
        { 
            mensagem = "Convite reenviado com sucesso",
            convite = result.Data
        });
    }
}