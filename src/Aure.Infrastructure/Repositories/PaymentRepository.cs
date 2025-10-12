using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetByContractIdAsync(Guid contractId)
    {
        return await _dbSet
            .Where(x => x.ContractId == contractId)
            .Include(x => x.Contract)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
    {
        return await _dbSet
            .Where(x => x.Status == status)
            .Include(x => x.Contract)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method)
    {
        return await _dbSet
            .Where(x => x.Method == method)
            .Include(x => x.Contract)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .Include(x => x.Contract)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAmountByContractAsync(Guid contractId)
    {
        return await _dbSet
            .Where(x => x.ContractId == contractId && x.Status == PaymentStatus.Completed)
            .SumAsync(x => x.Amount);
    }

    public override async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(x => x.Contract)
            .Include(x => x.SplitExecutions)
            .Include(x => x.LedgerEntries)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _dbSet
            .Include(x => x.Contract)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}