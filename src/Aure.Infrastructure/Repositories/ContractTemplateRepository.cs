using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Aure.Infrastructure.Repositories;

public class ContractTemplateRepository : IContractTemplateRepository
{
    private readonly AureDbContext _context;

    public ContractTemplateRepository(AureDbContext context)
    {
        _context = context;
    }

    public async Task<ContractTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.ContractTemplates
            .Include(ct => ct.Company)
            .FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);
    }

    public async Task<List<ContractTemplate>> GetAllByCompanyIdAsync(Guid companyId, bool apenasAtivos = true)
    {
        var query = _context.ContractTemplates
            .Where(ct => ct.CompanyId == companyId && !ct.IsDeleted);

        if (apenasAtivos)
        {
            query = query.Where(ct => ct.Ativo);
        }

        return await query
            .OrderByDescending(ct => ct.EhPadrao)
            .ThenBy(ct => ct.Nome)
            .ToListAsync();
    }

    public async Task<List<ContractTemplate>> GetTemplatesSistemaAsync(bool apenasAtivos = true)
    {
        var query = _context.ContractTemplates
            .Where(ct => ct.EhSistema && !ct.IsDeleted);

        if (apenasAtivos)
        {
            query = query.Where(ct => ct.Ativo);
        }

        return await query
            .OrderBy(ct => ct.Nome)
            .ToListAsync();
    }

    public async Task<ContractTemplate?> GetTemplatePadraoAsync(Guid companyId, ContractTemplateType tipo)
    {
        return await _context.ContractTemplates
            .FirstOrDefaultAsync(ct => 
                ct.CompanyId == companyId && 
                ct.Tipo == tipo && 
                ct.EhPadrao && 
                ct.Ativo && 
                !ct.IsDeleted);
    }

    public async Task<List<ContractTemplate>> GetByTipoAsync(Guid companyId, ContractTemplateType tipo)
    {
        return await _context.ContractTemplates
            .Where(ct => 
                ct.CompanyId == companyId && 
                ct.Tipo == tipo && 
                ct.Ativo && 
                !ct.IsDeleted)
            .OrderByDescending(ct => ct.EhPadrao)
            .ThenBy(ct => ct.Nome)
            .ToListAsync();
    }

    public async Task AddAsync(ContractTemplate template)
    {
        await _context.ContractTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ContractTemplate template)
    {
        _context.ContractTemplates.Update(template);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteTemplatePadraoAsync(Guid companyId, ContractTemplateType tipo, Guid? excluirTemplateId = null)
    {
        var query = _context.ContractTemplates
            .Where(ct => 
                ct.CompanyId == companyId && 
                ct.Tipo == tipo && 
                ct.EhPadrao && 
                ct.Ativo && 
                !ct.IsDeleted);

        if (excluirTemplateId.HasValue)
        {
            query = query.Where(ct => ct.Id != excluirTemplateId.Value);
        }

        return await query.AnyAsync();
    }
}
