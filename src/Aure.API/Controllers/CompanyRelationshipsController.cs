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
    /// Obter todos os relacionamentos ativos da empresa (como cliente ou provedor)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCompanyRelationships()
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

            var relationships = await _unitOfWork.CompanyRelationships.GetActiveRelationshipsByCompanyIdAsync(user.CompanyId.Value);

            var result = relationships.Select(r => new
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
            });

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
}