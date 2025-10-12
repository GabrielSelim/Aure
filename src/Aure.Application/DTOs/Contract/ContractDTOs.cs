using Aure.Domain.Enums;

namespace Aure.Application.DTOs.Contract;

public record CreateContractRequest(
    Guid ClientId,
    Guid ProviderId,
    string Title,
    decimal ValueTotal,
    string? IpfsCid = null
);

public record UpdateContractRequest(
    string Title,
    decimal ValueTotal
);

public record ContractResponse(
    Guid Id,
    Guid ClientId,
    Guid ProviderId,
    string Title,
    decimal ValueTotal,
    string? IpfsCid,
    string Sha256Hash,
    ContractStatus Status,
    bool IsFullySigned,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ContractListResponse(
    IEnumerable<ContractResponse> Contracts,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record SignContractRequest(
    Guid ContractId,
    Guid UserId,
    SignatureMethod Method
);