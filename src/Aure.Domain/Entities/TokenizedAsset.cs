using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class TokenizedAsset : BaseEntity
{
    public Guid ContractId { get; private set; }
    public string TokenAddress { get; private set; } = string.Empty;
    public int ChainId { get; private set; }
    public string TxHash { get; private set; } = string.Empty;

    public Contract Contract { get; private set; } = null!;

    private TokenizedAsset() { }

    public TokenizedAsset(Guid contractId, string tokenAddress, int chainId, string txHash)
    {
        ContractId = contractId;
        TokenAddress = tokenAddress;
        ChainId = chainId;
        TxHash = txHash;
    }

    public void UpdateTokenAddress(string newTokenAddress)
    {
        TokenAddress = newTokenAddress;
        UpdateTimestamp();
    }

    public void UpdateTransactionHash(string newTxHash)
    {
        TxHash = newTxHash;
        UpdateTimestamp();
    }
}