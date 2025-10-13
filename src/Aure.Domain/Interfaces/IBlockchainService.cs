using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface IBlockchainService
{
    Task<string> DeployContractTokenAsync(Contract contract);
    Task<string> TransferTokenAsync(string tokenAddress, string fromAddress, string toAddress, decimal amount);
    Task<TokenTransactionResult> GetTransactionStatusAsync(string transactionHash);
    Task<decimal> GetTokenBalanceAsync(string tokenAddress, string walletAddress);
    Task<IEnumerable<TokenTransaction>> GetTokenTransactionsAsync(string tokenAddress);
    Task<bool> ValidateContractAsync(string tokenAddress);
}

public class TokenTransactionResult
{
    public string TransactionHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal GasUsed { get; set; }
    public decimal GasPrice { get; set; }
    public string BlockNumber { get; set; } = string.Empty;
}

public class TokenTransaction
{
    public string TransactionHash { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}