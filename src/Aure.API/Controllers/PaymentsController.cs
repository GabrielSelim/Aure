using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IUnitOfWork unitOfWork, ILogger<PaymentsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os pagamentos da empresa do usuário atual
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] PaymentStatus? status = null, [FromQuery] Guid? contractId = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário ou empresa não encontrada" });
            }

            var payments = await _unitOfWork.Payments.GetByCompanyIdAsync(user.CompanyId.Value);

            if (status.HasValue)
            {
                payments = payments.Where(p => p.Status == status.Value);
            }

            if (contractId.HasValue)
            {
                payments = payments.Where(p => p.ContractId == contractId.Value);
            }

            var result = payments.Select(p => new
            {
                Id = p.Id,
                ContratoId = p.ContractId,
                TituloContrato = p.Contract.Title,
                Valor = p.Amount,
                Metodo = p.Method.ToString(),
                Status = p.Status.ToString(),
                DataPagamento = p.PaymentDate,
                CriadoEm = p.CreatedAt,
                EmpresaCliente = new { Id = p.Contract.Client.Id, Nome = p.Contract.Client.Name },
                EmpresaFornecedora = new { Id = p.Contract.Provider.Id, Nome = p.Contract.Provider.Name },
                EhAReceber = p.Contract.ProviderId == user.CompanyId,
                EhAPagar = p.Contract.ClientId == user.CompanyId
            }).OrderByDescending(p => p.CriadoEm);

            return Ok(new
            {
                TotalPagamentos = result.Count(),
                ValorTotal = payments.Sum(p => p.Amount),
                FiltradoPor = new
                {
                    Status = status?.ToString() ?? "Todos",
                    ContratoId = contractId?.ToString() ?? "Todos"
                },
                Pagamentos = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pagamentos");
            return BadRequest(new { message = "Erro ao buscar pagamentos" });
        }
    }

    /// <summary>
    /// Obter detalhes de um pagamento específico
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário ou empresa não encontrada" });
            }

            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Pagamento não encontrado" });
            }

            // Verificar se o usuário tem acesso ao pagamento
            if (payment.Contract.ClientId != user.CompanyId && payment.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este pagamento");
            }

            var result = new
            {
                Id = payment.Id,
                ContratoId = payment.ContractId,
                Contrato = new
                {
                    Id = payment.Contract.Id,
                    Titulo = payment.Contract.Title,
                    ValorTotal = payment.Contract.ValueTotal,
                    EmpresaCliente = new { Id = payment.Contract.Client.Id, Nome = payment.Contract.Client.Name, Cnpj = payment.Contract.Client.Cnpj },
                    EmpresaFornecedora = new { Id = payment.Contract.Provider.Id, Nome = payment.Contract.Provider.Name, Cnpj = payment.Contract.Provider.Cnpj }
                },
                Valor = payment.Amount,
                Metodo = payment.Method.ToString(),
                Status = payment.Status.ToString(),
                DataPagamento = payment.PaymentDate,
                CriadoEm = payment.CreatedAt,
                AtualizadoEm = payment.UpdatedAt,
                EhAReceber = payment.Contract.ProviderId == user.CompanyId,
                EhAPagar = payment.Contract.ClientId == user.CompanyId
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pagamento {PaymentId}", id);
            return BadRequest(new { message = "Erro ao buscar pagamento" });
        }
    }

    /// <summary>
    /// Criar um novo pagamento
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário ou empresa não encontrada" });
            }

            var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContratoId);
            if (contract == null)
            {
                return NotFound(new { message = "Contrato não encontrado" });
            }

            // Verificar se o usuário tem acesso ao contrato
            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este contrato");
            }

            // Apenas a empresa cliente pode criar pagamentos
            if (contract.ClientId != user.CompanyId)
            {
                return Forbid("Apenas a empresa contratante pode criar pagamentos");
            }

            // Verificar se o contrato está ativo
            if (contract.Status != ContractStatus.Active)
            {
                return BadRequest(new { message = "O contrato deve estar ativo para receber pagamentos" });
            }

            var payment = new Payment(request.ContratoId, request.Valor, request.Metodo);

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pagamento {PaymentId} criado para o contrato {ContractId}", payment.Id, request.ContratoId);

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, new
            {
                Id = payment.Id,
                ContratoId = payment.ContractId,
                Valor = payment.Amount,
                Metodo = payment.Method.ToString(),
                Status = payment.Status.ToString(),
                CriadoEm = payment.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pagamento");
            return BadRequest(new { message = "Erro ao criar pagamento" });
        }
    }

    /// <summary>
    /// Obter resumo financeiro dos pagamentos
    /// </summary>
    [HttpGet("resumo-financeiro")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> GetResumoFinanceiro()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário ou empresa não encontrada" });
            }

            var payments = await _unitOfWork.Payments.GetByCompanyIdAsync(user.CompanyId.Value);

            var pagamentosAReceber = payments.Where(p => p.Contract.ProviderId == user.CompanyId);
            var pagamentosAPagar = payments.Where(p => p.Contract.ClientId == user.CompanyId);

            var resumo = new
            {
                PagamentosAReceber = new
                {
                    Total = pagamentosAReceber.Sum(p => p.Amount),
                    Pendentes = pagamentosAReceber.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                    Concluidos = pagamentosAReceber.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                    Quantidade = pagamentosAReceber.Count()
                },
                PagamentosAPagar = new
                {
                    Total = pagamentosAPagar.Sum(p => p.Amount),
                    Pendentes = pagamentosAPagar.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount),
                    Concluidos = pagamentosAPagar.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                    Quantidade = pagamentosAPagar.Count()
                },
                SaldoLiquido = pagamentosAReceber.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount) -
                              pagamentosAPagar.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount)
            };

            return Ok(resumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resumo financeiro");
            return BadRequest(new { message = "Erro ao buscar resumo financeiro" });
        }
    }

    /// <summary>
    /// Processar um pagamento (marcar como pago)
    /// </summary>
    [HttpPut("{id:guid}/processar")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> ProcessarPagamento(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário ou empresa não encontrada" });
            }

            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Pagamento não encontrado" });
            }

            // Verificar se o usuário tem acesso ao pagamento
            if (payment.Contract.ClientId != user.CompanyId && payment.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este pagamento");
            }

            // Apenas a empresa cliente pode processar pagamentos
            if (payment.Contract.ClientId != user.CompanyId)
            {
                return Forbid("Apenas a empresa contratante pode processar pagamentos");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                return BadRequest(new { message = "Apenas pagamentos pendentes podem ser processados" });
            }

            payment.ProcessPayment();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pagamento {PaymentId} processado pela empresa {CompanyId}", id, user.CompanyId);

            return Ok(new
            {
                mensagem = "Pagamento processado com sucesso",
                pagamentoId = id,
                status = payment.Status.ToString(),
                dataPagamento = payment.PaymentDate,
                atualizadoEm = payment.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento {PaymentId}", id);
            return BadRequest(new { message = "Erro ao processar pagamento" });
        }
    }

    /// <summary>
    /// Cancelar um pagamento
    /// </summary>
    [HttpPut("{id:guid}/cancelar")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> CancelarPagamento(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário ou empresa não encontrada" });
            }

            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Pagamento não encontrado" });
            }

            // Verificar se o usuário tem acesso ao pagamento
            if (payment.Contract.ClientId != user.CompanyId && payment.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este pagamento");
            }

            // Apenas a empresa cliente pode cancelar pagamentos
            if (payment.Contract.ClientId != user.CompanyId)
            {
                return Forbid("Apenas a empresa contratante pode cancelar pagamentos");
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                return BadRequest(new { message = "Pagamentos já processados não podem ser cancelados" });
            }

            payment.CancelPayment();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pagamento {PaymentId} cancelado pela empresa {CompanyId}", id, user.CompanyId);

            return Ok(new
            {
                mensagem = "Pagamento cancelado com sucesso",
                pagamentoId = id,
                status = payment.Status.ToString(),
                atualizadoEm = payment.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar pagamento {PaymentId}", id);
            return BadRequest(new { message = "Erro ao cancelar pagamento" });
        }
    }
}

public class CreatePaymentRequest
{
    public Guid ContratoId { get; set; }
    public decimal Valor { get; set; }
    public PaymentMethod Metodo { get; set; }
}