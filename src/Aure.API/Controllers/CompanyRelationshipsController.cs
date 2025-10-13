using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
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
                FilteredBy = status ?? "All",
                TotalRelationships = relationshipsList.Count,
                StatusCounts = relationshipsList.GroupBy(r => r.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                Relationships = relationshipsList.Select(r => new
                {
                    Id = r.Id,
                    Type = r.Type.ToString(),
                    Status = r.Status.ToString(),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Notes = r.Notes,
                    IsClient = r.ClientCompanyId == user.CompanyId,
                    IsProvider = r.ProviderCompanyId == user.CompanyId,
                    RelatedCompany = r.ClientCompanyId == user.CompanyId 
                        ? new { Id = r.ProviderCompanyId, Name = r.ProviderCompany.Name, Cnpj = r.ProviderCompany.Cnpj }
                        : new { Id = r.ClientCompanyId, Name = r.ClientCompany.Name, Cnpj = r.ClientCompany.Cnpj }
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
    /// Obter relacionamentos onde a empresa é cliente (contrata outras empresas)
    /// </summary>
    [HttpGet("as-client")]
    public async Task<IActionResult> GetAsClient()
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
                Type = r.Type.ToString(),
                Status = r.Status.ToString(),
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Notes = r.Notes,
                ContractedCompany = new 
                { 
                    Id = r.ProviderCompanyId, 
                    Name = r.ProviderCompany.Name, 
                    Cnpj = r.ProviderCompany.Cnpj,
                    BusinessModel = r.ProviderCompany.BusinessModel.ToString()
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
    [HttpGet("as-provider")]
    public async Task<IActionResult> GetAsProvider()
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
                Type = r.Type.ToString(),
                Status = r.Status.ToString(),
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Notes = r.Notes,
                ClientCompany = new 
                { 
                    Id = r.ClientCompanyId, 
                    Name = r.ClientCompany.Name, 
                    Cnpj = r.ClientCompany.Cnpj,
                    BusinessModel = r.ClientCompany.BusinessModel.ToString()
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
    [HttpPut("{relationshipId:guid}/activate")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> ActivateRelationship(Guid relationshipId)
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
                message = "Relacionamento ativado com sucesso",
                relationshipId = relationshipId,
                status = "Active",
                updatedAt = relationship.UpdatedAt
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
    [HttpPut("{relationshipId:guid}/deactivate")]
    [Authorize(Roles = "Admin,Company")]
    public async Task<IActionResult> DeactivateRelationship(Guid relationshipId)
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
                message = "Relacionamento desativado com sucesso",
                relationshipId = relationshipId,
                status = "Inactive",
                updatedAt = relationship.UpdatedAt,
                note = "PJ não terá mais acesso aos sistemas da empresa contratante"
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
    [HttpGet("{relationshipId:guid}/users")]
    [Authorize]
    public async Task<IActionResult> GetRelationshipUsers(Guid relationshipId)
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
            if (currentUser.Role == Domain.Enums.UserRole.Provider)
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

            if (currentUser.Role == Domain.Enums.UserRole.Provider)
            {
                // PJs só podem ver contatos principais (Admin/Company) da empresa contratante
                filteredClientUsers = clientUsers.Where(u => 
                    u.Role == Domain.Enums.UserRole.Admin || 
                    u.Role == Domain.Enums.UserRole.Company);
            }

            var result = new
            {
                RelationshipId = relationship.Id,
                RelationshipType = relationship.Type.ToString(),
                RelationshipStatus = relationship.Status.ToString(),
                StartDate = relationship.StartDate,
                EndDate = relationship.EndDate,
                Notes = relationship.Notes,
                ClientCompany = new
                {
                    Id = clientCompany.Id,
                    Name = clientCompany.Name,
                    Cnpj = clientCompany.Cnpj,
                    BusinessModel = clientCompany.BusinessModel.ToString(),
                    Users = filteredClientUsers.Select(u => new
                    {
                        UserId = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role.ToString(),
                        CreatedAt = u.CreatedAt,
                        IsAccessible = true
                    })
                },
                ProviderCompany = new
                {
                    Id = providerCompany.Id,
                    Name = providerCompany.Name,
                    Cnpj = providerCompany.Cnpj,
                    BusinessModel = providerCompany.BusinessModel.ToString(),
                    Users = filteredProviderUsers.Select(u => new
                    {
                        UserId = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role.ToString(),
                        CreatedAt = u.CreatedAt,
                        IsAccessible = true
                    })
                },
                TotalUsers = filteredClientUsers.Count() + filteredProviderUsers.Count(),
                CurrentUserAccess = new
                {
                    CanSeeClientUsers = true,
                    CanSeeProviderUsers = true,
                    IsFromClientCompany = currentUser.CompanyId == relationship.ClientCompanyId,
                    IsFromProviderCompany = currentUser.CompanyId == relationship.ProviderCompanyId,
                    UserRole = currentUser.Role.ToString()
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