using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface ITokenizedAssetRepository : IBaseRepository<TokenizedAsset>
{
    Task<TokenizedAsset?> GetByContractIdAsync(Guid contractId);
    Task<IEnumerable<TokenizedAsset>> GetByChainIdAsync(int chainId);
    Task<TokenizedAsset?> GetByTokenAddressAsync(string tokenAddress);
    Task<TokenizedAsset?> GetByTransactionHashAsync(string txHash);
    Task<IEnumerable<TokenizedAsset>> GetActiveTokensAsync();
}