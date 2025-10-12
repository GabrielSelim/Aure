using Aure.Application.DTOs.Contract;
using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Application.Interfaces;

public interface IContractService
{
    Task<Result<ContractResponse>> GetByIdAsync(Guid id);
    Task<Result<ContractListResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<Result<IEnumerable<ContractResponse>>> GetByClientIdAsync(Guid clientId);
    Task<Result<IEnumerable<ContractResponse>>> GetByProviderIdAsync(Guid providerId);
    Task<Result<IEnumerable<ContractResponse>>> GetByStatusAsync(ContractStatus status);
    Task<Result<ContractResponse>> CreateAsync(CreateContractRequest request);
    Task<Result<ContractResponse>> UpdateAsync(Guid id, UpdateContractRequest request);
    Task<Result> SignContractAsync(SignContractRequest request);
    Task<Result> ActivateContractAsync(Guid id);
    Task<Result> CompleteContractAsync(Guid id);
    Task<Result> CancelContractAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}