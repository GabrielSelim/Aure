using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Aure.Application.DTOs.Company;
using Aure.Application.Interfaces;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        ICompanyService companyService,
        ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("empresa-pai")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Visualizar dados da empresa pai",
        Description = @"Retorna informações completas da empresa pai vinculada ao usuário autenticado.

**Visível para:**
- DonoEmpresaPai
- Financeiro
- Jurídico
- Funcionários CLT e PJ (dados limitados)

**Informações Retornadas:**
- Razão Social e CNPJ
- Tipo da empresa e modelo de negócio
- Endereço completo (sincronizado com endereço do DonoEmpresaPai)
- Estatísticas: Total de funcionários, contratos ativos
- Data de cadastro

**Sincronização de Endereço:**
- Para DonoEmpresaPai: Endereço pessoal = Endereço da empresa (sincronização automática)
- Ao atualizar endereço do perfil do Dono, atualiza empresa também
- Ao atualizar endereço da empresa, atualiza perfil do Dono também"
    )]
    [SwaggerResponse(200, "Dados da empresa pai", typeof(CompanyInfoResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(404, "Empresa não encontrada ou usuário sem empresa vinculada")]
    [ProducesResponseType(typeof(CompanyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyParent()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _companyService.GetCompanyParentInfoAsync(userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresa pai");
            return StatusCode(500, new { message = "Erro ao buscar empresa pai" });
        }
    }

    [HttpPut("empresa-pai")]
    [Authorize(Roles = "DonoEmpresaPai")]
    [SwaggerOperation(
        Summary = "Atualizar empresa pai (SOMENTE DonoEmpresaPai)",
        Description = @"Permite que o DonoEmpresaPai atualize dados da empresa pai.

**Restrições:**
- Apenas DonoEmpresaPai pode executar
- Validação via JWT (usuário só edita própria empresa)

**Sincronização Bidirecional de Endereço:**
- ✅ Ao atualizar endereço da empresa aqui, atualiza também o endereço pessoal do DonoEmpresaPai
- ✅ Ao atualizar endereço no perfil pessoal, também atualiza endereço da empresa
- ⚠️ Isso garante que empresa e dono sempre tenham mesmo endereço

**Campos Editáveis:**
- Razão Social
- CNPJ (com validação na Receita Federal e fluxo de divergência)
- Endereço completo (Rua, Número, Complemento, Bairro, Cidade, Estado, País, CEP)

**Validação de CNPJ:**
- Se o CNPJ for alterado, será validado na Receita Federal
- Se a Razão Social divergir (similaridade < 85%), será solicitada confirmação
- Use `ConfirmarDivergenciaRazaoSocial = true` para confirmar alteração mesmo com divergência

**Campos NÃO Editáveis:**
- CompanyType (imutável - definido na criação)
- BusinessModel (imutável - definido na criação)
- Estatísticas (calculadas automaticamente)"
    )]
    [SwaggerResponse(200, "Empresa pai atualizada com sucesso", typeof(UpdateCompanyParentResponse))]
    [SwaggerResponse(400, "Dados inválidos ou divergência de Razão Social", typeof(UpdateCompanyParentResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Apenas DonoEmpresaPai pode atualizar empresa pai")]
    [SwaggerResponse(404, "Empresa não encontrada")]
    [ProducesResponseType(typeof(UpdateCompanyParentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCompanyParent([FromBody] UpdateCompanyParentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _companyService.UpdateCompanyParentAsync(userId, request);

            if (!result.Sucesso && result.RequerConfirmacao)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa pai");
            return StatusCode(500, new { message = "Erro ao atualizar empresa pai" });
        }
    }
}
