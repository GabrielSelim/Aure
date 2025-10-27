using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using System.Security.Claims;

namespace Aure.API.Controllers;

/// <summary>
/// Controller estendido para gerenciamento de usuários considerando relacionamentos entre empresas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersExtendedController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UsersExtendedController> _logger;

    public UsersExtendedController(
        IUserService userService, 
        IUnitOfWork unitOfWork,
        ILogger<UsersExtendedController> logger)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private async Task<Guid?> GetCurrentUserCompanyIdAsync()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty) return null;

        var userResult = await _userService.GetByIdAsync(currentUserId);
        return userResult.IsSuccess ? userResult.Data?.CompanyId : null;
    }

    /// <summary>
    /// Lista usuários da rede da empresa (empresa própria + empresas relacionadas)
    /// Aplica regras de segurança baseadas no role do usuário
    /// </summary>
    [HttpGet("rede")]
    [Authorize]
    public async Task<IActionResult> GetUsuariosRede()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var companyId = await GetCurrentUserCompanyIdAsync();
            if (companyId == null)
            {
                return BadRequest(new { message = "Usuário não associado a nenhuma empresa" });
            }

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return BadRequest(new { message = "Usuário atual não encontrado" });
            }

            // Buscar dados da própria empresa
            var ownCompany = await _unitOfWork.Companies.GetByIdAsync(companyId.Value);
            
            var networkUsers = new List<object>();

            // REGRAS DE SEGURANÇA BASEADAS NO ROLE:
            
            if (currentUser.Role == UserRole.FuncionarioPJ)
            {
                // PJs (FuncionarioPJ) têm acesso MUITO limitado:
                // 1. Apenas a si mesmos
                // 2. Apenas contatos DonoEmpresaPai/Financeiro da empresa que os contratou (não outros funcionários)
                
                // Adicionar o próprio usuário
                networkUsers.Add(new
                {
                    UsuarioId = currentUser.Id,
                    Nome = currentUser.Name,
                    Email = currentUser.Email,
                    Funcao = currentUser.Role.ToString(),
                    EmpresaId = currentUser.CompanyId,
                    NomeEmpresa = ownCompany?.Name ?? "Empresa não encontrada",
                    CnpjEmpresa = ownCompany?.Cnpj,
                    ModeloNegocio = ownCompany?.BusinessModel.ToString(),
                    Relacionamento = "Proprio",
                    EhFuncionarioDireto = true
                });

                // Buscar apenas contatos principais (Admin/Company) das empresas que contrataram este PJ
                var contractingRelationships = await _unitOfWork.CompanyRelationships.GetByProviderCompanyIdAsync(companyId.Value);
                var activeContracts = contractingRelationships.Where(r => 
                    r.Type == RelationshipType.ContractedPJ && r.Status == RelationshipStatus.Active);

                foreach (var contract in activeContracts)
                {
                    // Buscar apenas usuários DonoEmpresaPai/Financeiro da empresa contratante
                    var clientUsers = await _unitOfWork.Users.GetByCompanyIdAsync(contract.ClientCompanyId);
                    var contactUsers = clientUsers.Where(u => u.Role == UserRole.DonoEmpresaPai || u.Role == UserRole.Financeiro);

                    foreach (var user in contactUsers)
                    {
                        networkUsers.Add(new
                        {
                            UsuarioId = user.Id,
                            Nome = user.Name,
                            Email = user.Email,
                            Funcao = user.Role.ToString(),
                            EmpresaId = user.CompanyId,
                            NomeEmpresa = contract.ClientCompany.Name,
                            CnpjEmpresa = contract.ClientCompany.Cnpj,
                            ModeloNegocio = contract.ClientCompany.BusinessModel.ToString(),
                            Relacionamento = "ContatoEmpresaContratante",
                            EhFuncionarioDireto = false
                        });
                    }
                }
            }
            else
            {
                // Admin e Company têm acesso completo à rede
                
                // Usuários da própria empresa
                var ownUsers = await _unitOfWork.Users.GetByCompanyIdAsync(companyId.Value);
                
                // Relacionamentos ativos da empresa
                var relationships = await _unitOfWork.CompanyRelationships.GetActiveRelationshipsByCompanyIdAsync(companyId.Value);

                // Adicionar usuários da própria empresa
                foreach (var user in ownUsers)
                {
                    networkUsers.Add(new
                    {
                        UsuarioId = user.Id,
                        Nome = user.Name,
                        Email = user.Email,
                        Funcao = user.Role.ToString(),
                        EmpresaId = user.CompanyId,
                        NomeEmpresa = ownCompany?.Name ?? "Empresa não encontrada",
                        CnpjEmpresa = ownCompany?.Cnpj,
                        ModeloNegocio = ownCompany?.BusinessModel.ToString(),
                        Relacionamento = "PropriaEmpresa",
                        EhFuncionarioDireto = true
                    });
                }

                // Adicionar usuários de empresas relacionadas
                foreach (var relationship in relationships)
                {
                    var relatedCompanyId = relationship.ClientCompanyId == companyId 
                        ? relationship.ProviderCompanyId 
                        : relationship.ClientCompanyId;

                    var relatedUsers = await _unitOfWork.Users.GetByCompanyIdAsync(relatedCompanyId);
                    var isClient = relationship.ClientCompanyId == companyId;
                    var relatedCompany = isClient ? relationship.ProviderCompany : relationship.ClientCompany;

                    foreach (var user in relatedUsers)
                    {
                        networkUsers.Add(new
                        {
                            UsuarioId = user.Id,
                            Nome = user.Name,
                            Email = user.Email,
                            Funcao = user.Role.ToString(),
                            EmpresaId = user.CompanyId,
                            NomeEmpresa = relatedCompany.Name,
                            CnpjEmpresa = relatedCompany.Cnpj,
                            ModeloNegocio = relatedCompany.BusinessModel.ToString(),
                            Relacionamento = relationship.Type.ToString(),
                            StatusRelacionamento = relationship.Status.ToString(),
                            EhFuncionarioDireto = false,
                            ObservacoesRelacionamento = relationship.Notes
                        });
                    }
                }
            }

            return Ok(new
            {
                TotalUsuarios = networkUsers.Count,
                UsuariosPropriaEmpresa = networkUsers.Count(u => (bool)(u.GetType().GetProperty("EhFuncionarioDireto")?.GetValue(u) ?? false)),
                UsuariosEmpresasRelacionadas = networkUsers.Count(u => !(bool)(u.GetType().GetProperty("EhFuncionarioDireto")?.GetValue(u) ?? false)),
                Usuarios = networkUsers,
                FuncaoUsuario = currentUser.Role.ToString(),
                NotaSeguranca = currentUser.Role == UserRole.FuncionarioPJ ? "Acesso limitado - PJ pode ver apenas a si mesmo e contatos da empresa contratante" : "Acesso completo à rede"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários da rede");
            return BadRequest(new { message = "Erro ao buscar usuários da rede" });
        }
    }

    /// <summary>
    /// Lista apenas usuários PJs contratados pela empresa
    /// </summary>
    [HttpGet("pjs-contratados")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetPjsContratados()
    {
        try
        {
            var companyId = await GetCurrentUserCompanyIdAsync();
            if (companyId == null)
            {
                return BadRequest(new { message = "Usuário não associado a nenhuma empresa" });
            }

            // Buscar relacionamentos onde a empresa é cliente (contratou PJs)
            var relationships = await _unitOfWork.CompanyRelationships.GetByClientCompanyIdAsync(companyId.Value);
            var pjRelationships = relationships.Where(r => r.Type == RelationshipType.ContractedPJ && r.Status == RelationshipStatus.Active);

            var contractedPJs = new List<object>();

            foreach (var relationship in pjRelationships)
            {
                var pjUsers = await _unitOfWork.Users.GetByCompanyIdAsync(relationship.ProviderCompanyId);
                
                foreach (var user in pjUsers)
                {
                    contractedPJs.Add(new
                    {
                        UsuarioId = user.Id,
                        Nome = user.Name,
                        Email = user.Email,
                        Funcao = user.Role.ToString(),
                        EmpresaPj = new
                        {
                            Id = relationship.ProviderCompany.Id,
                            Nome = relationship.ProviderCompany.Name,
                            Cnpj = relationship.ProviderCompany.Cnpj,
                            ModeloNegocio = relationship.ProviderCompany.BusinessModel.ToString()
                        },
                        InfoContrato = new
                        {
                            IdRelacionamento = relationship.Id,
                            DataInicio = relationship.StartDate,
                            Status = relationship.Status.ToString(),
                            Observacoes = relationship.Notes
                        }
                    });
                }
            }

            return Ok(new
            {
                TotalPjsContratados = contractedPJs.Count,
                ContratosAtivos = pjRelationships.Count(),
                PjsContratados = contractedPJs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar PJs contratados");
            return BadRequest(new { message = "Erro ao buscar PJs contratados" });
        }
    }

    /// <summary>
    /// Lista empresas que contrataram esta empresa PJ
    /// </summary>
    [HttpGet("contratado-por")]
    [Authorize]
    public async Task<IActionResult> GetContratadoPor()
    {
        try
        {
            var companyId = await GetCurrentUserCompanyIdAsync();
            if (companyId == null)
            {
                return BadRequest(new { message = "Usuário não associado a nenhuma empresa" });
            }

            // Buscar relacionamentos onde a empresa é provedor (foi contratada)
            var relationships = await _unitOfWork.CompanyRelationships.GetByProviderCompanyIdAsync(companyId.Value);
            var contractRelationships = relationships.Where(r => r.Type == RelationshipType.ContractedPJ && r.Status == RelationshipStatus.Active);

            var contractedBy = contractRelationships.Select(relationship => new
            {
                IdRelacionamento = relationship.Id,
                EmpresaCliente = new
                {
                    Id = relationship.ClientCompany.Id,
                    Nome = relationship.ClientCompany.Name,
                    Cnpj = relationship.ClientCompany.Cnpj,
                    ModeloNegocio = relationship.ClientCompany.BusinessModel.ToString()
                },
                InfoContrato = new
                {
                    DataInicio = relationship.StartDate,
                    Status = relationship.Status.ToString(),
                    Observacoes = relationship.Notes
                }
            });

            return Ok(new
            {
                TotalContratos = contractedBy.Count(),
                ContratadoPor = contractedBy
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresas contratantes");
            return BadRequest(new { message = "Erro ao buscar empresas contratantes" });
        }
    }

    /// <summary>
    /// Busca usuário por ID considerando a rede de relacionamentos
    /// Aplica regras de segurança baseadas no role do usuário
    /// </summary>
    [HttpGet("rede/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUsuarioRede(Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var companyId = await GetCurrentUserCompanyIdAsync();
            if (companyId == null)
            {
                return BadRequest(new { message = "Usuário não associado a nenhuma empresa" });
            }

            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return BadRequest(new { message = "Usuário atual não encontrado" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // REGRAS DE SEGURANÇA BASEADAS NO ROLE:

            if (currentUser.Role == UserRole.FuncionarioPJ)
            {
                // PJs só podem ver:
                // 1. A si mesmos
                // 2. Contatos DonoEmpresaPai/Financeiro das empresas que os contrataram
                
                // Verificar se está tentando ver a si mesmo
                if (userId == currentUserId)
                {
                    var ownCompany = await _unitOfWork.Companies.GetByIdAsync(companyId.Value);
                    
                    return Ok(new
                    {
                        UsuarioId = user.Id,
                        Nome = user.Name,
                        Email = user.Email,
                        Funcao = user.Role.ToString(),
                        EmpresaId = user.CompanyId,
                        NomeEmpresa = ownCompany?.Name ?? "Empresa não encontrada",
                        CnpjEmpresa = ownCompany?.Cnpj,
                        ModeloNegocio = ownCompany?.BusinessModel.ToString(),
                        Relacionamento = "ProprioUsuario",
                        EhFuncionarioDireto = true
                    });
                }

                // Verificar se é um contato autorizado de empresa contratante
                var contractingRelationships = await _unitOfWork.CompanyRelationships.GetByProviderCompanyIdAsync(companyId.Value);
                var activeContracts = contractingRelationships.Where(r => 
                    r.Type == RelationshipType.ContractedPJ && 
                    r.Status == RelationshipStatus.Active &&
                    r.ClientCompanyId == user.CompanyId);

                if (!activeContracts.Any())
                {
                    return Forbid("Acesso negado: Você só pode visualizar seus próprios dados e contatos das empresas que o contrataram");
                }

                // Verificar se o usuário solicitado é DonoEmpresaPai ou Financeiro da empresa contratante
                if (user.Role != UserRole.DonoEmpresaPai && user.Role != UserRole.Financeiro)
                {
                    return Forbid("Acesso negado: PJs só podem ver contatos principais (DonoEmpresaPai/Financeiro) das empresas contratantes");
                }

                var userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                
                return Ok(new
                {
                    UsuarioId = user.Id,
                    Nome = user.Name,
                    Email = user.Email,
                    Funcao = user.Role.ToString(),
                    EmpresaId = user.CompanyId,
                    NomeEmpresa = userCompany?.Name,
                    CnpjEmpresa = userCompany?.Cnpj,
                    ModeloNegocio = userCompany?.BusinessModel.ToString(),
                    Relacionamento = "ContatoEmpresaContratante",
                    EhFuncionarioDireto = false
                });
            }
            else
            {
                // Admin e Company têm acesso completo
                
                // Verificar se o usuário pertence à mesma empresa
                if (user.CompanyId == companyId)
                {
                    var ownCompany = await _unitOfWork.Companies.GetByIdAsync(companyId.Value);
                    
                    return Ok(new
                    {
                        UsuarioId = user.Id,
                        Nome = user.Name,
                        Email = user.Email,
                        Funcao = user.Role.ToString(),
                        EmpresaId = user.CompanyId,
                        NomeEmpresa = ownCompany?.Name ?? "Empresa não encontrada",
                        CnpjEmpresa = ownCompany?.Cnpj,
                        ModeloNegocio = ownCompany?.BusinessModel.ToString(),
                        Relacionamento = "PropriaEmpresa",
                        EhFuncionarioDireto = true
                    });
                }

                // Verificar se há relacionamento entre as empresas
                var relationship = await _unitOfWork.CompanyRelationships.GetActiveRelationshipsByCompanyIdAsync(companyId.Value);
                var userRelationship = relationship.FirstOrDefault(r => 
                    r.ClientCompanyId == user.CompanyId || r.ProviderCompanyId == user.CompanyId);

                if (userRelationship == null)
                {
                    return Forbid("Usuário não pertence à rede de relacionamentos da sua empresa");
                }

                var userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                var isFromProviderCompany = userRelationship.ProviderCompanyId == user.CompanyId;

                return Ok(new
                {
                    UsuarioId = user.Id,
                    Nome = user.Name,
                    Email = user.Email,
                    Funcao = user.Role.ToString(),
                    EmpresaId = user.CompanyId,
                    NomeEmpresa = userCompany?.Name,
                    CnpjEmpresa = userCompany?.Cnpj,
                    ModeloNegocio = userCompany?.BusinessModel.ToString(),
                    Relacionamento = userRelationship.Type.ToString(),
                    StatusRelacionamento = userRelationship.Status.ToString(),
                    EhFuncionarioDireto = false,
                    EhDePjContratado = isFromProviderCompany
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário da rede");
            return BadRequest(new { message = "Erro ao buscar usuário da rede" });
        }
    }
}