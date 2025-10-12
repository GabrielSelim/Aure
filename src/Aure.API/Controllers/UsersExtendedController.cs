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
    [HttpGet("network")]
    [Authorize]
    public async Task<IActionResult> GetNetworkUsers()
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
            
            if (currentUser.Role == UserRole.Provider)
            {
                // PJs (Providers) têm acesso MUITO limitado:
                // 1. Apenas a si mesmos
                // 2. Apenas contatos Admin/Company da empresa que os contratou (não outros funcionários)
                
                // Adicionar o próprio usuário
                networkUsers.Add(new
                {
                    UserId = currentUser.Id,
                    Name = currentUser.Name,
                    Email = currentUser.Email,
                    Role = currentUser.Role.ToString(),
                    CompanyId = currentUser.CompanyId,
                    CompanyName = ownCompany?.Name ?? "Empresa não encontrada",
                    CompanyCnpj = ownCompany?.Cnpj,
                    BusinessModel = ownCompany?.BusinessModel.ToString(),
                    Relationship = "Self",
                    IsDirectEmployee = true
                });

                // Buscar apenas contatos principais (Admin/Company) das empresas que contrataram este PJ
                var contractingRelationships = await _unitOfWork.CompanyRelationships.GetByProviderCompanyIdAsync(companyId.Value);
                var activeContracts = contractingRelationships.Where(r => 
                    r.Type == RelationshipType.ContractedPJ && r.Status == RelationshipStatus.Active);

                foreach (var contract in activeContracts)
                {
                    // Buscar apenas usuários Admin/Company da empresa contratante
                    var clientUsers = await _unitOfWork.Users.GetByCompanyIdAsync(contract.ClientCompanyId);
                    var contactUsers = clientUsers.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Company);

                    foreach (var user in contactUsers)
                    {
                        networkUsers.Add(new
                        {
                            UserId = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            Role = user.Role.ToString(),
                            CompanyId = user.CompanyId,
                            CompanyName = contract.ClientCompany.Name,
                            CompanyCnpj = contract.ClientCompany.Cnpj,
                            BusinessModel = contract.ClientCompany.BusinessModel.ToString(),
                            Relationship = "ContractingCompanyContact",
                            IsDirectEmployee = false
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
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        CompanyId = user.CompanyId,
                        CompanyName = ownCompany?.Name ?? "Empresa não encontrada",
                        CompanyCnpj = ownCompany?.Cnpj,
                        BusinessModel = ownCompany?.BusinessModel.ToString(),
                        Relationship = "OwnCompany",
                        IsDirectEmployee = true
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
                            UserId = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            Role = user.Role.ToString(),
                            CompanyId = user.CompanyId,
                            CompanyName = relatedCompany.Name,
                            CompanyCnpj = relatedCompany.Cnpj,
                            BusinessModel = relatedCompany.BusinessModel.ToString(),
                            Relationship = relationship.Type.ToString(),
                            RelationshipStatus = relationship.Status.ToString(),
                            IsDirectEmployee = false,
                            RelationshipNotes = relationship.Notes
                        });
                    }
                }
            }

            return Ok(new
            {
                TotalUsers = networkUsers.Count,
                OwnCompanyUsers = networkUsers.Count(u => (bool)(u.GetType().GetProperty("IsDirectEmployee")?.GetValue(u) ?? false)),
                RelatedCompanyUsers = networkUsers.Count(u => !(bool)(u.GetType().GetProperty("IsDirectEmployee")?.GetValue(u) ?? false)),
                Users = networkUsers,
                UserRole = currentUser.Role.ToString(),
                SecurityNote = currentUser.Role == UserRole.Provider ? "Limited access - PJ can only see self and contracting company contacts" : "Full network access"
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
    [HttpGet("contracted-pjs")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> GetContractedPJs()
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
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        PjCompany = new
                        {
                            Id = relationship.ProviderCompany.Id,
                            Name = relationship.ProviderCompany.Name,
                            Cnpj = relationship.ProviderCompany.Cnpj,
                            BusinessModel = relationship.ProviderCompany.BusinessModel.ToString()
                        },
                        ContractInfo = new
                        {
                            RelationshipId = relationship.Id,
                            StartDate = relationship.StartDate,
                            Status = relationship.Status.ToString(),
                            Notes = relationship.Notes
                        }
                    });
                }
            }

            return Ok(new
            {
                TotalContractedPJs = contractedPJs.Count,
                ActiveContracts = pjRelationships.Count(),
                ContractedPJs = contractedPJs
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
    [HttpGet("contracted-by")]
    [Authorize]
    public async Task<IActionResult> GetContractedBy()
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
                RelationshipId = relationship.Id,
                ClientCompany = new
                {
                    Id = relationship.ClientCompany.Id,
                    Name = relationship.ClientCompany.Name,
                    Cnpj = relationship.ClientCompany.Cnpj,
                    BusinessModel = relationship.ClientCompany.BusinessModel.ToString()
                },
                ContractInfo = new
                {
                    StartDate = relationship.StartDate,
                    Status = relationship.Status.ToString(),
                    Notes = relationship.Notes
                }
            });

            return Ok(new
            {
                TotalContracts = contractedBy.Count(),
                ContractedBy = contractedBy
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
    [HttpGet("network/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetNetworkUser(Guid userId)
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

            if (currentUser.Role == UserRole.Provider)
            {
                // PJs só podem ver:
                // 1. A si mesmos
                // 2. Contatos Admin/Company das empresas que os contrataram
                
                // Verificar se está tentando ver a si mesmo
                if (userId == currentUserId)
                {
                    var ownCompany = await _unitOfWork.Companies.GetByIdAsync(companyId.Value);
                    
                    return Ok(new
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        CompanyId = user.CompanyId,
                        CompanyName = ownCompany?.Name ?? "Empresa não encontrada",
                        CompanyCnpj = ownCompany?.Cnpj,
                        BusinessModel = ownCompany?.BusinessModel.ToString(),
                        Relationship = "Self",
                        IsDirectEmployee = true
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

                // Verificar se o usuário solicitado é Admin ou Company da empresa contratante
                if (user.Role != UserRole.Admin && user.Role != UserRole.Company)
                {
                    return Forbid("Acesso negado: PJs só podem ver contatos principais (Admin/Company) das empresas contratantes");
                }

                var userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                
                return Ok(new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CompanyId = user.CompanyId,
                    CompanyName = userCompany?.Name,
                    CompanyCnpj = userCompany?.Cnpj,
                    BusinessModel = userCompany?.BusinessModel.ToString(),
                    Relationship = "ContractingCompanyContact",
                    IsDirectEmployee = false
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
                        UserId = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        CompanyId = user.CompanyId,
                        CompanyName = ownCompany?.Name ?? "Empresa não encontrada",
                        CompanyCnpj = ownCompany?.Cnpj,
                        BusinessModel = ownCompany?.BusinessModel.ToString(),
                        Relationship = "OwnCompany",
                        IsDirectEmployee = true
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
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CompanyId = user.CompanyId,
                    CompanyName = userCompany?.Name,
                    CompanyCnpj = userCompany?.Cnpj,
                    BusinessModel = userCompany?.BusinessModel.ToString(),
                    Relationship = userRelationship.Type.ToString(),
                    RelationshipStatus = userRelationship.Status.ToString(),
                    IsDirectEmployee = false,
                    IsFromContractedPJ = isFromProviderCompany
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