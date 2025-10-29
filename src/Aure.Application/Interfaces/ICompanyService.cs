using Aure.Application.DTOs.Company;
using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Application.Interfaces;

public interface ICompanyService
{
    Task<Result<CompanyResponse>> GetByIdAsync(Guid id);
    Task<Result<CompanyResponse>> GetByCnpjAsync(string cnpj);
    Task<Result<CompanyListResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<Result<IEnumerable<CompanyResponse>>> GetByTypeAsync(CompanyType type);
    Task<Result<IEnumerable<CompanyResponse>>> GetByKycStatusAsync(KycStatus status);
    Task<Result<CompanyResponse>> CreateAsync(CreateCompanyRequest request);
    Task<Result<CompanyResponse>> UpdateAsync(Guid id, UpdateCompanyRequest request);
    Task<Result> UpdateKycStatusAsync(Guid id, KycStatus status);
    Task<Result> DeleteAsync(Guid id);
    Task<CompanyInfoResponse> GetCompanyParentInfoAsync(Guid userId);
    Task<UpdateCompanyParentResponse> UpdateCompanyParentAsync(Guid userId, UpdateCompanyParentRequest request);
}