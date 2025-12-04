using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Application.Interfaces;
using Aure.Application.DTOs.User;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IUnitOfWork unitOfWork, 
        INotificationService notificationService,
        IUserService userService,
        ILogger<ContractsController> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os contratos da empresa do usuário atual
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContracts([FromQuery] ContractStatus? status = null)
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

            var contracts = await _unitOfWork.Contracts.GetByCompanyIdAsync(user.CompanyId.Value);

            if (status.HasValue)
            {
                contracts = contracts.Where(c => c.Status == status.Value);
            }

            var result = contracts.Select(c => new
            {
                Id = c.Id,
                Titulo = c.Title,
                ValorTotal = c.ValueTotal,
                Status = c.Status.ToString(),
                CriadoEm = c.CreatedAt,
                AtualizadoEm = c.UpdatedAt,
                EmpresaCliente = new { Id = c.Client.Id, Nome = c.Client.Name, Cnpj = c.Client.Cnpj },
                EmpresaFornecedora = new { Id = c.Provider.Id, Nome = c.Provider.Name, Cnpj = c.Provider.Cnpj },
                EhCliente = c.ClientId == user.CompanyId,
                EhFornecedor = c.ProviderId == user.CompanyId
            });

            return Ok(new
            {
                TotalContratos = result.Count(),
                FiltradoPor = status?.ToString() ?? "Todos",
                Contratos = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contratos");
            return BadRequest(new { message = "Erro ao buscar contratos" });
        }
    }

    /// <summary>
    /// Obter valores a pagar este mês baseado nos contratos ativos
    /// </summary>
    [HttpGet("pagamentos-mensais")]
    public async Task<IActionResult> GetPagamentosMensais()
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

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            // Buscar contratos ativos onde a empresa é cliente (vai pagar)
            var activeContracts = await _unitOfWork.Contracts.GetByClientIdAsync(user.CompanyId.Value);
            activeContracts = activeContracts.Where(c => 
                c.Status == ContractStatus.Active && 
                !c.IsExpired &&
                c.MonthlyValue.HasValue &&
                c.StartDate <= DateTime.UtcNow &&
                (c.ExpirationDate == null || c.ExpirationDate >= DateTime.UtcNow)
            );

            var monthlyPayments = activeContracts.Select(c => new
            {
                ContractId = c.Id,
                Title = c.Title,
                ProviderCompany = new { c.Provider.Id, c.Provider.Name, c.Provider.Cnpj },
                MonthlyValue = c.MonthlyValue!.Value,
                StartDate = c.StartDate,
                ExpirationDate = c.ExpirationDate,
                DaysUntilExpiration = c.ExpirationDate.HasValue 
                    ? (c.ExpirationDate.Value - DateTime.UtcNow).Days 
                    : (int?)null
            });

            var totalToPay = monthlyPayments.Sum(p => p.MonthlyValue);

            return Ok(new
            {
                Mes = currentMonth,
                Ano = currentYear,
                NomeMes = new DateTime(currentYear, currentMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-BR")),
                TotalAPagar = totalToPay,
                QuantidadeContratos = monthlyPayments.Count(),
                Contratos = monthlyPayments.Select(p => new
                {
                    ContratoId = p.ContractId,
                    Titulo = p.Title,
                    EmpresaFornecedora = new 
                    { 
                        Id = p.ProviderCompany.Id, 
                        Nome = p.ProviderCompany.Name, 
                        Cnpj = p.ProviderCompany.Cnpj 
                    },
                    ValorMensal = p.MonthlyValue,
                    DataInicio = p.StartDate,
                    DataVencimento = p.ExpirationDate,
                    DiasAteVencimento = p.DaysUntilExpiration
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pagamentos mensais");
            return BadRequest(new { message = "Erro ao buscar pagamentos mensais" });
        }
    }

    /// <summary>
    /// Obter valores a receber este mês baseado nos contratos ativos (como provedor)
    /// </summary>
    [HttpGet("receitas-mensais")]
    public async Task<IActionResult> GetReceitasMensais()
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

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            // Buscar contratos ativos onde a empresa é provedora (vai receber)
            var activeContracts = await _unitOfWork.Contracts.GetByProviderIdAsync(user.CompanyId.Value);
            activeContracts = activeContracts.Where(c => 
                c.Status == ContractStatus.Active && 
                !c.IsExpired &&
                c.MonthlyValue.HasValue &&
                c.StartDate <= DateTime.UtcNow &&
                (c.ExpirationDate == null || c.ExpirationDate >= DateTime.UtcNow)
            );

            var monthlyIncome = activeContracts.Select(c => new
            {
                ContractId = c.Id,
                Title = c.Title,
                ClientCompany = new { c.Client.Id, c.Client.Name, c.Client.Cnpj },
                MonthlyValue = c.MonthlyValue!.Value,
                StartDate = c.StartDate,
                ExpirationDate = c.ExpirationDate,
                DaysUntilExpiration = c.ExpirationDate.HasValue 
                    ? (c.ExpirationDate.Value - DateTime.UtcNow).Days 
                    : (int?)null
            });

            var totalToReceive = monthlyIncome.Sum(p => p.MonthlyValue);

            return Ok(new
            {
                Mes = currentMonth,
                Ano = currentYear,
                NomeMes = new DateTime(currentYear, currentMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-BR")),
                TotalAReceber = totalToReceive,
                QuantidadeContratos = monthlyIncome.Count(),
                Contratos = monthlyIncome.Select(r => new
                {
                    ContratoId = r.ContractId,
                    Titulo = r.Title,
                    EmpresaCliente = new 
                    { 
                        Id = r.ClientCompany.Id, 
                        Nome = r.ClientCompany.Name, 
                        Cnpj = r.ClientCompany.Cnpj 
                    },
                    ValorMensal = r.MonthlyValue,
                    DataInicio = r.StartDate,
                    DataVencimento = r.ExpirationDate,
                    DiasAteVencimento = r.DaysUntilExpiration
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar receitas mensais");
            return BadRequest(new { message = "Erro ao buscar receitas mensais" });
        }
    }

    /// <summary>
    /// Obter detalhes de um contrato específico
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContract(Guid id)
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

            var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound(new { message = "Contrato não encontrado" });
            }

            // Verificar se o usuário tem acesso ao contrato
            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este contrato");
            }

            var signatures = await _unitOfWork.Signatures.GetByContractIdAsync(id);
            var payments = await _unitOfWork.Payments.GetByContractIdAsync(id);
            var splitRules = await _unitOfWork.SplitRules.GetByContractIdAsync(id);

            var result = new
            {
                Id = contract.Id,
                Titulo = contract.Title,
                Descricao = contract.Description,
                ValorTotal = contract.ValueTotal,
                ValorMensal = contract.MonthlyValue,
                DataInicio = contract.StartDate,
                DataVencimento = contract.ExpirationDate,
                DataAssinatura = contract.SignedDate,
                Status = contract.Status.ToString(),
                EstaExpirado = contract.IsExpired,
                EstaAtivo = contract.IsActive,
                IpfsCid = contract.IpfsCid,
                HashSha256 = contract.Sha256Hash,
                CriadoEm = contract.CreatedAt,
                AtualizadoEm = contract.UpdatedAt,
                EmpresaCliente = new { Id = contract.Client.Id, Nome = contract.Client.Name, Cnpj = contract.Client.Cnpj },
                EmpresaFornecedora = new { Id = contract.Provider.Id, Nome = contract.Provider.Name, Cnpj = contract.Provider.Cnpj },
                Assinaturas = signatures.Select(s => new
                {
                    Id = s.Id,
                    UsuarioId = s.UserId,
                    NomeUsuario = s.User.Name,
                    AssinadoEm = s.SignedAt,
                    Metodo = s.Method.ToString(),
                    HashAssinatura = s.SignatureHash
                }),
                Pagamentos = payments.Select(p => new
                {
                    Id = p.Id,
                    Valor = p.Amount,
                    Metodo = p.Method.ToString(),
                    Status = p.Status.ToString(),
                    DataPagamento = p.PaymentDate,
                    CriadoEm = p.CreatedAt
                }),
                RegrasDivisao = splitRules.Select(sr => new
                {
                    Id = sr.Id,
                    RefBeneficiario = sr.BeneficiaryRef,
                    Percentual = sr.Percentage,
                    TaxaFixa = sr.FixedFee,
                    Prioridade = sr.Priority
                }),
                EhCliente = contract.ClientId == user.CompanyId,
                EhFornecedor = contract.ProviderId == user.CompanyId,
                PodeModificar = contract.CanBeModified(),
                TotalmenteAssinado = signatures.Count() >= 2
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contrato {ContractId}", id);
            return BadRequest(new { message = "Erro ao buscar contrato" });
        }
    }

    /// <summary>
    /// Criar um novo contrato
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
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

            // Verificar se a empresa provedor existe
            var providerCompany = await _unitOfWork.Companies.GetByIdAsync(request.ProviderId);
            if (providerCompany == null)
            {
                return NotFound(new { message = "Empresa provedora não encontrada" });
            }

            // Verificar se há relacionamento ativo entre as empresas
            var relationship = await _unitOfWork.CompanyRelationships.GetActiveRelationshipsByCompanyIdAsync(user.CompanyId.Value);
            var hasRelationship = relationship.Any(r => 
                (r.ClientCompanyId == user.CompanyId && r.ProviderCompanyId == request.ProviderId) ||
                (r.ProviderCompanyId == user.CompanyId && r.ClientCompanyId == request.ProviderId));

            if (!hasRelationship)
            {
                return BadRequest(new { message = "Não existe relacionamento ativo entre as empresas" });
            }

            // Gerar hash do contrato
            var contractHash = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{request.Title}{request.Description}{request.ValueTotal}")
            );
            var hashString = Convert.ToHexString(contractHash);

            var contract = new Contract(
                user.CompanyId.Value,
                request.ProviderId,
                request.Title,
                request.Description,
                request.ValueTotal,
                request.MonthlyValue,
                request.StartDate,
                request.ExpirationDate,
                hashString
            );

            await _unitOfWork.Contracts.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Contrato {ContractId} criado pela empresa {CompanyId}", contract.Id, user.CompanyId);

            // Enviar notificação para funcionário PJ sobre novo contrato
            _ = Task.Run(async () => await _notificationService.SendContractCreatedToPJAsync(contract.Id));

            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, new
            {
                Id = contract.Id,
                Title = contract.Title,
                ValueTotal = contract.ValueTotal,
                Status = contract.Status.ToString(),
                CreatedAt = contract.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar contrato");
            return BadRequest(new { message = "Erro ao criar contrato" });
        }
    }

    /// <summary>
    /// Assinar um contrato
    /// </summary>
    [HttpPost("{id:guid}/assinar")]
    public async Task<IActionResult> SignContract(Guid id, [FromBody] SignContractRequest request)
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

            var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound(new { message = "Contrato não encontrado" });
            }

            // Verificar se o usuário tem acesso ao contrato
            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid("Você não tem acesso a este contrato");
            }

            // Verificar se o usuário já assinou
            var existingSignature = await _unitOfWork.Signatures.GetByContractAndUserAsync(id, userId);
            if (existingSignature != null)
            {
                return BadRequest(new { message = "Usuário já assinou este contrato" });
            }

            var signature = new Signature(id, userId, request.Method, request.SignatureHash);

            await _unitOfWork.Signatures.AddAsync(signature);

            // Verificar se o contrato está totalmente assinado
            var isFullySigned = await _unitOfWork.Signatures.IsContractFullySignedAsync(id);
            if (isFullySigned)
            {
                contract.UpdateStatus(ContractStatus.Active);
                await _unitOfWork.Contracts.UpdateAsync(contract);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Contrato {ContractId} assinado pelo usuário {UserId}", id, userId);

            // Enviar notificação para gestores sobre contrato assinado
            if (isFullySigned)
            {
                _ = Task.Run(async () => await _notificationService.SendContractSignedToManagersAsync(id));
            }

            return Ok(new
            {
                message = "Contrato assinado com sucesso",
                signatureId = signature.Id,
                isFullySigned = isFullySigned,
                contractStatus = contract.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao assinar contrato {ContractId}", id);
            return BadRequest(new { message = "Erro ao assinar contrato" });
        }
    }

    [HttpGet("funcionarios-internos")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [SwaggerOperation(
        Summary = "Listar funcionários internos para contratos",
        Description = @"Retorna lista completa de funcionários internos (Dono da Empresa e Jurídicos) com todos os dados necessários para criação de contratos.

**Permissões:**
- DonoEmpresaPai
- Jurídico

**Dados Retornados:**
- ID, Nome, Email, Cargo
- CPF e RG (formatados)
- Data de Nascimento
- Telefones (Celular e Fixo)
- Endereço Completo
- Data de Cadastro

**Uso:**
- Seleção de signatários para contratos
- Definição de responsáveis por contratos
- Dados completos para documentação contratual"
    )]
    [SwaggerResponse(200, "Lista de funcionários internos", typeof(IEnumerable<FuncionarioInternoResponse>))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Sem permissão de acesso")]
    [SwaggerResponse(404, "Empresa pai não encontrada")]
    [ProducesResponseType(typeof(IEnumerable<FuncionarioInternoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFuncionariosInternos()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var result = await _userService.GetFuncionariosInternosAsync(userId);

            if (result.IsFailure)
                return BadRequest(new { erro = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar funcionários internos");
            return StatusCode(500, new { message = "Erro ao buscar funcionários internos" });
        }
    }

    [HttpGet("funcionarios-pj")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [SwaggerOperation(
        Summary = "Listar funcionários PJ para contratos",
        Description = @"Retorna lista completa de funcionários PJ (Pessoa Jurídica) contratados pela empresa com todos os dados necessários para criação de contratos.

**Permissões:**
- DonoEmpresaPai
- Jurídico

**Dados Retornados:**
- ID, Nome, Email, Cargo
- CPF e RG (formatados)
- Data de Nascimento
- Telefones (Celular e Fixo)
- Endereço Completo
- **Dados da Empresa PJ**: Razão Social, CNPJ, Tipo, Modelo de Negócio
- Data de Cadastro

**Uso:**
- Seleção de prestadores de serviço para contratos
- Dados completos da PJ para documentação contratual
- Informações da empresa PJ para emissão de notas fiscais

**Observação:**
Retorna apenas PJs que possuem relacionamento ativo com a empresa principal."
    )]
    [SwaggerResponse(200, "Lista de funcionários PJ", typeof(IEnumerable<FuncionarioPJResponse>))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Sem permissão de acesso")]
    [SwaggerResponse(404, "Empresa pai não encontrada")]
    [ProducesResponseType(typeof(IEnumerable<FuncionarioPJResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFuncionariosPJ()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var result = await _userService.GetFuncionariosPJAsync(userId);

            if (result.IsFailure)
                return BadRequest(new { erro = result.Error });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar funcionários PJ");
            return StatusCode(500, new { message = "Erro ao buscar funcionários PJ" });
        }
    }

    /// <summary>
    /// Deletar contrato com status Draft
    /// </summary>
    /// <param name="id">ID do contrato</param>
    [HttpDelete("{id}")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [SwaggerOperation(
        Summary = "Deletar contrato Draft",
        Description = "Deleta um contrato que está com status Draft (rascunho). Apenas contratos Draft podem ser deletados. Contratos ativos devem ser cancelados."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDraftContract(Guid id)
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

            var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound(new { message = "Contrato não encontrado" });
            }

            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid();
            }

            if (contract.Status != ContractStatus.Draft)
            {
                return BadRequest(new { 
                    message = $"Apenas contratos com status Draft podem ser deletados. Status atual: {contract.Status}",
                    statusAtual = contract.Status.ToString()
                });
            }

            await _unitOfWork.Contracts.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Contrato Draft deletado. ContractId: {ContractId}, UserId: {UserId}, CompanyId: {CompanyId}",
                id, userId, user.CompanyId
            );

            return Ok(new { 
                message = "Contrato Draft deletado com sucesso",
                contratoId = id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar contrato Draft {ContractId}", id);
            return StatusCode(500, new { message = "Erro ao deletar contrato Draft" });
        }
    }
}

public class CreateContractRequest
{
    public Guid ProviderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ValueTotal { get; set; }
    public decimal? MonthlyValue { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
}

public class SignContractRequest
{
    public SignatureMethod Method { get; set; }
    public string SignatureHash { get; set; } = string.Empty;
}