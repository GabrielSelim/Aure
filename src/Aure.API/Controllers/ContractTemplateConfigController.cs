using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Aure.Application.DTOs.Contract;
using Aure.Application.Interfaces;

namespace Aure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractTemplateConfigController : ControllerBase
    {
        private readonly IContractTemplateConfigService _service;
        private readonly ILogger<ContractTemplateConfigController> _logger;

        public ContractTemplateConfigController(
            IContractTemplateConfigService service,
            ILogger<ContractTemplateConfigController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("presets")]
        [ProducesResponseType(typeof(List<ContractTemplatePresetResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPresets()
        {
            var result = await _service.GetPresetsAsync();
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }

        [HttpGet("presets/{tipo}")]
        [ProducesResponseType(typeof(ContractTemplatePresetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPresetByTipo(string tipo)
        {
            var result = await _service.GetPresetByTipoAsync(tipo);
            
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Data);
        }

        [HttpGet("config")]
        [ProducesResponseType(typeof(List<ContractTemplateConfigResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCompanyConfigs()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.GetAllCompanyConfigsAsync(userId);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }

        [HttpGet("config/{nomeConfig}")]
        [ProducesResponseType(typeof(ContractTemplateConfigResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyConfigByNome(string nomeConfig)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.GetCompanyConfigByNomeAsync(userId, nomeConfig);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            if (result.Data == null)
                return NotFound(new { message = $"Configuração '{nomeConfig}' não encontrada" });

            return Ok(result.Data);
        }

        [HttpPost("config")]
        [Authorize(Roles = "DonoEmpresaPai")]
        [ProducesResponseType(typeof(ContractTemplateConfigResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateOrUpdateConfig([FromBody] ContractTemplateConfigRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.CreateOrUpdateConfigAsync(userId, request);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }

        [HttpPost("clonar-preset/{tipoPreset}")]
        [Authorize(Roles = "DonoEmpresaPai")]
        [ProducesResponseType(typeof(ContractTemplateConfigResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClonarPreset(string tipoPreset, [FromBody] ClonarPresetRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.ClonarPresetAsync(userId, tipoPreset, request.NomeConfig);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }

        [HttpPost("preview")]
        [Authorize(Roles = "DonoEmpresaPai")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PreviewContract([FromBody] PreviewTemplateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.PreviewContractHtmlAsync(userId, request);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Content(result.Data!, "text/html");
        }

        [HttpPost("gerar-contrato")]
        [Authorize(Roles = "DonoEmpresaPai,Juridico")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GerarContrato([FromBody] GerarContratoComConfigRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.GerarContratoComConfigAsync(userId, request);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return CreatedAtAction(
                "ObterContratoPorId",
                "Contracts",
                new { id = result.Data },
                new { contractId = result.Data, message = "Contrato criado com sucesso" }
            );
        }

        [HttpDelete("config/{nomeConfig}")]
        [Authorize(Roles = "DonoEmpresaPai")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteConfig(string nomeConfig)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.DeleteCompanyConfigAsync(userId, nomeConfig);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = $"Configuração '{nomeConfig}' deletada com sucesso" });
        }
    }
}
