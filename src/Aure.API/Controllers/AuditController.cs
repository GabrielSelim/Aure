using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IUnitOfWork unitOfWork, ILogger<AuditController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obter logs de auditoria
    /// </summary>
    [HttpGet("logs")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null,
        [FromQuery] Guid? userId = null)
    {
        try
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync();

            if (startDate.HasValue)
                logs = logs.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                logs = logs.Where(l => l.Timestamp <= endDate.Value);

            if (!string.IsNullOrEmpty(entityName))
                logs = logs.Where(l => l.EntityName.Contains(entityName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(action) && Enum.TryParse<AuditAction>(action, true, out var auditAction))
                logs = logs.Where(l => l.Action == auditAction);

            if (userId.HasValue)
                logs = logs.Where(l => l.PerformedBy == userId.Value);

            var result = logs.OrderByDescending(l => l.Timestamp).Select(log => new
            {
                Id = log.Id,
                NomeEntidade = log.EntityName,
                IdEntidade = log.EntityId,
                Acao = log.Action.ToString(),
                RealizadoPor = log.PerformedBy,
                EnderecoIp = log.IpAddress,
                DataHora = log.Timestamp,
                HashCadeia = log.HashChain
            });

            return Ok(new
            {
                TotalRegistros = result.Count(),
                Logs = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar logs de auditoria");
            return BadRequest(new { mensagem = "Erro ao buscar logs de auditoria" });
        }
    }

    /// <summary>
    /// Obter registros KYC da empresa
    /// </summary>
    [HttpGet("kyc")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
    public async Task<IActionResult> GetKycRecords([FromQuery] string? status = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { mensagem = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { mensagem = "Usuário ou empresa não encontrados" });
            }

            var kycRecords = await _unitOfWork.KycRecords.GetByCompanyIdAsync(user.CompanyId.Value);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<KycStatus>(status, true, out var kycStatus))
            {
                kycRecords = kycRecords.Where(k => k.Status == kycStatus);
            }

            var result = kycRecords.Select(kyc => new
            {
                Id = kyc.Id,
                EmpresaId = kyc.CompanyId,
                TipoDocumento = kyc.DocumentType.ToString(),
                HashDocumento = kyc.DocumentHash,
                DataVerificacao = kyc.VerifiedAt,
                Status = kyc.Status.ToString(),
                ReferenciaPrestador = kyc.ProviderRef,
                DataCriacao = kyc.CreatedAt
            });

            return Ok(new
            {
                TotalRegistros = result.Count(),
                RegistrosKyc = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar registros KYC");
            return BadRequest(new { mensagem = "Erro ao buscar registros KYC" });
        }
    }

    /// <summary>
    /// Obter relatório de compliance
    /// </summary>
    [HttpGet("relatorio-compliance")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> GetRelatorioCompliance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            // Buscar dados de auditoria
            var auditLogs = await _unitOfWork.AuditLogs.GetAllAsync();
            var periodLogs = auditLogs.Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

            // Buscar dados KYC
            var kycRecords = await _unitOfWork.KycRecords.GetAllAsync();
            var periodKyc = kycRecords.Where(k => k.CreatedAt >= startDate && k.CreatedAt <= endDate);

            // Buscar contratos e pagamentos
            var contracts = await _unitOfWork.Contracts.GetAllAsync();
            var payments = await _unitOfWork.Payments.GetAllAsync();
            var invoices = await _unitOfWork.Invoices.GetAllAsync();

            var periodContracts = contracts.Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);
            var periodPayments = payments.Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);
            var periodInvoices = invoices.Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);

            // Estatísticas de auditoria
            var auditStats = periodLogs.GroupBy(l => l.Action).Select(g => new
            {
                Acao = g.Key.ToString(),
                Quantidade = g.Count()
            });

            // Estatísticas KYC
            var kycStats = periodKyc.GroupBy(k => k.Status).Select(g => new
            {
                Status = g.Key.ToString(),
                Quantidade = g.Count()
            });

            // Estatísticas financeiras
            var financialStats = new
            {
                TotalContratos = periodContracts.Count(),
                ValorTotalContratos = periodContracts.Sum(c => c.ValueTotal),
                TotalPagamentos = periodPayments.Count(),
                ValorTotalPagamentos = periodPayments.Sum(p => p.Amount),
                TotalNotasFiscais = periodInvoices.Count(),
                ValorTotalNotasFiscais = periodInvoices.Sum(i => i.TotalAmount)
            };

            return Ok(new
            {
                Periodo = new
                {
                    Inicio = startDate,
                    Fim = endDate
                },
                EstatisticasAuditoria = auditStats,
                EstatisticasKyc = kycStats,
                EstatisticasFinanceiras = financialStats,
                ResumoGeral = new
                {
                    TotalLogsAuditoria = periodLogs.Count(),
                    TotalRegistrosKyc = periodKyc.Count(),
                    TotalEventosCompliance = periodLogs.Count() + periodKyc.Count()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de compliance");
            return BadRequest(new { mensagem = "Erro ao gerar relatório de compliance" });
        }
    }

    /// <summary>
    /// Obter notificações da empresa
    /// </summary>
    [HttpGet("notificacoes")]
    public async Task<IActionResult> GetNotificacoes([FromQuery] string? type = null, [FromQuery] string? status = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { mensagem = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { mensagem = "Usuário ou empresa não encontrados" });
            }

            // Buscar contratos e pagamentos da empresa
            var contracts = await _unitOfWork.Contracts.GetByCompanyIdAsync(user.CompanyId.Value);
            var payments = await _unitOfWork.Payments.GetByCompanyIdAsync(user.CompanyId.Value);

            var contractIds = contracts.Select(c => c.Id).ToList();
            var paymentIds = payments.Select(p => p.Id).ToList();

            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var companyNotifications = allNotifications.Where(n => 
                (n.ContractId.HasValue && contractIds.Contains(n.ContractId.Value)) ||
                (n.PaymentId.HasValue && paymentIds.Contains(n.PaymentId.Value)));

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, true, out var notificationType))
            {
                companyNotifications = companyNotifications.Where(n => n.Type == notificationType);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, true, out var notificationStatus))
            {
                companyNotifications = companyNotifications.Where(n => n.Status == notificationStatus);
            }

            var result = companyNotifications.OrderByDescending(n => n.CreatedAt).Select(notification => new
            {
                Id = notification.Id,
                ContratoId = notification.ContractId,
                PagamentoId = notification.PaymentId,
                Tipo = notification.Type.ToString(),
                DestinatarioEmail = notification.RecipientEmail,
                Assunto = notification.Subject,
                Conteudo = notification.Content,
                DataEnvio = notification.SentAt,
                Status = notification.Status.ToString(),
                DataCriacao = notification.CreatedAt
            });

            return Ok(new
            {
                TotalNotificacoes = result.Count(),
                Notificacoes = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações");
            return BadRequest(new { mensagem = "Erro ao buscar notificações" });
        }
    }
}