using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Aure.Application.DTOs.User;
using Aure.Application.Interfaces;
using System.Security.Claims;

namespace Aure.API.Controllers;

/// <summary>
/// Controller para gerenciamento pessoal de usuários
/// 
/// FLUXO DE USUÁRIOS:
/// 1. Admin registra empresa via /api/registration/company-admin
/// 2. Admin convida usuários via /api/registration/invite (sem senha)
/// 3. Usuário aceita convite via /api/registration/accept-invite/{token} (define senha)
/// 4. Usuário faz login via /api/auth/login
/// 
/// ENDPOINTS DISPONÍVEIS:
/// - GET /api/users/{id} - Busca usuário específico (próprio usuário ou da rede de relacionamentos)
/// - PUT /api/users/profile - Usuário atualiza seus próprios dados (nome, email)
/// - PATCH /api/users/password - Usuário troca própria senha
/// 
/// NOTA: Para listar usuários da empresa, use /api/CompanyRelationships
/// NOTA: Para gerenciar PJs, use os endpoints de CompanyRelationships
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
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
    /// Usuário atualiza seus próprios dados (nome, email)
    /// </summary>
    [HttpPut("perfil")]
    [Authorize]
    public async Task<IActionResult> UpdateOwnProfile([FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            _logger.LogWarning("ID de usuário inválido do token");
            return BadRequest(new { Erro = "Autenticação de usuário inválida" });
        }

        var result = await _userService.UpdateAsync(currentUserId, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao atualizar perfil do usuário {UserId}: {Error}", currentUserId, result.Error);
            return BadRequest(new { Erro = result.Error });
        }

        _logger.LogInformation("Perfil atualizado com sucesso para o usuário {UserId}", currentUserId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Usuário troca sua própria senha
    /// </summary>
    [HttpPatch("senha")]
    [Authorize]
    public async Task<IActionResult> ChangeOwnPassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            _logger.LogWarning("ID de usuário inválido do token");
            return BadRequest(new { Erro = "Autenticação de usuário inválida" });
        }

        var result = await _userService.ChangePasswordAsync(currentUserId, request);
        
        if (result.IsFailure)
        {
            _logger.LogError("Falha ao trocar senha do usuário {UserId}: {Error}", currentUserId, result.Error);
            return BadRequest(new { Erro = result.Error });
        }

        _logger.LogInformation("Senha trocada com sucesso para o usuário {UserId}", currentUserId);
        return NoContent();
    }

    [HttpGet("funcionarios")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Listar funcionários da empresa",
        Description = @"Retorna lista paginada de funcionários da empresa com filtros.

**Permissões por Role:**
- **DonoEmpresaPai**: Vê TODOS os funcionários (CLT, PJ, Financeiro, Jurídico)
- **Financeiro/Jurídico**: Vê apenas funcionários CLT e PJ (não vê outros Financeiro/Jurídico)
- **Outros roles**: Não têm acesso

**Filtros Disponíveis:**
- `role`: Filtrar por tipo de funcionário (FuncionarioCLT, FuncionarioPJ, etc)
- `cargo`: Filtrar por cargo (ex: 'Desenvolvedor', 'Designer')
- `status`: Filtrar por status ('Ativo', 'Inativo')
- `busca`: Buscar por nome ou email

**Paginação:**
- `pageNumber`: Número da página (padrão: 1)
- `pageSize`: Itens por página (padrão: 20, máx: 100)

**Informações Retornadas:**
- Nome, Email, Role, Cargo
- Status (Ativo/Inativo)
- Data de entrada
- Telefone celular
- Nome da empresa PJ (se FuncionarioPJ)

**Observações:**
- CPF é mascarado em listagens (***.***.123-45)
- Dados sensíveis completos apenas em visualização individual
- Lista ordenada por nome (A-Z)"
    )]
    [SwaggerResponse(200, "Lista de funcionários", typeof(PagedResult<EmployeeListItemResponse>))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Sem permissão para listar funcionários")]
    [ProducesResponseType(typeof(PagedResult<EmployeeListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        [FromQuery] string? cargo = null,
        [FromQuery] string? status = null,
        [FromQuery] string? busca = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var request = new EmployeeListRequest
            {
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100),
                Role = string.IsNullOrEmpty(role) ? null : Enum.Parse<Domain.Enums.UserRole>(role),
                Cargo = cargo,
                Status = status,
                Busca = busca
            };

            var result = await _userService.GetEmployeesAsync(userId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar funcionários");
            return StatusCode(500, new { message = "Erro ao listar funcionários" });
        }
    }

    [HttpGet("exportar-dados")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Exportar dados do usuário (LGPD Art. 18, IV)",
        Description = @"Exporta todos os dados pessoais do usuário conforme LGPD.

**Dados Exportados:**
- **Dados Pessoais**: Nome, Email, CPF (descriptografado), RG, Data Nascimento, Telefones, Endereço completo, Cargo, Avatar
- **Dados da Empresa**: Razão Social, CNPJ, Tipo (se aplicável)
- **Histórico de Contratos**: Todos os contratos vinculados (ativos, finalizados, cancelados)
- **Histórico de Pagamentos**: Todos os pagamentos recebidos (apenas para FuncionarioPJ)
- **Preferências de Notificação**: Configurações de email
- **Aceites de Termos**: Datas e versões aceitas
- **Metadata**: Data da exportação

**Conformidade LGPD:**
- Art. 18, IV: Portabilidade dos dados a outro fornecedor de serviço
- Dados entregues em formato estruturado (JSON)
- Inclui CPF/RG descriptografados para portabilidade completa

**Segurança:**
- Apenas o próprio usuário pode exportar seus dados
- Endpoint requer autenticação
- Ação é logada para auditoria

**Formato:** JSON estruturado"
    )]
    [SwaggerResponse(200, "Dados exportados com sucesso", typeof(UserDataExportResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(404, "Usuário não encontrado")]
    [ProducesResponseType(typeof(UserDataExportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportUserData()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.ExportUserDataAsync(userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar dados do usuário");
            return StatusCode(500, new { message = "Erro ao exportar dados" });
        }
    }

    [HttpDelete("solicitar-exclusao")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Solicitar exclusão de conta (LGPD Art. 18, VI)",
        Description = @"Anonimiza a conta do usuário conforme LGPD e legislação fiscal brasileira.

**Processo de Anonimização:**
- ✅ Nome → ""Usuário Removido {ID}""
- ✅ Email → ""removed_{ID}@aure.deleted""
- ✅ Telefones → NULL
- ✅ Endereço completo → NULL
- ✅ Avatar → NULL
- ✅ Cargo → NULL
- ✅ IsDeleted → TRUE
- ⚠️ CPF/RG → **MANTIDOS CRIPTOGRAFADOS** (auditoria fiscal)

**Dados Mantidos (Legislação Fiscal):**
- Contratos e documentos contratuais (5 anos - Lei 8.934/94)
- Notas Fiscais e documentos fiscais (5 anos - Código Civil)
- Pagamentos e registros financeiros (5 anos)
- CPF e RG criptografados (apenas para auditoria)

**Validações:**
- ❌ Não pode ter contratos ativos
- ❌ Conta já excluída
- ✅ Encerrar ou transferir contratos antes

**Conformidade LGPD:**
- Art. 18, VI: Eliminação dos dados pessoais tratados com consentimento
- Balanceamento entre LGPD e obrigações fiscais
- Mantém apenas dados necessários para compliance fiscal

**⚠️ Ação Irreversível:**
Esta ação não pode ser desfeita. Após anonimização, você não poderá mais acessar o sistema com esta conta."
    )]
    [SwaggerResponse(200, "Conta anonimizada com sucesso", typeof(AccountDeletionResponse))]
    [SwaggerResponse(400, "Não é possível excluir conta (contratos ativos ou já excluída)")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(404, "Usuário não encontrado")]
    [ProducesResponseType(typeof(AccountDeletionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestAccountDeletion()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.RequestAccountDeletionAsync(userId);
            
            if (!result.Sucesso)
                return BadRequest(result);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar exclusão de conta");
            return StatusCode(500, new { message = "Erro ao processar exclusão" });
        }
    }

    [HttpPut("{employeeId}/cargo")]
    [Authorize(Roles = "DonoEmpresaPai,Juridico")]
    [SwaggerOperation(
        Summary = "Atualizar cargo de usuário",
        Description = @"Permite que o dono da empresa ou jurídico altere o cargo de qualquer usuário da empresa.

**Quem Pode Alterar:**
- DonoEmpresaPai pode alterar cargos de: Financeiro, Juridico, FuncionarioCLT, FuncionarioPJ
- Juridico pode alterar cargos de: Financeiro, Juridico (outros), FuncionarioCLT, FuncionarioPJ
- Não é possível alterar o cargo do proprietário

**Exemplos de Cargos:**
- **Financeiro**: Gerente Financeiro, Analista Contábil, Controller, CFO
- **Jurídico**: Advogado Contratual, Advogado Corporativo, Consultor Jurídico, Gerente Jurídico
- **FuncionarioCLT**: Analista de TI, Gerente de Vendas, Coordenador, Assistente
- **FuncionarioPJ**: Desenvolvedor Full Stack, Consultor, Designer, Arquiteto de Software

**Validações:**
- Cargo não pode ser vazio
- Cargo deve ter no máximo 100 caracteres
- Usuário deve pertencer à mesma empresa"
    )]
    [SwaggerResponse(200, "Cargo atualizado com sucesso", typeof(UserResponse))]
    [SwaggerResponse(400, "Requisição inválida")]
    [SwaggerResponse(401, "Não autenticado ou sem permissão")]
    [SwaggerResponse(404, "Funcionário não encontrado")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarCargoFuncionario(Guid employeeId, [FromBody] UpdateEmployeePositionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UpdateEmployeePositionAsync(employeeId, request.Cargo, userId);
            
            if (result.IsFailure)
                return BadRequest(new { erro = result.Error });
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cargo do funcionário {EmployeeId}", employeeId);
            return StatusCode(500, new { message = "Erro ao atualizar cargo do funcionário" });
        }
    }
}
