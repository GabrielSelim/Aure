using Aure.Domain.Enums;

namespace Aure.Application.DTOs.Company;

public record CreateCompanyRequest(
    string Name,
    string Cnpj,
    CompanyType Type,
    BusinessModel BusinessModel = BusinessModel.Standard
);

public record UpdateCompanyRequest(
    string Name,
    CompanyType Type,
    BusinessModel BusinessModel
);

public record CompanyResponse(
    Guid Id,
    string Name,
    string Cnpj,
    CompanyType Type,
    BusinessModel BusinessModel,
    KycStatus KycStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CompanyListResponse(
    IEnumerable<CompanyResponse> Companies,
    int TotalCount,
    int PageNumber,
    int PageSize
);