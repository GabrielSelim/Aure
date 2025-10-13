using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LedgerController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LedgerController> _logger;

    public LedgerController(IUnitOfWork unitOfWork, ILogger<LedgerController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obter extratos contábeis da empresa
    /// </summary>
    [HttpGet("extratos")]
    public async Task<IActionResult> GetExtratos(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? contractId = null,
        [FromQuery] Guid? paymentId = null)
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

            IEnumerable<Domain.Entities.LedgerEntry> entries;

            if (paymentId.HasValue)
            {
                entries = await _unitOfWork.LedgerEntries.GetByPaymentIdAsync(paymentId.Value);
            }
            else if (contractId.HasValue)
            {
                entries = await _unitOfWork.LedgerEntries.GetByContractIdAsync(contractId.Value);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                entries = await _unitOfWork.LedgerEntries.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                entries = await _unitOfWork.LedgerEntries.GetByCompanyIdAsync(user.CompanyId.Value);
            }

            // Filtrar apenas entradas da empresa do usuário
            entries = entries.Where(e => e.Contract.ClientId == user.CompanyId || e.Contract.ProviderId == user.CompanyId);

            var result = entries.Select(entry => new
            {
                Id = entry.Id,
                PagamentoId = entry.PaymentId,
                ContratoId = entry.ContractId,
                Debito = entry.Debit,
                Credito = entry.Credit,
                Moeda = entry.Currency,
                DataHora = entry.Timestamp,
                Observacoes = entry.Note,
                ContractTitle = entry.Contract?.Title,
                PaymentAmount = entry.Payment?.Amount
            });

            var totalDebit = entries.Sum(e => e.Debit);
            var totalCredit = entries.Sum(e => e.Credit);

            return Ok(new
            {
                TotalRegistros = entries.Count(),
                TotalDebito = totalDebit,
                TotalCredito = totalCredit,
                Saldo = totalCredit - totalDebit,
                Registros = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar extratos contábeis");
            return BadRequest(new { mensagem = "Erro ao buscar extratos contábeis" });
        }
    }

    /// <summary>
    /// Obter balanço/saldo da empresa
    /// </summary>
    [HttpGet("balanco")]
    public async Task<IActionResult> GetBalanco([FromQuery] DateTime? asOfDate = null)
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

            var balance = await _unitOfWork.LedgerEntries.GetBalanceAsync(user.CompanyId.Value, asOfDate);
            var totalDebit = await _unitOfWork.LedgerEntries.GetTotalDebitAsync(user.CompanyId.Value, 
                asOfDate.HasValue ? DateTime.MinValue : null, asOfDate);
            var totalCredit = await _unitOfWork.LedgerEntries.GetTotalCreditAsync(user.CompanyId.Value, 
                asOfDate.HasValue ? DateTime.MinValue : null, asOfDate);

            return Ok(new
            {
                DataConsulta = asOfDate ?? DateTime.UtcNow,
                TotalDebito = totalDebit,
                TotalCredito = totalCredit,
                SaldoAtual = balance,
                Moeda = "BRL"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular balanço");
            return BadRequest(new { mensagem = "Erro ao calcular balanço" });
        }
    }

    /// <summary>
    /// Obter relatório financeiro detalhado
    /// </summary>
    [HttpGet("relatorio-financeiro")]
    public async Task<IActionResult> GetRelatorioFinanceiro(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
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

            var entries = await _unitOfWork.LedgerEntries.GetByDateRangeAsync(startDate, endDate);
            var companyEntries = entries.Where(e => e.Contract.ClientId == user.CompanyId || e.Contract.ProviderId == user.CompanyId);

            var contracts = await _unitOfWork.Contracts.GetByCompanyIdAsync(user.CompanyId.Value);
            var payments = await _unitOfWork.Payments.GetByCompanyIdAsync(user.CompanyId.Value);

            // Agrupar por contrato
            var contractSummary = companyEntries
                .GroupBy(e => e.ContractId)
                .Select(g => new
                {
                    ContratoId = g.Key,
                    TituloContrato = g.First().Contract?.Title,
                    TotalDebito = g.Sum(e => e.Debit),
                    TotalCredito = g.Sum(e => e.Credit),
                    Saldo = g.Sum(e => e.Credit) - g.Sum(e => e.Debit),
                    NumeroRegistros = g.Count()
                });

            // Agrupar por período (mensal)
            var monthlySummary = companyEntries
                .GroupBy(e => new { e.Timestamp.Year, e.Timestamp.Month })
                .Select(g => new
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalDebito = g.Sum(e => e.Debit),
                    TotalCredito = g.Sum(e => e.Credit),
                    Saldo = g.Sum(e => e.Credit) - g.Sum(e => e.Debit)
                })
                .OrderBy(m => m.Ano).ThenBy(m => m.Mes);

            return Ok(new
            {
                Periodo = new
                {
                    Inicio = startDate,
                    Fim = endDate
                },
                Resumo = new
                {
                    TotalDebito = companyEntries.Sum(e => e.Debit),
                    TotalCredito = companyEntries.Sum(e => e.Credit),
                    SaldoLiquido = companyEntries.Sum(e => e.Credit) - companyEntries.Sum(e => e.Debit),
                    TotalContratos = contractSummary.Count(),
                    TotalRegistros = companyEntries.Count()
                },
                ResumoPorContrato = contractSummary,
                ResumoPorMes = monthlySummary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório financeiro");
            return BadRequest(new { mensagem = "Erro ao gerar relatório financeiro" });
        }
    }
}