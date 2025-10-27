using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyRelationshipsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyRelationshipsController> _logger;

    public CompanyRelationshipsController(IUnitOfWork unitOfWork, ILogger<CompanyRelationshipsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obter relacionamentos da empresa por status (como cliente ou provedor)
    /// </summary>
    /// <param name="status">Status do relacionamento: Active, Inactive, Terminated, Suspended. Se não informado, retorna todos os status</param>
    [HttpGet]
    public async Task<IActionResult> GetCompanyRelationships([FromQuery] string? status = null)
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

            // Parse do status se fornecido
            Domain.Enums.RelationshipStatus? relationshipStatus = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<Domain.Enums.RelationshipStatus>(status, true, out var parsedStatus))
                {
                    return BadRequest(new { message = "Status inválido. Valores aceitos: Active, Inactive, Terminated, Suspended" });
                }
                relationshipStatus = parsedStatus;
            }

            var relationships = await _unitOfWork.CompanyRelationships.GetRelationshipsByCompanyIdAndStatusAsync(user.CompanyId.Value, relationshipStatus);

            var relationshipsList = relationships.ToList();
            var result = new
            {
                FiltradoPor = status ?? "Todos",
                TotalRelacionamentos = relationshipsList.Count,
                ContagemPorStatus = relationshipsList.GroupBy(r => r.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                Relacionamentos = relationshipsList.Select(r => new
                {
                    Id = r.Id,
                    Tipo = r.Type.ToString(),
                    Status = r.Status.ToString(),
                    DataInicio = r.StartDate,
                    DataFim = r.EndDate,
                    Observacoes = r.Notes,
                    EhCliente = r.ClientCompanyId == user.CompanyId,
                    EhFornecedor = r.ProviderCompanyId == user.CompanyId,
                    EmpresaRelacionada = r.ClientCompanyId == user.CompanyId 
                        ? new { Id = r.ProviderCompanyId, Nome = r.ProviderCompany.Name, Cnpj = r.ProviderCompany.Cnpj }
                        : new { Id = r.ClientCompanyId, Nome = r.ClientCompany.Name, Cnpj = r.ClientCompany.Cnpj }
                })
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar relacionamentos da empresa");
            return BadRequest(new { message = "Erro ao buscar relacionamentos" });
        }
    }

    /// <summary>
    /// Obter valores a pagar este mês baseado nos relacionamentos ativos
    /// </summary>
    [HttpGet("compromissos-mensais")]
    public async Task<IActionResult> GetCompromissosMensais()
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

            // Buscar relacionamentos ativos onde a empresa é cliente
            var clientRelationships = await _unitOfWork.CompanyRelationships.GetByClientCompanyIdAsync(user.CompanyId.Value);
            clientRelationships = clientRelationships.Where(r => r.Status == Domain.Enums.RelationshipStatus.Active);

            // Buscar contratos ativos para esses relacionamentos
            var monthlyCommitments = new List<object>();
            decimal totalCommitments = 0;

            foreach (var relationship in clientRelationships)
            {
                var contracts = await _unitOfWork.Contracts.GetByClientIdAsync(user.CompanyId.Value);
                var activeContracts = contracts.Where(c => 
                    c.ProviderId == relationship.ProviderCompanyId &&
                    c.Status == ContractStatus.Active && 
                    !c.IsExpired &&
                    c.MonthlyValue.HasValue &&
                    c.StartDate <= DateTime.UtcNow &&
                    (c.ExpirationDate == null || c.ExpirationDate >= DateTime.UtcNow)
                );

                var relationshipTotal = activeContracts.Sum(c => c.MonthlyValue!.Value);
                if (relationshipTotal > 0)
                {
                    monthlyCommitments.Add(new
                    {
                        RelationshipId = relationship.Id,
                        ProviderCompany = new 
                        { 
                            relationship.ProviderCompany.Id, 
                            relationship.ProviderCompany.Name, 
                            relationship.ProviderCompany.Cnpj 
                        },
                        RelationshipType = relationship.Type.ToString(),
                        MonthlyTotal = relationshipTotal,
                        ContractCount = activeContracts.Count(),
                        Contracts = activeContracts.Select(c => new
                        {
                            ContractId = c.Id,
                            Title = c.Title,
                            MonthlyValue = c.MonthlyValue!.Value,
                            StartDate = c.StartDate,
                            ExpirationDate = c.ExpirationDate
                        })
                    });
                    totalCommitments += relationshipTotal;
                }
            }

            return Ok(new
            {
                Mes = currentMonth,
                Ano = currentYear,
                NomeMes = new DateTime(currentYear, currentMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-BR")),
                TotalCompromissos = totalCommitments,
                QuantidadeRelacionamentos = monthlyCommitments.Count,
                CompromissosMensais = monthlyCommitments.Select(c => new
                {
                    RelacionamentoId = ((dynamic)c).RelationshipId,
                    EmpresaFornecedora = ((dynamic)c).ProviderCompany,
                    TipoRelacionamento = ((dynamic)c).RelationshipType,
                    TotalMensal = ((dynamic)c).MonthlyTotal,
                    QuantidadeContratos = ((dynamic)c).ContractCount,
                    Contratos = ((dynamic)c).Contracts
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar compromissos mensais");
            return BadRequest(new { message = "Erro ao buscar compromissos mensais" });
        }
    }

    /// <summary>
    /// Obter valores a receber este mês baseado nos relacionamentos como provedor
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

            // Buscar relacionamentos ativos onde a empresa é provedora
            var providerRelationships = await _unitOfWork.CompanyRelationships.GetByProviderCompanyIdAsync(user.CompanyId.Value);
            providerRelationships = providerRelationships.Where(r => r.Status == RelationshipStatus.Active);

            // Buscar contratos ativos para esses relacionamentos
            var monthlyIncome = new List<object>();
            decimal totalIncome = 0;

            foreach (var relationship in providerRelationships)
            {
                var contracts = await _unitOfWork.Contracts.GetByProviderIdAsync(user.CompanyId.Value);
                var activeContracts = contracts.Where(c => 
                    c.ClientId == relationship.ClientCompanyId &&
                    c.Status == ContractStatus.Active && 
                    !c.IsExpired &&
                    c.MonthlyValue.HasValue &&
                    c.StartDate <= DateTime.UtcNow &&
                    (c.ExpirationDate == null || c.ExpirationDate >= DateTime.UtcNow)
                );

                var relationshipTotal = activeContracts.Sum(c => c.MonthlyValue!.Value);
                if (relationshipTotal > 0)
                {
                    monthlyIncome.Add(new
                    {
                        RelationshipId = relationship.Id,
                        ClientCompany = new 
                        { 
                            relationship.ClientCompany.Id, 
                            relationship.ClientCompany.Name, 
                            relationship.ClientCompany.Cnpj 
                        },
                        RelationshipType = relationship.Type.ToString(),
                        MonthlyTotal = relationshipTotal,
                        ContractCount = activeContracts.Count(),
                        Contracts = activeContracts.Select(c => new
                        {
                            ContractId = c.Id,
                            Title = c.Title,
                            MonthlyValue = c.MonthlyValue!.Value,
                            StartDate = c.StartDate,
                            ExpirationDate = c.ExpirationDate
                        })
                    });
                    totalIncome += relationshipTotal;
                }
            }

            return Ok(new
            {
                Mes = currentMonth,
                Ano = currentYear,
                NomeMes = new DateTime(currentYear, currentMonth, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-BR")),
                TotalReceitas = totalIncome,
                QuantidadeRelacionamentos = monthlyIncome.Count,
                ReceitasMensais = monthlyIncome.Select(i => new
                {
                    RelacionamentoId = ((dynamic)i).RelationshipId,
                    EmpresaCliente = ((dynamic)i).ClientCompany,
                    TipoRelacionamento = ((dynamic)i).RelationshipType,
                    TotalMensal = ((dynamic)i).MonthlyTotal,
                    QuantidadeContratos = ((dynamic)i).ContractCount,
                    Contratos = ((dynamic)i).Contracts
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
    /// Obter relacionamentos onde a empresa é cliente (contrata outras empresas)
    /// </summary>
    [HttpGet("como-cliente")]
    public async Task<IActionResult> GetComoCliente()
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

            var relationships = await _unitOfWork.CompanyRelationships.GetByClientCompanyIdAsync(user.CompanyId.Value);

            var result = relationships.Select(r => new
            {
                Id = r.Id,
                Tipo = r.Type.ToString(),
                Status = r.Status.ToString(),
                DataInicio = r.StartDate,
                DataFim = r.EndDate,
                Observacoes = r.Notes,
                EmpresaContratada = new 
                { 
                    Id = r.ProviderCompanyId, 
                    Nome = r.ProviderCompany.Name, 
                    Cnpj = r.ProviderCompany.Cnpj,
                    ModeloNegocio = r.ProviderCompany.BusinessModel.ToString()
                }
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresas contratadas");
            return BadRequest(new { message = "Erro ao buscar empresas contratadas" });
        }
    }

    /// <summary>
    /// Obter relacionamentos onde a empresa é provedor (foi contratada por outras empresas)
    /// </summary>
    [HttpGet("como-fornecedor")]
    public async Task<IActionResult> GetComoFornecedor()
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

            var relationships = await _unitOfWork.CompanyRelationships.GetByProviderCompanyIdAsync(user.CompanyId.Value);

            var result = relationships.Select(r => new
            {
                Id = r.Id,
                Tipo = r.Type.ToString(),
                Status = r.Status.ToString(),
                DataInicio = r.StartDate,
                DataFim = r.EndDate,
                Observacoes = r.Notes,
                EmpresaCliente = new 
                { 
                    Id = r.ClientCompanyId, 
                    Nome = r.ClientCompany.Name, 
                    Cnpj = r.ClientCompany.Cnpj,
                    ModeloNegocio = r.ClientCompany.BusinessModel.ToString()
                }
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresas contratantes");
            return BadRequest(new { message = "Erro ao buscar empresas contratantes" });
        }
    }

    /// <summary>
    /// Ativar relacionamento com PJ (apenas Admin/Company)
    /// </summary>
    [HttpPut("{relationshipId:guid}/ativar")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    public async Task<IActionResult> AtivarRelacionamento(Guid relationshipId)
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

            var relationship = await _unitOfWork.CompanyRelationships.GetByIdAsync(relationshipId);
            if (relationship == null)
            {
                return NotFound(new { message = "Relacionamento não encontrado" });
            }

            // Verificar se o usuário tem permissão para modificar este relacionamento
            if (relationship.ClientCompanyId != user.CompanyId && relationship.ProviderCompanyId != user.CompanyId)
            {
                return Forbid("Você não tem permissão para modificar este relacionamento");
            }

            // Apenas a empresa cliente (contratante) pode ativar/desativar PJs
            if (relationship.ClientCompanyId != user.CompanyId)
            {
                return Forbid("Apenas a empresa contratante pode ativar relacionamentos com PJs");
            }

            relationship.UpdateStatus(Domain.Enums.RelationshipStatus.Active);

            await _unitOfWork.CompanyRelationships.UpdateAsync(relationship);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Relacionamento {RelationshipId} ativado pela empresa {CompanyId}", relationshipId, user.CompanyId);

            return Ok(new
            {
                mensagem = "Relacionamento ativado com sucesso",
                relacionamentoId = relationshipId,
                status = "Active",
                atualizadoEm = relationship.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar relacionamento {RelationshipId}", relationshipId);
            return BadRequest(new { message = "Erro ao ativar relacionamento" });
        }
    }

    /// <summary>
    /// Desativar relacionamento com PJ (apenas Admin/Company)
    /// </summary>
    [HttpPut("{relationshipId:guid}/desativar")]
    [Authorize(Roles = "DonoEmpresaPai,Financeiro")]
    public async Task<IActionResult> DesativarRelacionamento(Guid relationshipId)
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

            var relationship = await _unitOfWork.CompanyRelationships.GetByIdAsync(relationshipId);
            if (relationship == null)
            {
                return NotFound(new { message = "Relacionamento não encontrado" });
            }

            // Verificar se o usuário tem permissão para modificar este relacionamento
            if (relationship.ClientCompanyId != user.CompanyId && relationship.ProviderCompanyId != user.CompanyId)
            {
                return Forbid("Você não tem permissão para modificar este relacionamento");
            }

            // Apenas a empresa cliente (contratante) pode ativar/desativar PJs
            if (relationship.ClientCompanyId != user.CompanyId)
            {
                return Forbid("Apenas a empresa contratante pode desativar relacionamentos com PJs");
            }

            relationship.UpdateStatus(Domain.Enums.RelationshipStatus.Inactive);

            await _unitOfWork.CompanyRelationships.UpdateAsync(relationship);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Relacionamento {RelationshipId} desativado pela empresa {CompanyId}", relationshipId, user.CompanyId);

            return Ok(new
            {
                mensagem = "Relacionamento desativado com sucesso",
                relacionamentoId = relationshipId,
                status = "Inactive",
                atualizadoEm = relationship.UpdatedAt,
                observacao = "PJ não terá mais acesso aos sistemas da empresa contratante"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar relacionamento {RelationshipId}", relationshipId);
            return BadRequest(new { message = "Erro ao desativar relacionamento" });
        }
    }

    /// <summary>
    /// Buscar usuários de um relacionamento específico
    /// </summary>
    [HttpGet("{relationshipId:guid}/usuarios")]
    [Authorize]
    public async Task<IActionResult> GetUsuariosRelacionamento(Guid relationshipId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new { message = "Token de usuário inválido" });
            }

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null || currentUser.CompanyId == null)
            {
                return NotFound(new { message = "Usuário atual não encontrado ou sem empresa" });
            }

            // Buscar o relacionamento específico
            var relationship = await _unitOfWork.CompanyRelationships.GetByIdAsync(relationshipId);
            if (relationship == null)
            {
                return NotFound(new { message = "Relacionamento não encontrado" });
            }

            // Verificar se o usuário atual tem acesso a este relacionamento
            if (relationship.ClientCompanyId != currentUser.CompanyId && relationship.ProviderCompanyId != currentUser.CompanyId)
            {
                return Forbid("Você não tem acesso a este relacionamento");
            }

            // Aplicar regras de segurança para PJs
            if (currentUser.Role == Domain.Enums.UserRole.FuncionarioPJ)
            {
                // PJs só podem ver relacionamentos onde eles são o provedor
                if (relationship.ProviderCompanyId != currentUser.CompanyId)
                {
                    return Forbid("PJs só podem ver relacionamentos onde são o provedor");
                }
            }

            // Buscar usuários das duas empresas do relacionamento
            var clientUsers = await _unitOfWork.Users.GetByCompanyIdAsync(relationship.ClientCompanyId);
            var providerUsers = await _unitOfWork.Users.GetByCompanyIdAsync(relationship.ProviderCompanyId);

            var clientCompany = await _unitOfWork.Companies.GetByIdAsync(relationship.ClientCompanyId);
            var providerCompany = await _unitOfWork.Companies.GetByIdAsync(relationship.ProviderCompanyId);

            if (clientCompany == null || providerCompany == null)
            {
                return NotFound(new { message = "Uma ou ambas as empresas do relacionamento não foram encontradas" });
            }

            // Filtrar usuários baseado no papel do usuário atual
            var filteredClientUsers = clientUsers.AsEnumerable();
            var filteredProviderUsers = providerUsers.AsEnumerable();

            if (currentUser.Role == Domain.Enums.UserRole.FuncionarioPJ)
            {
                // PJs só podem ver contatos principais (DonoEmpresaPai/Financeiro) da empresa contratante
                filteredClientUsers = clientUsers.Where(u => 
                    u.Role == Domain.Enums.UserRole.DonoEmpresaPai || 
                    u.Role == Domain.Enums.UserRole.Financeiro);
            }

            var result = new
            {
                RelacionamentoId = relationship.Id,
                TipoRelacionamento = relationship.Type.ToString(),
                StatusRelacionamento = relationship.Status.ToString(),
                DataInicio = relationship.StartDate,
                DataFim = relationship.EndDate,
                Observacoes = relationship.Notes,
                EmpresaCliente = new
                {
                    Id = clientCompany.Id,
                    Nome = clientCompany.Name,
                    Cnpj = clientCompany.Cnpj,
                    ModeloNegocio = clientCompany.BusinessModel.ToString(),
                    Usuarios = filteredClientUsers.Select(u => new
                    {
                        UsuarioId = u.Id,
                        Nome = u.Name,
                        Email = u.Email,
                        Funcao = u.Role.ToString(),
                        CriadoEm = u.CreatedAt,
                        EhAcessivel = true
                    })
                },
                EmpresaFornecedora = new
                {
                    Id = providerCompany.Id,
                    Nome = providerCompany.Name,
                    Cnpj = providerCompany.Cnpj,
                    ModeloNegocio = providerCompany.BusinessModel.ToString(),
                    Usuarios = filteredProviderUsers.Select(u => new
                    {
                        UsuarioId = u.Id,
                        Nome = u.Name,
                        Email = u.Email,
                        Funcao = u.Role.ToString(),
                        CriadoEm = u.CreatedAt,
                        EhAcessivel = true
                    })
                },
                TotalUsuarios = filteredClientUsers.Count() + filteredProviderUsers.Count(),
                AcessoUsuarioAtual = new
                {
                    PodeVerUsuariosCliente = true,
                    PodeVerUsuariosFornecedor = true,
                    EhDaEmpresaCliente = currentUser.CompanyId == relationship.ClientCompanyId,
                    EhDaEmpresaFornecedora = currentUser.CompanyId == relationship.ProviderCompanyId,
                    FuncaoUsuario = currentUser.Role.ToString()
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários do relacionamento {RelationshipId}", relationshipId);
            return BadRequest(new { message = "Erro ao buscar usuários do relacionamento" });
        }
    }
}