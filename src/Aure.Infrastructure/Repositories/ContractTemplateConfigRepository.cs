using Microsoft.EntityFrameworkCore;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Infrastructure.Data;

namespace Aure.Infrastructure.Repositories
{
    public class ContractTemplateConfigRepository : IContractTemplateConfigRepository
    {
        private readonly AureDbContext _context;

        public ContractTemplateConfigRepository(AureDbContext context)
        {
            _context = context;
        }

        public async Task<ContractTemplateConfig?> GetByIdAsync(Guid id)
        {
            return await _context.ContractTemplateConfigs
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ContractTemplateConfig?> GetByCompanyIdAndNomeAsync(Guid companyId, string nomeConfig)
        {
            return await _context.ContractTemplateConfigs
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.NomeConfig == nomeConfig);
        }

        public async Task<IEnumerable<ContractTemplateConfig>> GetAllByCompanyIdAsync(Guid companyId)
        {
            return await _context.ContractTemplateConfigs
                .Include(c => c.Company)
                .Where(c => c.CompanyId == companyId)
                .OrderBy(c => c.Categoria)
                .ThenBy(c => c.NomeConfig)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContractTemplateConfig>> GetAllAsync()
        {
            return await _context.ContractTemplateConfigs
                .Include(c => c.Company)
                .ToListAsync();
        }

        public async Task AddAsync(ContractTemplateConfig config)
        {
            await _context.ContractTemplateConfigs.AddAsync(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ContractTemplateConfig config)
        {
            _context.ContractTemplateConfigs.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var config = await GetByIdAsync(id);
            if (config != null)
            {
                _context.ContractTemplateConfigs.Remove(config);
                await _context.SaveChangesAsync();
            }
        }
    }
}
