using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories;

public class SignatureRepository : BaseRepository<Signature>, ISignatureRepository
{
    public SignatureRepository(AureDbContext context) : base(context) { }

    public async Task<IEnumerable<Signature>> GetByContractIdAsync(Guid contractId)
    {
        return await _context.Signatures
            .Where(s => !s.IsDeleted && s.ContractId == contractId)
            .Include(s => s.User)
            .Include(s => s.Contract)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Signature>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Signatures
            .Where(s => !s.IsDeleted && s.UserId == userId)
            .Include(s => s.User)
            .Include(s => s.Contract)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Signature?> GetByContractAndUserAsync(Guid contractId, Guid userId)
    {
        return await _context.Signatures
            .Where(s => !s.IsDeleted && s.ContractId == contractId && s.UserId == userId)
            .Include(s => s.User)
            .Include(s => s.Contract)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsContractFullySignedAsync(Guid contractId)
    {
        var contract = await _context.Contracts
            .Include(c => c.Signatures)
            .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDeleted);

        if (contract == null) return false;

        // Verificar se tanto cliente quanto provedor assinaram
        var clientSignature = contract.Signatures.Any(s => !s.IsDeleted && s.SignedAt != default);
        var providerSignature = contract.Signatures.Any(s => !s.IsDeleted && s.SignedAt != default);

        return clientSignature && providerSignature;
    }
}