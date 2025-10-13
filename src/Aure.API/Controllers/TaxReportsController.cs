using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaxReportsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaxReportsController> _logger;

    public TaxReportsController(IUnitOfWork unitOfWork, ILogger<TaxReportsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obter relatório de impostos por período
    /// </summary>
    [HttpGet("impostos")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetRelatorioImpostos(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? taxType = null)
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

            var taxCalculations = await _unitOfWork.TaxCalculations.GetByDateRangeAsync(startDate, endDate);
            var companyTaxes = taxCalculations.Where(tc => 
                tc.Invoice.Contract.ClientId == user.CompanyId || 
                tc.Invoice.Contract.ProviderId == user.CompanyId);

            if (!string.IsNullOrEmpty(taxType) && Enum.TryParse<TaxType>(taxType, true, out var taxTypeEnum))
            {
                companyTaxes = companyTaxes.Where(tc => tc.TaxType == taxTypeEnum);
            }

            // Resumo por tipo de imposto
            var taxSummary = companyTaxes
                .GroupBy(tc => tc.TaxType)
                .Select(g => new
                {
                    TipoImposto = g.Key.ToString(),
                    TotalCalculos = g.Count(),
                    BaseCalculoTotal = g.Sum(tc => tc.TaxBase),
                    ValorImpostoTotal = g.Sum(tc => tc.TaxAmount),
                    AliquotaMedia = g.Average(tc => tc.TaxRate)
                });

            // Resumo por mês
            var monthlySummary = companyTaxes
                .GroupBy(tc => new { tc.CreatedAt.Year, tc.CreatedAt.Month })
                .Select(g => new
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    TotalCalculos = g.Count(),
                    BaseCalculoTotal = g.Sum(tc => tc.TaxBase),
                    ValorImpostoTotal = g.Sum(tc => tc.TaxAmount)
                })
                .OrderBy(m => m.Ano).ThenBy(m => m.Mes);

            // Detalhes dos cálculos
            var taxDetails = companyTaxes.Select(tc => new
            {
                Id = tc.Id,
                NotaFiscalId = tc.InvoiceId,
                NumeroNota = tc.Invoice.InvoiceNumber,
                TipoImposto = tc.TaxType.ToString(),
                Aliquota = tc.TaxRate,
                BaseCalculo = tc.TaxBase,
                ValorImposto = tc.TaxAmount,
                DataCalculo = tc.CreatedAt
            });

            return Ok(new
            {
                Periodo = new
                {
                    Inicio = startDate,
                    Fim = endDate
                },
                ResumoGeral = new
                {
                    TotalCalculos = companyTaxes.Count(),
                    BaseCalculoTotal = companyTaxes.Sum(tc => tc.TaxBase),
                    ValorImpostoTotal = companyTaxes.Sum(tc => tc.TaxAmount)
                },
                ResumoPorTipo = taxSummary,
                ResumoPorMes = monthlySummary,
                DetalhesCalculos = taxDetails
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de impostos");
            return BadRequest(new { mensagem = "Erro ao gerar relatório de impostos" });
        }
    }

    /// <summary>
    /// Obter livro de registro de saídas
    /// </summary>
    [HttpGet("livro-saidas")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetLivroSaidas(
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

            var invoices = await _unitOfWork.Invoices.GetByCompanyIdAsync(user.CompanyId.Value);
            var periodInvoices = invoices.Where(i => 
                i.IssueDate >= startDate && 
                i.IssueDate <= endDate &&
                i.Status == InvoiceStatus.Issued);

            var outputBook = periodInvoices.Select(invoice => new
            {
                DataEmissao = invoice.IssueDate,
                NumeroNota = invoice.InvoiceNumber,
                Serie = invoice.Series,
                ChaveAcesso = invoice.AccessKey,
                ValorTotal = invoice.TotalAmount,
                ValorImpostos = invoice.TaxAmount,
                BaseCalculo = invoice.TotalAmount - invoice.TaxAmount,
                Status = invoice.Status.ToString(),
                ProtocoloSefaz = invoice.SefazProtocol,
                ContratoId = invoice.ContractId,
                Impostos = invoice.TaxCalculations.Select(tc => new
                {
                    Tipo = tc.TaxType.ToString(),
                    Aliquota = tc.TaxRate,
                    BaseCalculo = tc.TaxBase,
                    Valor = tc.TaxAmount
                })
            }).OrderBy(i => i.DataEmissao);

            var summary = new
            {
                TotalNotas = outputBook.Count(),
                ValorTotalFaturado = outputBook.Sum(i => i.ValorTotal),
                ValorTotalImpostos = outputBook.Sum(i => i.ValorImpostos),
                BaseCalculoTotal = outputBook.Sum(i => i.BaseCalculo)
            };

            return Ok(new
            {
                Periodo = new
                {
                    Inicio = startDate,
                    Fim = endDate
                },
                Resumo = summary,
                RegistrosSaidas = outputBook
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar livro de saídas");
            return BadRequest(new { mensagem = "Erro ao gerar livro de saídas" });
        }
    }

    /// <summary>
    /// Obter dados para SPED Fiscal
    /// </summary>
    [HttpGet("sped-fiscal")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetSpedFiscalData(
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

            var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
            if (company == null)
            {
                return NotFound(new { mensagem = "Empresa não encontrada" });
            }

            var invoices = await _unitOfWork.Invoices.GetByCompanyIdAsync(user.CompanyId.Value);
            var periodInvoices = invoices.Where(i => 
                i.IssueDate >= startDate && 
                i.IssueDate <= endDate &&
                i.Status == InvoiceStatus.Issued);

            // Registro 0000 - Abertura do arquivo digital
            var reg0000 = new
            {
                Registro = "0000",
                CodVer = "017", // Versão do layout
                CodFin = "0", // Finalidade do arquivo (0 = Remessa original)
                DtIni = startDate,
                DtFin = endDate,
                NomeEmpresa = company.Name,
                Cnpj = company.Cnpj,
                IeUf = "", // Inscrição estadual
                CodMun = "", // Código do município
                Im = "", // Inscrição municipal
                Suframa = "" // Inscrição SUFRAMA
            };

            // Registro C100 - Documento fiscal
            var regC100 = periodInvoices.Select(invoice => new
            {
                Registro = "C100",
                IndOper = "1", // Saída
                IndEmit = "0", // Emissão própria
                CodPart = "", // Código do participante
                CodMod = "55", // Modelo NFe
                CodSit = "00", // Situação do documento
                Ser = invoice.Series,
                NumDoc = invoice.InvoiceNumber,
                ChvNfe = invoice.AccessKey,
                DtDoc = invoice.IssueDate,
                DtES = invoice.IssueDate,
                VlDoc = invoice.TotalAmount,
                IndPgto = "0", // À vista
                VlDesc = 0,
                VlAbatNt = 0,
                VlMerc = invoice.TotalAmount - invoice.TaxAmount,
                IndFrt = "9", // Sem frete
                VlFrt = 0,
                VlSeg = 0,
                VlOutDa = 0,
                VlBcIcms = invoice.TaxCalculations.Where(tc => tc.TaxType == TaxType.ICMS).Sum(tc => tc.TaxBase),
                VlIcms = invoice.TaxCalculations.Where(tc => tc.TaxType == TaxType.ICMS).Sum(tc => tc.TaxAmount),
                VlBcIcmsSt = 0,
                VlIcmsSt = 0,
                VlIpi = invoice.TaxCalculations.Where(tc => tc.TaxType == TaxType.IPI).Sum(tc => tc.TaxAmount),
                VlPis = invoice.TaxCalculations.Where(tc => tc.TaxType == TaxType.PIS).Sum(tc => tc.TaxAmount),
                VlCofins = invoice.TaxCalculations.Where(tc => tc.TaxType == TaxType.COFINS).Sum(tc => tc.TaxAmount),
                VlPisSt = 0,
                VlCofinsSt = 0
            });

            return Ok(new
            {
                Periodo = new
                {
                    Inicio = startDate,
                    Fim = endDate
                },
                EmpresaInfo = reg0000,
                DocumentosFiscais = regC100,
                TotalRegistros = regC100.Count(),
                ObservacaoArquivo = "Dados preparados para geração do arquivo SPED Fiscal. Consulte um contador para validação final."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar dados SPED Fiscal");
            return BadRequest(new { mensagem = "Erro ao gerar dados SPED Fiscal" });
        }
    }

    /// <summary>
    /// Obter conciliação contábil
    /// </summary>
    [HttpGet("conciliacao-contabil")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetConciliacaoContabil(
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

            // Dados fiscais (notas fiscais)
            var invoices = await _unitOfWork.Invoices.GetByCompanyIdAsync(user.CompanyId.Value);
            var periodInvoices = invoices.Where(i => 
                i.IssueDate >= startDate && 
                i.IssueDate <= endDate);

            // Dados contábeis (ledger)
            var ledgerEntries = await _unitOfWork.LedgerEntries.GetByDateRangeAsync(startDate, endDate);
            var companyLedger = ledgerEntries.Where(le => 
                le.Contract.ClientId == user.CompanyId || 
                le.Contract.ProviderId == user.CompanyId);

            // Dados de pagamentos
            var payments = await _unitOfWork.Payments.GetByCompanyIdAsync(user.CompanyId.Value);
            var periodPayments = payments.Where(p => 
                p.PaymentDate.HasValue && 
                p.PaymentDate >= startDate && 
                p.PaymentDate <= endDate);

            var reconciliation = new
            {
                ResumoFiscal = new
                {
                    TotalNotasEmitidas = periodInvoices.Count(),
                    ValorTotalFiscal = periodInvoices.Sum(i => i.TotalAmount),
                    ValorTotalImpostos = periodInvoices.Sum(i => i.TaxAmount)
                },
                ResumoContabil = new
                {
                    TotalLancamentos = companyLedger.Count(),
                    TotalDebitos = companyLedger.Sum(le => le.Debit),
                    TotalCreditos = companyLedger.Sum(le => le.Credit),
                    SaldoContabil = companyLedger.Sum(le => le.Credit) - companyLedger.Sum(le => le.Debit)
                },
                ResumoFinanceiro = new
                {
                    TotalPagamentos = periodPayments.Count(),
                    ValorTotalPagamentos = periodPayments.Sum(p => p.Amount)
                },
                Diferencas = new
                {
                    DiferencaFiscalContabil = periodInvoices.Sum(i => i.TotalAmount) - (companyLedger.Sum(le => le.Credit) - companyLedger.Sum(le => le.Debit)),
                    DiferencaContabilFinanceira = (companyLedger.Sum(le => le.Credit) - companyLedger.Sum(le => le.Debit)) - periodPayments.Sum(p => p.Amount)
                }
            };

            return Ok(new
            {
                Periodo = new
                {
                    Inicio = startDate,
                    Fim = endDate
                },
                ConciliacaoContabil = reconciliation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar conciliação contábil");
            return BadRequest(new { mensagem = "Erro ao gerar conciliação contábil" });
        }
    }
}