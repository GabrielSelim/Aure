using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aure.Application.DTOs.Notification;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using Aure.Application.Interfaces;
using Aure.Domain.Entities;
using Aure.Infrastructure.Data;
using System.Security.Claims;

namespace Aure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AureDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationsController> _logger;
        private readonly IConfiguration _configuration;

        public NotificationsController(
            IUnitOfWork unitOfWork,
            AureDbContext context,
            IEmailService emailService, 
            ILogger<NotificationsController> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("notificar-completar-cadastro-pj")]
        [Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
        [ProducesResponseType(typeof(NotificarCompletarCadastroResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificarCompletarCadastroResponse>> NotificarCompletarCadastro(
            [FromBody] NotificarCompletarCadastroRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);

                if (currentUser == null)
                    return Unauthorized(new { message = "Usuário não autenticado" });

                var funcionarioPJ = await _unitOfWork.Users.GetByIdAsync(request.UserId);

                if (funcionarioPJ == null)
                    return NotFound(new { message = "Funcionário não encontrado" });

                if (funcionarioPJ.Role != UserRole.FuncionarioPJ)
                    return BadRequest(new { message = "Usuário não é um funcionário PJ" });

                if (funcionarioPJ.CompanyId != currentUser.CompanyId)
                    return Forbid();

                if (request.CamposFaltando == null || !request.CamposFaltando.Any())
                    return BadRequest(new { message = "Lista de campos faltando não pode estar vazia" });

                var historicoRecente = await _context.NotificationHistories
                    .Where(h => h.UserId == request.UserId && h.TipoNotificacao == "CompletarCadastro")
                    .OrderByDescending(h => h.DataEnvio)
                    .FirstOrDefaultAsync();

                if (historicoRecente != null && historicoRecente.FoiEnviadoRecentemente(24) && historicoRecente.MesmosCampos(request.CamposFaltando))
                {
                    return Ok(new NotificarCompletarCadastroResponse
                    {
                        Success = false,
                        Message = "Email já foi enviado para este funcionário nas últimas 24 horas com os mesmos campos"
                    });
                }

                var company = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value);
                var systemUrl = _configuration["FrontendUrl"] ?? "https://aure.gabrielsanztech.com.br";

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendCompletarCadastroEmailAsync(
                            funcionarioPJ.Email,
                            funcionarioPJ.Name,
                            company?.Name ?? "Empresa",
                            request.CamposFaltando,
                            systemUrl
                        );

                        var historico = new NotificationHistory(
                            request.UserId,
                            "CompletarCadastro",
                            request.CamposFaltando
                        );

                        await _context.NotificationHistories.AddAsync(historico);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation(
                            "Email de completar cadastro enviado para {Email}. Campos faltando: {Campos}",
                            funcionarioPJ.Email,
                            string.Join(", ", request.CamposFaltando)
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao enviar email de completar cadastro para {Email}", funcionarioPJ.Email);
                    }
                });

                return Ok(new NotificarCompletarCadastroResponse
                {
                    Success = true,
                    Message = "Email enviado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificação de completar cadastro");
                return StatusCode(500, new { message = "Erro interno ao processar notificação" });
            }
        }

        [HttpPost("notificar-completar-cadastro-empresa")]
        [Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
        [ProducesResponseType(typeof(NotificarEmpresaIncompletaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificarEmpresaIncompletaResponse>> NotificarEmpresaIncompleta([FromBody] NotificarEmpresaIncompletaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == request.EmpresaId);

            if (company == null)
                return NotFound(new { message = "Empresa não encontrada" });

            var gestores = await _context.Users
                .Where(u => u.CompanyId == request.EmpresaId && 
                           (u.Role == UserRole.DonoEmpresaPai || 
                            u.Role == UserRole.Financeiro || 
                            u.Role == UserRole.Juridico))
                .ToListAsync();                if (!gestores.Any())
                    return NotFound(new { message = "Nenhum gestor encontrado para esta empresa" });

                var dono = gestores.FirstOrDefault(u => u.Role == UserRole.DonoEmpresaPai);
                
                if (dono == null)
                    return NotFound(new { message = "Proprietário da empresa não encontrado" });

                var historicoRecente = await _context.NotificationHistories
                    .Where(h => h.UserId == dono.Id && h.TipoNotificacao == "EmpresaIncompleta")
                    .OrderByDescending(h => h.DataEnvio)
                    .FirstOrDefaultAsync();

                if (historicoRecente != null && historicoRecente.FoiEnviadoRecentemente(24))
                {
                    if (historicoRecente.MesmosCampos(request.CamposFaltando))
                    {
                        return Ok(new NotificarEmpresaIncompletaResponse
                        {
                            Success = false,
                            Message = "Esta notificação já foi enviada nas últimas 24 horas",
                            Destinatarios = new List<string> { dono.Email }
                        });
                    }
                }

                var systemUrl = _emailService.GetType()
                    .GetProperty("FrontendUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_emailService)?.ToString() ?? "https://aure.gabrielsanztech.com.br";

                await Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmpresaIncompletaEmailAsync(
                            dono.Email,
                            dono.Name,
                            company.Name,
                            request.CamposFaltando,
                            systemUrl,
                            request.UsuarioSolicitante
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao enviar email de empresa incompleta para {Email}", dono.Email);
                    }
                });

                var historico = new NotificationHistory(
                    dono.Id,
                    "EmpresaIncompleta",
                    request.CamposFaltando
                );

                _context.NotificationHistories.Add(historico);
                await _unitOfWork.CommitAsync();

                return Ok(new NotificarEmpresaIncompletaResponse
                {
                    Success = true,
                    Message = "Email enviado com sucesso ao proprietário da empresa",
                    Destinatarios = new List<string> { dono.Email }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificação de empresa incompleta");
                return StatusCode(500, new { message = "Erro interno ao processar notificação" });
            }
        }
    }
}
