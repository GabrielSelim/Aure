using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Application.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISefazService _sefazService;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IUnitOfWork unitOfWork, 
        ISefazService sefazService,
        ILogger<InvoicesController> logger)
    {
        _unitOfWork = unitOfWork;
        _sefazService = sefazService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todas as notas fiscais da empresa do usuário atual
    /// </summary>
    [HttpGet]
    [HttpGet("listar")]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceStatus? status = null, [FromQuery] Guid? contractId = null)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoices = await _unitOfWork.Invoices.GetByCompanyIdAsync(user.CompanyId.Value);

            if (status.HasValue)
            {
                invoices = invoices.Where(i => i.Status == status.Value);
            }

            if (contractId.HasValue)
            {
                invoices = invoices.Where(i => i.ContractId == contractId.Value);
            }

            var result = invoices.Select(i => new
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Series = i.Series,
                AccessKey = i.AccessKey,
                ContractId = i.ContractId,
                ContractTitle = i.Contract.Title,
                PaymentId = i.PaymentId,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                TaxAmount = i.TaxAmount,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                ClientCompany = new { i.Contract.Client.Id, i.Contract.Client.Name, i.Contract.Client.Cnpj },
                ProviderCompany = new { i.Contract.Provider.Id, i.Contract.Provider.Name, i.Contract.Provider.Cnpj },
                IsIssuer = i.Contract.ProviderId == user.CompanyId,
                IsRecipient = i.Contract.ClientId == user.CompanyId
            }).OrderByDescending(i => i.CreatedAt);

            return Ok(new
            {
                TotalInvoices = result.Count(),
                TotalAmount = invoices.Sum(i => i.TotalAmount),
                FilteredBy = new
                {
                    Status = status?.ToString() ?? "All",
                    ContractId = contractId?.ToString() ?? "All"
                },
                Invoices = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notas fiscais");
            return BadRequest(new { mensagem = "Erro ao buscar notas fiscais" });
        }
    }

    /// <summary>
    /// Obter detalhes de uma nota fiscal específica
    /// </summary>
    [HttpGet("{id:guid}")]
    [HttpGet("detalhes/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            var result = new
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Series = invoice.Series,
                AccessKey = invoice.AccessKey,
                ContractId = invoice.ContractId,
                Contract = new
                {
                    Id = invoice.Contract.Id,
                    Title = invoice.Contract.Title,
                    ValueTotal = invoice.Contract.ValueTotal,
                    ClientCompany = new { invoice.Contract.Client.Id, invoice.Contract.Client.Name, invoice.Contract.Client.Cnpj },
                    ProviderCompany = new { invoice.Contract.Provider.Id, invoice.Contract.Provider.Name, invoice.Contract.Provider.Cnpj }
                },
                PaymentId = invoice.PaymentId,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                TaxAmount = invoice.TaxAmount,
                Status = invoice.Status.ToString(),
                XmlContent = invoice.XmlContent,
                PdfUrl = invoice.PdfUrl,
                CancellationReason = invoice.CancellationReason,
                SefazProtocol = invoice.SefazProtocol,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                Items = invoice.Items.Select(item => new
                {
                    Id = item.Id,
                    ItemSequence = item.ItemSequence,
                    Description = item.Description,
                    NcmCode = item.NcmCode,
                    Quantity = item.Quantity,
                    UnitValue = item.UnitValue,
                    TotalValue = item.TotalValue,
                    TaxClassification = item.TaxClassification
                }),
                TaxCalculations = invoice.TaxCalculations.Select(tax => new
                {
                    Id = tax.Id,
                    TaxType = tax.TaxType.ToString(),
                    TaxRate = tax.TaxRate,
                    TaxBase = tax.TaxBase,
                    TaxAmount = tax.TaxAmount
                }),
                IsIssuer = invoice.Contract.ProviderId == user.CompanyId,
                IsRecipient = invoice.Contract.ClientId == user.CompanyId
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar nota fiscal {InvoiceId}", id);
            return BadRequest(new { mensagem = "Erro ao buscar nota fiscal" });
        }
    }

    /// <summary>
    /// Criar uma nova nota fiscal
    /// </summary>
    [HttpPost]
    [HttpPost("criar")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractId);
            if (contract == null)
            {
                return NotFound(new { mensagem = "Contrato não encontrado" });
            }

            // Verificar se o usuário tem acesso ao contrato
            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este contrato");
            }

            // Apenas a empresa provedora pode emitir notas fiscais
            if (contract.ProviderId != user.CompanyId)
            {
                return Forbid("Apenas a empresa prestadora de serviços pode emitir notas fiscais");
            }

            // Verificar se o contrato está ativo
            if (contract.Status != ContractStatus.Active)
            {
                return BadRequest(new { mensagem = "O contrato deve estar ativo para emitir notas fiscais" });
            }

            // Gerar próximo número da nota fiscal
            var nextInvoiceNumber = await _unitOfWork.Invoices.GetNextInvoiceNumberAsync(request.Series);

            var invoice = new Invoice(
                request.ContractId,
                request.PaymentId,
                nextInvoiceNumber,
                request.Series,
                request.DueDate,
                request.TotalAmount,
                request.TaxAmount
            );

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {InvoiceId} criada para o contrato {ContractId}", invoice.Id, request.ContractId);

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, new
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Series = invoice.Series,
                AccessKey = invoice.AccessKey,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status.ToString(),
                CreatedAt = invoice.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar nota fiscal");
            return BadRequest(new { mensagem = "Erro ao criar nota fiscal" });
        }
    }

    /// <summary>
    /// Emitir uma nota fiscal (enviar para SEFAZ)
    /// </summary>
    [HttpPut("{id:guid}/emitir")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    public async Task<IActionResult> IssueInvoice(Guid id)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            // Apenas a empresa provedora pode emitir notas fiscais
            if (invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Apenas a empresa prestadora de serviços pode emitir notas fiscais");
            }

            if (invoice.Status != InvoiceStatus.Draft)
            {
                return BadRequest(new { mensagem = "Apenas notas fiscais em rascunho podem ser emitidas" });
            }

            // Aqui seria implementada a integração com SEFAZ
            // Por enquanto, simularemos a emissão
            invoice.IssueInvoice("PROTOCOLO_SIMULADO_" + DateTime.UtcNow.Ticks);
            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {InvoiceId} emitida pela empresa {CompanyId}", id, user.CompanyId);

            return Ok(new
            {
                mensagem = "Nota fiscal emitida com sucesso",
                invoiceId = id,
                status = invoice.Status.ToString(),
                accessKey = invoice.AccessKey,
                sefazProtocol = invoice.SefazProtocol,
                issueDate = invoice.IssueDate,
                updatedAt = invoice.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir nota fiscal {InvoiceId}", id);
            return BadRequest(new { mensagem = "Erro ao emitir nota fiscal" });
        }
    }

    /// <summary>
    /// Cancelar uma nota fiscal
    /// </summary>
    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    public async Task<IActionResult> CancelInvoice(Guid id, [FromBody] CancelInvoiceRequest request)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            // Apenas a empresa provedora pode cancelar notas fiscais
            if (invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Apenas a empresa prestadora de serviços pode cancelar notas fiscais");
            }

            if (invoice.Status != InvoiceStatus.Issued && invoice.Status != InvoiceStatus.Sent)
            {
                return BadRequest(new { mensagem = "Apenas notas fiscais emitidas ou enviadas podem ser canceladas" });
            }

            // Aqui seria implementada a integração com SEFAZ para cancelamento
            // Por enquanto, simularemos o cancelamento
            invoice.CancelInvoice(request.Reason);
            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Nota fiscal {InvoiceId} cancelada pela empresa {CompanyId} - Motivo: {Reason}", id, user.CompanyId, request.Reason);

            return Ok(new
            {
                mensagem = "Nota fiscal cancelada com sucesso",
                invoiceId = id,
                status = invoice.Status.ToString(),
                cancellationReason = invoice.CancellationReason,
                updatedAt = invoice.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar nota fiscal {InvoiceId}", id);
            return BadRequest(new { mensagem = "Erro ao cancelar nota fiscal" });
        }
    }

    /// <summary>
    /// Obter XML da nota fiscal
    /// </summary>
    [HttpGet("{id:guid}/xml-nota")]
    public async Task<IActionResult> GetXmlNota(Guid id)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            if (string.IsNullOrEmpty(invoice.XmlContent))
            {
                return NotFound(new { mensagem = "XML da nota fiscal não encontrado" });
            }

            return File(System.Text.Encoding.UTF8.GetBytes(invoice.XmlContent), "application/xml", $"NFe_{invoice.InvoiceNumber}.xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter XML da nota fiscal {InvoiceId}", id);
            return BadRequest(new { mensagem = "Erro ao obter XML da nota fiscal" });
        }
    }

    /// <summary>
    /// Emitir nota fiscal via SEFAZ
    /// </summary>
    [HttpPost("{id:guid}/emitir-sefaz")]
    public async Task<IActionResult> IssueInvoiceToSefaz(Guid id)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            if (invoice.Status != InvoiceStatus.Draft)
            {
                return BadRequest(new { mensagem = "Nota fiscal deve estar em status de rascunho para ser emitida" });
            }

            // Emitir na SEFAZ
            var sefazResponse = await _sefazService.IssueInvoiceAsync(invoice);

            if (sefazResponse.Success)
            {
                // Atualizar status da nota fiscal usando método da entidade
                invoice.IssueInvoice(sefazResponse.Protocol ?? "");

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Nota fiscal {InvoiceId} emitida com sucesso na SEFAZ. Protocolo: {Protocol}", 
                    id, sefazResponse.Protocol);

                return Ok(new
                {
                    mensagem = "Nota fiscal emitida com sucesso na SEFAZ",
                    protocolo = sefazResponse.Protocol,
                    chaveAcesso = invoice.AccessKey,
                    processadoEm = sefazResponse.ProcessedAt,
                    invoice = new
                    {
                        Id = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        Series = invoice.Series,
                        Status = invoice.Status.ToString(),
                        AccessKey = invoice.AccessKey,
                        SefazProtocol = invoice.SefazProtocol
                    }
                });
            }
            else
            {
                // Atualizar status para erro usando método da entidade
                invoice.MarkAsError();
                await _unitOfWork.SaveChangesAsync();

                _logger.LogError("Erro ao emitir nota fiscal {InvoiceId} na SEFAZ: {ErrorMessage}", 
                    id, sefazResponse.Message);

                return BadRequest(new
                {
                    mensagem = "Erro ao emitir nota fiscal na SEFAZ",
                    erro = sefazResponse.Message,
                    codigoErro = sefazResponse.ErrorCode
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao emitir nota fiscal {InvoiceId} na SEFAZ", id);
            return BadRequest(new { mensagem = "Erro inesperado ao emitir nota fiscal na SEFAZ" });
        }
    }

    /// <summary>
    /// Cancelar nota fiscal via SEFAZ
    /// </summary>
    [HttpPost("{id:guid}/cancelar-sefaz")]
    public async Task<IActionResult> CancelInvoiceInSefaz(Guid id, [FromBody] CancelInvoiceRequest request)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            if (invoice.Status != InvoiceStatus.Issued)
            {
                return BadRequest(new { mensagem = "Nota fiscal deve estar emitida para ser cancelada" });
            }

            if (string.IsNullOrEmpty(invoice.AccessKey))
            {
                return BadRequest(new { mensagem = "Chave de acesso não encontrada" });
            }

            // Cancelar na SEFAZ
            var sefazResponse = await _sefazService.CancelInvoiceAsync(invoice.AccessKey, request.Reason);

            if (sefazResponse.Success)
            {
                // Atualizar status da nota fiscal usando método da entidade
                invoice.CancelInvoice(request.Reason);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Nota fiscal {InvoiceId} cancelada com sucesso na SEFAZ", id);

                return Ok(new
                {
                    mensagem = "Nota fiscal cancelada com sucesso na SEFAZ",
                    protocolo = sefazResponse.Protocol,
                    processadoEm = sefazResponse.ProcessedAt,
                    justificativa = request.Reason
                });
            }
            else
            {
                _logger.LogError("Erro ao cancelar nota fiscal {InvoiceId} na SEFAZ: {ErrorMessage}", 
                    id, sefazResponse.Message);

                return BadRequest(new
                {
                    mensagem = "Erro ao cancelar nota fiscal na SEFAZ",
                    erro = sefazResponse.Message,
                    codigoErro = sefazResponse.ErrorCode
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao cancelar nota fiscal {InvoiceId} na SEFAZ", id);
            return BadRequest(new { mensagem = "Erro inesperado ao cancelar nota fiscal na SEFAZ" });
        }
    }

    /// <summary>
    /// Consultar status da nota fiscal na SEFAZ
    /// </summary>
    [HttpGet("{id:guid}/status-sefaz")]
    public async Task<IActionResult> CheckInvoiceStatusInSefaz(Guid id)
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
                return NotFound(new { mensagem = "Usuário ou empresa não encontrada" });
            }

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new { mensagem = "Nota fiscal não encontrada" });
            }

            // Verificar se o usuário tem acesso à nota fiscal
            if (invoice.Contract.ClientId != user.CompanyId && invoice.Contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a esta nota fiscal");
            }

            if (string.IsNullOrEmpty(invoice.AccessKey))
            {
                return BadRequest(new { mensagem = "Chave de acesso não encontrada" });
            }

            // Consultar status na SEFAZ
            var statusResponse = await _sefazService.CheckStatusAsync(invoice.AccessKey);

            return Ok(new
            {
                chaveAcesso = invoice.AccessKey,
                statusSefaz = statusResponse.Status,
                protocolo = statusResponse.Protocol,
                mensagem = statusResponse.Message,
                sucesso = statusResponse.Success,
                ultimaAtualizacao = statusResponse.LastUpdate,
                invoice = new
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    Series = invoice.Series,
                    Status = invoice.Status.ToString(),
                    TotalAmount = invoice.TotalAmount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da nota fiscal {InvoiceId} na SEFAZ", id);
            return BadRequest(new { mensagem = "Erro ao consultar status da nota fiscal na SEFAZ" });
        }
    }

    /// <summary>
    /// Validar certificado digital SEFAZ
    /// </summary>
    [HttpGet("validar-certificado-sefaz")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ValidateSefazCertificate()
    {
        try
        {
            var isValid = await _sefazService.ValidateCertificateAsync();

            return Ok(new
            {
                certificadoValido = isValid,
                mensagem = isValid ? "Certificado digital válido" : "Certificado digital inválido ou expirado",
                verificadoEm = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar certificado SEFAZ");
            return BadRequest(new { mensagem = "Erro ao validar certificado SEFAZ" });
        }
    }
}

public class CreateInvoiceRequest
{
    public Guid ContractId { get; set; }
    public Guid? PaymentId { get; set; }
    public string Series { get; set; } = "1";
    public DateTime? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public class CancelInvoiceRequest
{
    public string Reason { get; set; } = string.Empty;
}
