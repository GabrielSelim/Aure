using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Aure.Domain.Interfaces;
using Aure.Domain.Entities;
using System.Security.Claims;

namespace Aure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TokenizedAssetsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TokenizedAssetsController> _logger;

    public TokenizedAssetsController(IUnitOfWork unitOfWork, ILogger<TokenizedAssetsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Tokenizar um contrato na blockchain
    /// </summary>
    [HttpPost("tokenizar")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> TokenizeContract([FromBody] TokenizeContractRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { mensagem = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { mensagem = "Usuário ou empresa não encontrados" });
            }

            var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ContractId);
            if (contract == null)
            {
                return NotFound(new { mensagem = "Contrato não encontrado" });
            }

            // Verificar se o usuário tem acesso ao contrato
            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid("Usuário não tem acesso a este contrato");
            }

            // Verificar se o contrato já foi tokenizado
            var existingToken = await _unitOfWork.TokenizedAssets.GetByContractIdAsync(request.ContractId);
            if (existingToken != null)
            {
                return BadRequest(new { mensagem = "Contrato já foi tokenizado" });
            }

            var tokenizedAsset = new TokenizedAsset(
                request.ContractId,
                request.TokenAddress,
                request.ChainId,
                request.TransactionHash
            );

            await _unitOfWork.TokenizedAssets.AddAsync(tokenizedAsset);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Contrato {ContractId} tokenizado com sucesso", request.ContractId);

            return Ok(new
            {
                Id = tokenizedAsset.Id,
                ContratoId = tokenizedAsset.ContractId,
                EnderecoToken = tokenizedAsset.TokenAddress,
                ChainId = tokenizedAsset.ChainId,
                HashTransacao = tokenizedAsset.TxHash,
                DataCriacao = tokenizedAsset.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao tokenizar contrato {ContractId}", request.ContractId);
            return BadRequest(new { mensagem = "Erro ao tokenizar contrato" });
        }
    }

    /// <summary>
    /// Obter informações de tokenização de um contrato
    /// </summary>
    [HttpGet("contrato/{contractId:guid}")]
    public async Task<IActionResult> GetPorContrato(Guid contractId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { mensagem = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { mensagem = "Usuário ou empresa não encontrados" });
            }

            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            if (contract == null)
            {
                return NotFound(new { mensagem = "Contrato não encontrado" });
            }

            // Verificar se o usuário tem acesso ao contrato
            if (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId)
            {
                return Forbid("Usuário não tem acesso a este contrato");
            }

            var tokenizedAsset = await _unitOfWork.TokenizedAssets.GetByContractIdAsync(contractId);
            if (tokenizedAsset == null)
            {
                return NotFound(new { mensagem = "Contrato não foi tokenizado" });
            }

            return Ok(new
            {
                Id = tokenizedAsset.Id,
                ContratoId = tokenizedAsset.ContractId,
                EnderecoToken = tokenizedAsset.TokenAddress,
                ChainId = tokenizedAsset.ChainId,
                HashTransacao = tokenizedAsset.TxHash,
                DataCriacao = tokenizedAsset.CreatedAt,
                UltimaAtualizacao = tokenizedAsset.UpdatedAt,
                Contrato = new
                {
                    Id = contract.Id,
                    Titulo = contract.Title,
                    ValorTotal = contract.ValueTotal,
                    Status = contract.Status.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tokenização do contrato {ContractId}", contractId);
            return BadRequest(new { mensagem = "Erro ao buscar tokenização do contrato" });
        }
    }

    /// <summary>
    /// Listar todos os contratos tokenizados da empresa
    /// </summary>
    [HttpGet("listar")]
    public async Task<IActionResult> ListarAtivos([FromQuery] int? chainId = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { mensagem = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { mensagem = "Usuário ou empresa não encontrados" });
            }

            var contracts = await _unitOfWork.Contracts.GetByCompanyIdAsync(user.CompanyId.Value);
            var contractIds = contracts.Select(c => c.Id).ToList();

            var allTokenizedAssets = await _unitOfWork.TokenizedAssets.GetAllAsync();
            var companyTokenizedAssets = allTokenizedAssets.Where(ta => contractIds.Contains(ta.ContractId));

            if (chainId.HasValue)
            {
                companyTokenizedAssets = companyTokenizedAssets.Where(ta => ta.ChainId == chainId.Value);
            }

            var result = companyTokenizedAssets.Select(ta => new
            {
                Id = ta.Id,
                ContratoId = ta.ContractId,
                EnderecoToken = ta.TokenAddress,
                ChainId = ta.ChainId,
                HashTransacao = ta.TxHash,
                DataCriacao = ta.CreatedAt,
                UltimaAtualizacao = ta.UpdatedAt
            });

            return Ok(new
            {
                TotalTokenizados = result.Count(),
                ContratosTokenizados = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar contratos tokenizados");
            return BadRequest(new { mensagem = "Erro ao listar contratos tokenizados" });
        }
    }

    /// <summary>
    /// Atualizar informações de tokenização
    /// </summary>
    [HttpPut("{id:guid}")]
    [HttpPut("{id:guid}/atualizar")]
    [Authorize(Roles = "DonoEmpresaPai")]
    public async Task<IActionResult> UpdateTokenizedAsset(Guid id, [FromBody] UpdateTokenizedAssetRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { mensagem = "Token de usuário inválido" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.CompanyId == null)
            {
                return NotFound(new { mensagem = "Usuário ou empresa não encontrados" });
            }

            var tokenizedAsset = await _unitOfWork.TokenizedAssets.GetByIdAsync(id);
            if (tokenizedAsset == null)
            {
                return NotFound(new { mensagem = "Token não encontrado" });
            }

            var contract = await _unitOfWork.Contracts.GetByIdAsync(tokenizedAsset.ContractId);
            if (contract == null || (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId))
            {
                return Forbid("Usuário não tem acesso a este token");
            }

            if (!string.IsNullOrEmpty(request.TokenAddress))
            {
                tokenizedAsset.UpdateTokenAddress(request.TokenAddress);
            }

            if (!string.IsNullOrEmpty(request.TransactionHash))
            {
                tokenizedAsset.UpdateTransactionHash(request.TransactionHash);
            }

            await _unitOfWork.TokenizedAssets.UpdateAsync(tokenizedAsset);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Token {TokenId} atualizado com sucesso", id);

            return Ok(new
            {
                Id = tokenizedAsset.Id,
                ContratoId = tokenizedAsset.ContractId,
                EnderecoToken = tokenizedAsset.TokenAddress,
                ChainId = tokenizedAsset.ChainId,
                HashTransacao = tokenizedAsset.TxHash,
                DataCriacao = tokenizedAsset.CreatedAt,
                UltimaAtualizacao = tokenizedAsset.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token {TokenId}", id);
            return BadRequest(new { mensagem = "Erro ao atualizar token" });
        }
    }
}

public class TokenizeContractRequest
{
    public Guid ContractId { get; set; }
    public string TokenAddress { get; set; } = string.Empty;
    public int ChainId { get; set; }
    public string TransactionHash { get; set; } = string.Empty;
}

public class UpdateTokenizedAssetRequest
{
    public string? TokenAddress { get; set; }
    public string? TransactionHash { get; set; }
}