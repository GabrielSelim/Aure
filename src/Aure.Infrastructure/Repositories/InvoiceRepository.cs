using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<Invoice>> GetByContractIdAsync(Guid contractId)
    {
        return await _context.Invoices
            .Where(i => !i.IsDeleted && i.ContractId == contractId)
            .Include(i => i.Contract)
            .Include(i => i.Items)
            .Include(i => i.TaxCalculations)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Invoices
            .Where(i => !i.IsDeleted && 
                       (i.Contract.ClientId == companyId || i.Contract.ProviderId == companyId))
            .Include(i => i.Contract)
            .Include(i => i.Items)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
    {
        return await _context.Invoices
            .Where(i => !i.IsDeleted && i.Status == status)
            .Include(i => i.Contract)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .Where(i => !i.IsDeleted && i.InvoiceNumber == invoiceNumber)
            .Include(i => i.Contract)
            .Include(i => i.Items)
            .Include(i => i.TaxCalculations)
            .FirstOrDefaultAsync();
    }

    public async Task<Invoice?> GetByAccessKeyAsync(string accessKey)
    {
        return await _context.Invoices
            .Where(i => !i.IsDeleted && i.AccessKey == accessKey)
            .Include(i => i.Contract)
            .Include(i => i.Items)
            .Include(i => i.TaxCalculations)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .AnyAsync(i => !i.IsDeleted && i.InvoiceNumber == invoiceNumber);
    }

    public async Task<bool> AccessKeyExistsAsync(string accessKey)
    {
        return await _context.Invoices
            .AnyAsync(i => !i.IsDeleted && i.AccessKey == accessKey);
    }

    public async Task<string> GetNextInvoiceNumberAsync(string series)
    {
        var lastInvoice = await _context.Invoices
            .Where(i => !i.IsDeleted && i.Series == series)
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastInvoice == null)
        {
            return "1";
        }

        if (int.TryParse(lastInvoice.InvoiceNumber, out int lastNumber))
        {
            return (lastNumber + 1).ToString();
        }

        return "1";
    }
}