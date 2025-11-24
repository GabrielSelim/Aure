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
        [ProducesResponseType(typeof(ContractTemplateConfigResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyConfig()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.GetCompanyConfigAsync(userId);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            if (result.Data == null)
                return NotFound(new { message = "Empresa ainda não possui configuração de template. Configure um template para começar a gerar contratos personalizados." });

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

        [HttpDelete("config")]
        [Authorize(Roles = "DonoEmpresaPai")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteConfig()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _service.DeleteCompanyConfigAsync(userId);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = "Configuração deletada com sucesso" });
        }
    }
}
