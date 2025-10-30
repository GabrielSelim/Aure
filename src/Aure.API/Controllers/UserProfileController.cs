using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Aure.Application.DTOs.User;
using Aure.Application.DTOs.Company;
using Aure.Application.Interfaces;
using Aure.Domain.Interfaces;
using AvatarSvc = Aure.API.Services;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly AvatarSvc.IAvatarService _avatarService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        IUserProfileService userProfileService,
        AvatarSvc.IAvatarService avatarService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService;
        _avatarService = avatarService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Usuário não autenticado");
        
        return userId;
    }

    [HttpGet("perfil")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obter perfil do usuário autenticado",
        Description = "Retorna os dados completos do perfil do usuário logado"
    )]
    [SwaggerResponse(200, "Perfil retornado com sucesso", typeof(UserProfileResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(404, "Usuário não encontrado")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerfil()
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userProfileService.GetUserProfileAsync(userId, userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar perfil do usuário");
            return StatusCode(500, new { message = "Erro ao buscar perfil" });
        }
    }

    [HttpPut("perfil-completo")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Atualizar perfil completo do usuário autenticado",
        Description = @"Permite usuário atualizar seu próprio perfil com todos os campos disponíveis.

**Campos Editáveis por TODOS os usuários:**
- Nome, email, telefones (celular e fixo)
- Data de nascimento
- Endereço completo (8 campos)
- Senha (requer senhaAtual para validação)

**Campos Específicos por Role:**
- **Cargo**: Apenas FuncionarioCLT (role 4) e FuncionarioPJ (role 5)
- **CPF/RG**: Todos podem editar (serão criptografados automaticamente)

**Validações:**
- Email único no sistema
- CPF único no sistema
- Data nascimento: Idade entre 16 e 100 anos
- Senha: Requer senhaAtual correta para alteração"
    )]
    [SwaggerResponse(200, "Perfil atualizado com sucesso", typeof(UserProfileResponse))]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(401, "Não autenticado")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePerfilCompleto([FromBody] UpdateFullProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userProfileService.UpdateFullProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil do usuário");
            return StatusCode(500, new { message = "Erro ao atualizar perfil" });
        }
    }

    [HttpGet("notificacoes/preferencias")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obter preferências de notificação",
        Description = "Retorna as preferências de notificação por email do usuário"
    )]
    [SwaggerResponse(200, "Preferências retornadas com sucesso", typeof(NotificationPreferencesDTO))]
    [SwaggerResponse(401, "Não autenticado")]
    [ProducesResponseType(typeof(NotificationPreferencesDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _userProfileService.GetNotificationPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar preferências de notificação");
            return StatusCode(500, new { message = "Erro ao buscar preferências" });
        }
    }

    [HttpPut("notificacoes/preferencias")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Atualizar preferências de notificação por email",
        Description = @"Define quais tipos de email o usuário deseja receber.

**Preferências Disponíveis por Role:**

**DonoEmpresaPai (role 1):**
- Contratos (novo, assinado, vencendo)
- Pagamentos (processado, alertas financeiros)
- Operações (novos funcionários)
- Sistema (atualizações)

**Financeiro (role 2):**
- Contratos (novo, assinado, vencendo)
- Pagamentos (processado - notificação interna)
- Operações (novos funcionários)
- Sistema

**Juridico (role 3):**
- Contratos (novo, assinado, vencendo)
- Operações (novos funcionários)
- Sistema

**FuncionarioPJ (role 5):**
- Contratos (vencendo - próprios contratos)
- Pagamentos (recebido)
- Sistema

**FuncionarioCLT (role 4):**
- Sistema

**Padrão:** Todas as preferências iniciam como `true`"
    )]
    [SwaggerResponse(200, "Preferências atualizadas com sucesso", typeof(NotificationPreferencesDTO))]
    [SwaggerResponse(401, "Não autenticado")]
    [ProducesResponseType(typeof(NotificationPreferencesDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateNotificationPreferences(
        [FromBody] NotificationPreferencesDTO preferences)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updated = await _userProfileService.UpdateNotificationPreferencesAsync(userId, preferences);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar preferências de notificação");
            return StatusCode(500, new { message = "Erro ao atualizar preferências" });
        }
    }

    [HttpPost("aceitar-termos")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Aceitar termos de uso e política de privacidade",
        Description = @"Registra aceite dos termos de uso e política de privacidade (separados).

**Validações:**
- Ambos documentos podem ser aceitos em requisições separadas
- Sistema registra data/hora e versão de cada aceite
- Auditoria completa de aceites"
    )]
    [SwaggerResponse(200, "Termos aceitos com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(401, "Não autenticado")]
    public async Task<IActionResult> AcceptTerms([FromBody] AcceptTermsRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userProfileService.AcceptTermsAsync(userId, request);
            return Ok(new { message = "Termos aceitos com sucesso" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar termos");
            return StatusCode(500, new { message = "Erro ao aceitar termos" });
        }
    }

    [HttpGet("termos/versoes")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obter versões atuais dos termos",
        Description = "Retorna versões atuais de termos de uso e política de privacidade, e se usuário precisa aceitar"
    )]
    [SwaggerResponse(200, "Versões retornadas com sucesso", typeof(TermsVersionsResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [ProducesResponseType(typeof(TermsVersionsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTermsVersions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var versions = await _userProfileService.GetTermsVersionsAsync(userId);
            return Ok(versions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar versões dos termos");
            return StatusCode(500, new { message = "Erro ao buscar versões" });
        }
    }

    [HttpPost("avatar")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Upload de avatar/foto de perfil",
        Description = @"Faz upload de uma imagem de avatar para o usuário autenticado.

**Formatos Aceitos:** JPG, JPEG, PNG
**Tamanho Máximo:** 5MB

**Processamento Automático:**
- Crop quadrado (1:1)
- Resize para 400x400px (original)
- Resize para 80x80px (thumbnail)
- Compressão automática (JPEG com qualidade 85%)

**Storage:** Local filesystem em `/wwwroot/uploads/avatars/`
**Fallback:** Se não houver avatar, sistema usa iniciais do nome"
    )]
    [SwaggerResponse(200, "Avatar enviado com sucesso", typeof(AvatarSvc.AvatarUploadResponse))]
    [SwaggerResponse(400, "Arquivo inválido ou muito grande")]
    [SwaggerResponse(401, "Não autenticado")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AvatarSvc.AvatarUploadResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            var avatarUrl = await _avatarService.UploadAvatarAsync(file, userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.UpdateAvatar(avatarUrl);
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            var thumbnailUrl = avatarUrl.Replace(".jpg", "_thumb.jpg");

            return Ok(new AvatarSvc.AvatarUploadResponse
            {
                AvatarUrl = avatarUrl,
                ThumbnailUrl = thumbnailUrl
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload de avatar");
            return StatusCode(500, new { message = "Erro ao processar avatar" });
        }
    }

    [HttpDelete("avatar")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Remover avatar/foto de perfil",
        Description = @"Remove o avatar do usuário autenticado.

**Comportamento:**
- Deleta arquivo original (400x400px)
- Deleta thumbnail (80x80px)
- Sistema volta a usar iniciais do nome como fallback

**Nota:** Operação permanente - avatar não pode ser recuperado"
    )]
    [SwaggerResponse(200, "Avatar removido com sucesso")]
    [SwaggerResponse(401, "Não autenticado")]
    public async Task<IActionResult> DeleteAvatar()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _avatarService.DeleteAvatarAsync(userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.UpdateAvatar(null);
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            return Ok(new { message = "Avatar removido com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar avatar");
            return StatusCode(500, new { message = "Erro ao remover avatar" });
        }
    }

    [HttpGet("empresa")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obter dados da empresa do usuário autenticado",
        Description = @"Retorna os dados da empresa em que o usuário trabalha.

**Disponível para todos os usuários autenticados:**
- DonoEmpresaPai: Visualiza dados da empresa pai
- Financeiro: Visualiza dados da empresa pai
- Jurídico: Visualiza dados da empresa pai
- FuncionarioCLT: Visualiza dados da empresa pai
- FuncionarioPJ: Visualiza dados da empresa pai (não confundir com empresa PJ do funcionário)

**Dados Retornados:**
- Nome da empresa
- CNPJ (normal e formatado)
- Tipo de empresa
- Modelo de negócio
- Telefones (celular e fixo)
- Endereço completo"
    )]
    [SwaggerResponse(200, "Dados da empresa retornados com sucesso", typeof(UserCompanyInfoResponse))]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(404, "Empresa não encontrada")]
    [ProducesResponseType(typeof(UserCompanyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmpresa()
    {
        try
        {
            var userId = GetCurrentUserId();
            var companyService = HttpContext.RequestServices.GetRequiredService<ICompanyService>();
            var result = await companyService.GetCompanyInfoByUserIdAsync(userId);
            
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados da empresa");
            return StatusCode(500, new { message = "Erro ao buscar dados da empresa" });
        }
    }

    [HttpPut("empresa")]
    [Authorize(Roles = "DonoEmpresaPai")]
    [SwaggerOperation(
        Summary = "Atualizar dados da empresa (SOMENTE DonoEmpresaPai)",
        Description = @"Permite que o dono da empresa atualize os dados cadastrais da empresa pai.

**Restrição de Acesso:**
- Apenas usuários com role DonoEmpresaPai (role 1)
- Só pode atualizar a própria empresa (validação via JWT)

**Campos Editáveis:**
- Nome da empresa (razão social)
- Telefone celular (obrigatório - 10 ou 11 dígitos)
- Telefone fixo (opcional - 10 dígitos)
- Endereço completo (rua, número, complemento, bairro, cidade, estado, país, CEP)

**Campos NÃO Editáveis:**
- CNPJ (imutável)
- Tipo de empresa (imutável)
- Modelo de negócio (imutável)

**Validações:**
- Estado: Deve ter exatamente 2 caracteres (sigla)
- CEP: Deve ter exatamente 8 dígitos
- Telefones: Apenas números, sem formatação"
    )]
    [SwaggerResponse(200, "Empresa atualizada com sucesso", typeof(UserCompanyInfoResponse))]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Apenas DonoEmpresaPai pode atualizar a empresa")]
    [SwaggerResponse(404, "Empresa não encontrada")]
    [ProducesResponseType(typeof(UserCompanyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEmpresa([FromBody] UpdateUserCompanyInfoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var companyService = HttpContext.RequestServices.GetRequiredService<ICompanyService>();
            var result = await companyService.UpdateCompanyInfoAsync(userId, request);
            
            if (!result.IsSuccess)
            {
                if (result.Error.Contains("Apenas o dono"))
                    return StatusCode(403, new { message = result.Error });
                    
                return BadRequest(new { message = result.Error });
            }
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar dados da empresa");
            return StatusCode(500, new { message = "Erro ao atualizar dados da empresa" });
        }
    }

    [HttpPut("empresa-pj")]
    [Authorize(Roles = "FuncionarioPJ")]
    [SwaggerOperation(
        Summary = "Atualizar empresa PJ (SOMENTE FuncionarioPJ)",
        Description = @"Permite que um funcionário PJ atualize os dados da própria empresa.

**Validações Implementadas:**
1. **Formato CNPJ**: Valida se tem 14 dígitos
2. **Unicidade**: Verifica se CNPJ já está cadastrado
3. **Receita Federal**: Consulta API da Receita para validar CNPJ
4. **Divergência de Razão Social**: 
   - Compara Razão Social informada com a registrada na Receita
   - Usa algoritmo de similaridade (Levenshtein)
   - Se similaridade < 85%, requer confirmação do usuário
   - Frontend deve mostrar modal com as duas razões sociais

**Fluxo de Divergência:**
1. Usuário envia CNPJ + Razão Social
2. Sistema detecta divergência
3. Retorna `DivergenciaRazaoSocial = true` com ambas as razões sociais
4. Frontend mostra modal de confirmação
5. Usuário reenvia com `ConfirmarDivergenciaRazaoSocial = true`
6. Sistema aceita e registra na auditoria

**Restrições:**
- Apenas FuncionarioPJ pode executar
- Só pode atualizar própria empresa (validação via JWT)
- Endereço da empresa é independente do endereço pessoal do usuário"
    )]
    [SwaggerResponse(200, "Empresa PJ atualizada com sucesso", typeof(UpdateCompanyPJResponse))]
    [SwaggerResponse(400, "Dados inválidos ou CNPJ duplicado")]
    [SwaggerResponse(401, "Não autenticado")]
    [SwaggerResponse(403, "Apenas FuncionarioPJ pode atualizar empresa PJ")]
    [ProducesResponseType(typeof(UpdateCompanyPJResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCompanyPJ([FromBody] UpdateCompanyPJRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userProfileService.UpdateCompanyPJAsync(userId, request);

            if (!result.Sucesso)
                return BadRequest(result);

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
            _logger.LogError(ex, "Erro ao atualizar empresa PJ");
            return StatusCode(500, new { message = "Erro ao atualizar empresa PJ" });
        }
    }
}
