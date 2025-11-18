using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aure.Infrastructure.Repositories;

public class ContractDocumentRepository : IContractDocumentRepository
{
    private readonly AureDbContext _context;

    public ContractDocumentRepository(AureDbContext context)
    {
        _context = context;
    }

    public async Task<ContractDocument?> GetByIdAsync(Guid id)
    {
        return await _context.ContractDocuments
            .Include(cd => cd.Contract)
            .Include(cd => cd.Template)
            .Include(cd => cd.GeradoPorUsuario)
            .FirstOrDefaultAsync(cd => cd.Id == id && !cd.IsDeleted);
    }

    public async Task<List<ContractDocument>> GetByContractIdAsync(Guid contractId)
    {
        return await _context.ContractDocuments
            .Include(cd => cd.Template)
            .Include(cd => cd.GeradoPorUsuario)
            .Where(cd => cd.ContractId == contractId && !cd.IsDeleted)
            .OrderByDescending(cd => cd.VersaoMajor)
            .ThenByDescending(cd => cd.VersaoMinor)
            .ToListAsync();
    }

    public async Task<ContractDocument?> GetVersaoFinalAsync(Guid contractId)
    {
        return await _context.ContractDocuments
            .Include(cd => cd.Template)
            .Include(cd => cd.GeradoPorUsuario)
            .Where(cd => cd.ContractId == contractId && cd.EhVersaoFinal && !cd.IsDeleted)
            .OrderByDescending(cd => cd.VersaoMajor)
            .ThenByDescending(cd => cd.VersaoMinor)
            .FirstOrDefaultAsync();
    }

    public async Task<ContractDocument?> GetUltimaVersaoAsync(Guid contractId)
    {
        return await _context.ContractDocuments
            .Include(cd => cd.Template)
            .Include(cd => cd.GeradoPorUsuario)
            .Where(cd => cd.ContractId == contractId && !cd.IsDeleted)
            .OrderByDescending(cd => cd.VersaoMajor)
            .ThenByDescending(cd => cd.VersaoMinor)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ContractDocument document)
    {
        await _context.ContractDocuments.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContractDocument document)
    {
        _context.ContractDocuments.Update(document);
        await _context.SaveChangesAsync();
    }
}
