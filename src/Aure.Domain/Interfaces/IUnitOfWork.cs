using Aure.Domain.Entities;

namespace Aure.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICompanyRepository Companies { get; }
    ICompanyRelationshipRepository CompanyRelationships { get; }
    IUserInviteRepository UserInvites { get; }
    IUserInvitationRepository UserInvitations { get; }
    IContractRepository Contracts { get; }
    IPaymentRepository Payments { get; }
    ISignatureRepository Signatures { get; }
    ISplitRuleRepository SplitRules { get; }
    ISplitExecutionRepository SplitExecutions { get; }
    ILedgerEntryRepository LedgerEntries { get; }
    ITokenizedAssetRepository TokenizedAssets { get; }
    IInvoiceRepository Invoices { get; }
    ITaxCalculationRepository TaxCalculations { get; }
    IAuditLogRepository AuditLogs { get; }
    IKycRecordRepository KycRecords { get; }
    INotificationRepository Notifications { get; }
    IContractTemplateConfigRepository ContractTemplateConfigs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> CommitAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
}