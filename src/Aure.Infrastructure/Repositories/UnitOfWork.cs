using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AureDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AureDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Companies = new CompanyRepository(_context);
        CompanyRelationships = new CompanyRelationshipRepository(_context);
        UserInvites = new UserInviteRepository(_context);
        Contracts = new ContractRepository(_context);
        Payments = new PaymentRepository(_context);
    }

    public IUserRepository Users { get; }
    public ICompanyRepository Companies { get; }
    public ICompanyRelationshipRepository CompanyRelationships { get; }
    public IUserInviteRepository UserInvites { get; }
    public IContractRepository Contracts { get; }
    public IPaymentRepository Payments { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}