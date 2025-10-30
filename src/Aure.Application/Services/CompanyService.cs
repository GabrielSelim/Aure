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
    private readonly IUserRepository _userRepository;
    private readonly IContractRepository _contractRepository;
    private readonly ICnpjValidationService _cnpjValidationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        IUnitOfWork unitOfWork, 
        IUserRepository userRepository,
        IContractRepository contractRepository,
        ICnpjValidationService cnpjValidationService,
        IMapper mapper, 
        ILogger<CompanyService> logger)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _contractRepository = contractRepository;
        _cnpjValidationService = cnpjValidationService;
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

    public async Task<CompanyInfoResponse> GetCompanyParentInfoAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        if (!user.CompanyId.HasValue)
            throw new ArgumentException("Usuário não possui empresa vinculada");

        var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);

        if (company == null)
            throw new ArgumentException("Empresa não encontrada");

        var allUsers = await _userRepository.GetAllAsync();
        var companyUsers = allUsers.Where(u => u.CompanyId == company.Id).ToList();

        var allContracts = await _contractRepository.GetAllAsync();
        var activeContracts = allContracts.Count(c => 
            c.ClientId == company.Id && 
            c.Status == ContractStatus.Active);

        var enderecoCompleto = string.IsNullOrEmpty(user.EnderecoRua) 
            ? null 
            : $"{user.EnderecoRua}, {user.EnderecoNumero} - {user.EnderecoBairro}, {user.EnderecoCidade}/{user.EnderecoEstado}";

        return new CompanyInfoResponse
        {
            Id = company.Id,
            RazaoSocial = company.Name,
            Cnpj = company.Cnpj,
            CompanyType = company.Type,
            BusinessModel = company.BusinessModel,
            EnderecoCompleto = enderecoCompleto,
            TotalFuncionarios = companyUsers.Count,
            ContratosAtivos = activeContracts,
            DataCadastro = company.CreatedAt
        };
    }

    public async Task<UpdateCompanyParentResponse> UpdateCompanyParentAsync(Guid userId, UpdateCompanyParentRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        if (user.Role != UserRole.DonoEmpresaPai)
            throw new InvalidOperationException("Apenas DonoEmpresaPai pode atualizar empresa pai");

        if (!user.CompanyId.HasValue)
            throw new ArgumentException("Usuário não possui empresa vinculada");

        var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);

        if (company == null)
            throw new ArgumentException("Empresa não encontrada");

        if (!string.IsNullOrEmpty(request.Cnpj))
        {
            var cnpjLimpo = new string(request.Cnpj.Where(char.IsDigit).ToArray());

            if (cnpjLimpo.Length != 14)
                throw new ArgumentException("CNPJ inválido. Deve conter 14 dígitos.");

            if (cnpjLimpo != company.Cnpj)
            {
                var cnpjExiste = await _unitOfWork.Companies.CnpjExistsAsync(cnpjLimpo, company.Id);
                if (cnpjExiste)
                    throw new ArgumentException("CNPJ já cadastrado para outra empresa");

                var cnpjValidationResult = await _cnpjValidationService.ValidateAsync(cnpjLimpo);

                if (!cnpjValidationResult.IsValid)
                    throw new ArgumentException($"CNPJ inválido na Receita Federal: {cnpjValidationResult.ErrorMessage}");

                var razaoSocialReceita = cnpjValidationResult.RazaoSocial ?? "";
                var razaoSocialAtual = request.RazaoSocial ?? company.Name;

                var similarity = CalculateSimilarity(razaoSocialReceita, razaoSocialAtual);

                if (similarity < 0.85 && !request.ConfirmarDivergenciaRazaoSocial)
                {
                    _logger.LogWarning(
                        "Divergência de Razão Social detectada. Receita: {RazaoSocialReceita}, Informada: {RazaoSocialInformada}, Similaridade: {Similarity}%",
                        razaoSocialReceita, razaoSocialAtual, similarity * 100);

                    return new UpdateCompanyParentResponse
                    {
                        Sucesso = false,
                        DivergenciaRazaoSocial = true,
                        RazaoSocialReceita = razaoSocialReceita,
                        RazaoSocialInformada = razaoSocialAtual,
                        RequerConfirmacao = true,
                        Mensagem = $"A Razão Social informada ({razaoSocialAtual}) difere da registrada na Receita Federal ({razaoSocialReceita}). Confirme para prosseguir."
                    };
                }

                company.UpdateCnpj(cnpjLimpo);
                _logger.LogInformation("CNPJ da empresa pai {CompanyId} alterado para {Cnpj}", company.Id, cnpjLimpo);
            }
        }

        if (!string.IsNullOrEmpty(request.RazaoSocial))
        {
            company.UpdateCompanyInfo(request.RazaoSocial, company.Type, company.BusinessModel);
            _logger.LogInformation("Razão Social da empresa pai {CompanyId} alterada para {RazaoSocial}", company.Id, request.RazaoSocial);
        }

        if (!string.IsNullOrEmpty(request.EnderecoRua))
        {
            user.UpdateAddress(
                request.EnderecoRua,
                request.EnderecoNumero,
                request.EnderecoComplemento,
                request.EnderecoBairro,
                request.EnderecoCidade,
                request.EnderecoEstado,
                request.EnderecoPais,
                request.EnderecoCep);

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Endereço da empresa pai {CompanyId} atualizado (sync bidirecional)", company.Id);
        }

        await _unitOfWork.Companies.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Empresa pai {CompanyId} atualizada com sucesso pelo usuário {UserId}", company.Id, userId);

        var empresaInfo = await GetCompanyParentInfoAsync(userId);

        return new UpdateCompanyParentResponse
        {
            Sucesso = true,
            Mensagem = "Empresa atualizada com sucesso",
            Empresa = empresaInfo
        };
    }

    public async Task<Result<UserCompanyInfoResponse>> GetCompanyInfoByUserIdAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID {UserId}", userId);
                return Result.Failure<UserCompanyInfoResponse>("Usuário não encontrado");
            }

            if (!user.CompanyId.HasValue)
            {
                _logger.LogWarning("User {UserId} does not have a company associated", userId);
                return Result.Failure<UserCompanyInfoResponse>("Usuário não possui empresa associada");
            }

            var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
            if (company == null)
            {
                _logger.LogWarning("Company not found with ID {CompanyId} for user {UserId}", user.CompanyId, userId);
                return Result.Failure<UserCompanyInfoResponse>("Empresa não encontrada");
            }

            var response = new UserCompanyInfoResponse
            {
                Id = company.Id,
                Nome = company.Name,
                Cnpj = company.Cnpj,
                CnpjFormatado = company.GetFormattedCnpj(),
                Tipo = company.Type.ToString(),
                ModeloNegocio = company.BusinessModel.ToString(),
                TelefoneCelular = company.PhoneMobile,
                TelefoneFixo = company.PhoneLandline,
                Endereco = string.IsNullOrWhiteSpace(company.AddressStreet) ? null : new EnderecoEmpresaDto
                {
                    Rua = company.AddressStreet,
                    Numero = company.AddressNumber!,
                    Complemento = company.AddressComplement,
                    Bairro = company.AddressNeighborhood!,
                    Cidade = company.AddressCity!,
                    Estado = company.AddressState!,
                    Pais = company.AddressCountry!,
                    Cep = company.AddressZipCode!,
                    EnderecoCompleto = company.GetFullAddress()
                }
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company info for user {UserId}", userId);
            return Result.Failure<UserCompanyInfoResponse>("Erro ao buscar dados da empresa");
        }
    }

    public async Task<Result<UserCompanyInfoResponse>> UpdateCompanyInfoAsync(Guid userId, UpdateUserCompanyInfoRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role != UserRole.DonoEmpresaPai)
            {
                _logger.LogWarning("Unauthorized attempt to update company info by user {UserId}", userId);
                return Result.Failure<UserCompanyInfoResponse>("Apenas o dono da empresa pode alterar os dados da empresa");
            }

            if (!user.CompanyId.HasValue)
            {
                _logger.LogWarning("User {UserId} does not have a company associated", userId);
                return Result.Failure<UserCompanyInfoResponse>("Usuário não possui empresa associada");
            }

            var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
            if (company == null)
            {
                _logger.LogWarning("Company not found with ID {CompanyId} for user {UserId}", user.CompanyId, userId);
                return Result.Failure<UserCompanyInfoResponse>("Empresa não encontrada");
            }

            company.UpdateName(request.Nome);
            company.UpdateContactInfo(request.TelefoneCelular, request.TelefoneFixo);
            company.UpdateAddress(
                request.Rua,
                request.Numero,
                request.Complemento,
                request.Bairro,
                request.Cidade,
                request.Estado,
                request.Pais,
                request.Cep
            );

            await _unitOfWork.Companies.UpdateAsync(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company {CompanyId} info updated by user {UserId}", company.Id, userId);

            return await GetCompanyInfoByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company info for user {UserId}", userId);
            return Result.Failure<UserCompanyInfoResponse>("Erro ao atualizar dados da empresa");
        }
    }

    private static double CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0;

        source = source.ToUpperInvariant().Trim();
        target = target.ToUpperInvariant().Trim();

        if (source == target)
            return 1.0;

        var distance = LevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);

        return 1.0 - (double)distance / maxLength;
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var matrix = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; i++)
            matrix[i, 0] = i;

        for (var j = 0; j <= targetLength; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[sourceLength, targetLength];
    }
}