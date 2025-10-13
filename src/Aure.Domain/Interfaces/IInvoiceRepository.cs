using Aure.Domain.Entities;
using Aure.Domain.Enums;

namespace Aure.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<IEnumerable<Invoice>> GetByContractIdAsync(Guid contractId);
    Task<IEnumerable<Invoice>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<Invoice?> GetByAccessKeyAsync(string accessKey);
    Task AddAsync(Invoice entity);
    Task UpdateAsync(Invoice entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber);
    Task<bool> AccessKeyExistsAsync(string accessKey);
    Task<string> GetNextInvoiceNumberAsync(string series);
}