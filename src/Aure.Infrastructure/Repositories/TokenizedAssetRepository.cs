using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class TokenizedAssetRepository : BaseRepository<TokenizedAsset>, ITokenizedAssetRepository
{
    public TokenizedAssetRepository(AureDbContext context) : base(context) { }

    public async Task<TokenizedAsset?> GetByContractIdAsync(Guid contractId)
    {
        return await _context.TokenizedAssets
            .Include(ta => ta.Contract)
            .FirstOrDefaultAsync(ta => ta.ContractId == contractId);
    }

    public async Task<IEnumerable<TokenizedAsset>> GetByChainIdAsync(int chainId)
    {
        return await _context.TokenizedAssets
            .Include(ta => ta.Contract)
            .Where(ta => ta.ChainId == chainId)
            .ToListAsync();
    }

    public async Task<TokenizedAsset?> GetByTokenAddressAsync(string tokenAddress)
    {
        return await _context.TokenizedAssets
            .Include(ta => ta.Contract)
            .FirstOrDefaultAsync(ta => ta.TokenAddress == tokenAddress);
    }

    public async Task<TokenizedAsset?> GetByTransactionHashAsync(string txHash)
    {
        return await _context.TokenizedAssets
            .Include(ta => ta.Contract)
            .FirstOrDefaultAsync(ta => ta.TxHash == txHash);
    }

    public async Task<IEnumerable<TokenizedAsset>> GetActiveTokensAsync()
    {
        return await _context.TokenizedAssets
            .Include(ta => ta.Contract)
            .Where(ta => !string.IsNullOrEmpty(ta.TokenAddress) && !string.IsNullOrEmpty(ta.TxHash))
            .ToListAsync();
    }
}
