using AutoMapper;
using Microsoft.Extensions.Logging;
using Aure.Application.DTOs.Company;
using Aure.Application.Interfaces;
using Aure.Domain.Common;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;

namespace Aure.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CompanyService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CompanyResponse>> GetByIdAsync(Guid id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        
        if (company == null)
        {
            _logger.LogWarning("Company not found with ID {CompanyId}", id);
            return Result.Failure<CompanyResponse>("Company not found");
        }

        var response = _mapper.Map<CompanyResponse>(company);
        return Result.Success(response);
    }

    public async Task<Result<CompanyResponse>> GetByCnpjAsync(string cnpj)
    {
        var company = await _unitOfWork.Companies.GetByCnpjAsync(cnpj);
        
        if (company == null)
        {
            _logger.LogWarning("Company not found with CNPJ {Cnpj}", cnpj);
            return Result.Failure<CompanyResponse>("Company not found");
        }

        var response = _mapper.Map<CompanyResponse>(company);
        return Result.Success(response);
    }

    public async Task<Result<CompanyListResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var companies = await _unitOfWork.Companies.GetAllAsync();
        var totalCount = companies.Count();
        
        var pagedCompanies = companies
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var responses = _mapper.Map<IEnumerable<CompanyResponse>>(pagedCompanies);
        
        var result = new CompanyListResponse(
            Companies: responses,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize
        );

        _logger.LogInformation("Retrieved {CompanyCount} companies", totalCount);
        return Result.Success(result);
    }

    public async Task<Result<IEnumerable<CompanyResponse>>> GetByTypeAsync(CompanyType type)
    {
        var companies = await _unitOfWork.Companies.GetByTypeAsync(type);
        var responses = _mapper.Map<IEnumerable<CompanyResponse>>(companies);
        
        _logger.LogInformation("Retrieved {CompanyCount} companies of type {Type}", companies.Count(), type);
        return Result.Success(responses);
    }

    public async Task<Result<IEnumerable<CompanyResponse>>> GetByKycStatusAsync(KycStatus status)
    {
        var companies = await _unitOfWork.Companies.GetByKycStatusAsync(status);
        var responses = _mapper.Map<IEnumerable<CompanyResponse>>(companies);
        
        _logger.LogInformation("Retrieved {CompanyCount} companies with KYC status {Status}", companies.Count(), status);
        return Result.Success(responses);
    }

    public async Task<Result<CompanyResponse>> CreateAsync(CreateCompanyRequest request)
    {
        if (await _unitOfWork.Companies.CnpjExistsAsync(request.Cnpj))
        {
            _logger.LogWarning("Attempt to create company with existing CNPJ {Cnpj}", request.Cnpj);
            return Result.Failure<CompanyResponse>("CNPJ already exists");
        }

        var company = new Company(request.Name, request.Cnpj, request.Type, request.BusinessModel);
        
        await _unitOfWork.Companies.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CompanyResponse>(company);
        
        _logger.LogInformation("Company created successfully with ID {CompanyId}", company.Id);
        return Result.Success(response);
    }

    public async Task<Result<CompanyResponse>> UpdateAsync(Guid id, UpdateCompanyRequest request)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        
        if (company == null)
        {
            _logger.LogWarning("Company not found for update with ID {CompanyId}", id);
            return Result.Failure<CompanyResponse>("Company not found");
        }

        // Atualizar apenas nome, tipo e modelo de negócio - CNPJ não pode ser alterado
        company.UpdateCompanyInfo(request.Name, request.Type, request.BusinessModel);
        
        await _unitOfWork.Companies.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CompanyResponse>(company);
        
        _logger.LogInformation("Company updated successfully with ID {CompanyId}", company.Id);
        return Result.Success(response);
    }

    public async Task<Result> UpdateKycStatusAsync(Guid id, KycStatus status)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        
        if (company == null)
        {
            _logger.LogWarning("Company not found for KYC update with ID {CompanyId}", id);
            return Result.Failure("Company not found");
        }

        company.UpdateKycStatus(status);
        await _unitOfWork.Companies.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Company KYC status updated to {Status} for ID {CompanyId}", status, company.Id);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        
        if (company == null)
        {
            _logger.LogWarning("Company not found for deletion with ID {CompanyId}", id);
            return Result.Failure("Company not found");
        }

        company.MarkAsDeleted();
        await _unitOfWork.Companies.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Company soft deleted with ID {CompanyId}", company.Id);
        return Result.Success();
    }
}