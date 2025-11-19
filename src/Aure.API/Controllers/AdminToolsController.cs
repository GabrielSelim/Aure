using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Application.Interfaces;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "DonoEmpresaPai")]
public class AdminToolsController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;

    public AdminToolsController(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    [HttpPost("encrypt-data")]
    public IActionResult EncryptData([FromBody] EncryptDataRequest request)
    {
        var encrypted = _encryptionService.Encrypt(request.Data);
        return Ok(new { encrypted });
    }
}

public class EncryptDataRequest
{
    public string Data { get; set; } = string.Empty;
}
