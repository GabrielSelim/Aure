using AutoMapper;
using Microsoft.Extensions.Logging;
using Aure.Application.DTOs.User;
using Aure.Application.DTOs.Auth;
using Aure.Application.Interfaces;
using Aure.Domain.Common;
using Aure.Domain.Entities;
using Aure.Domain.Interfaces;
using Aure.Domain.Enums;

namespace Aure.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly ICnpjValidationService _cnpjValidationService;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger, ICnpjValidationService cnpjValidationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cnpjValidationService = cnpjValidationService;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User not found with ID {UserId}", id);
            return Result.Failure<UserResponse>("User not found");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        
        if (user == null)
        {
            _logger.LogWarning("User not found with email {Email}", email);
            return Result.Failure<UserResponse>("User not found");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var response = _mapper.Map<IEnumerable<UserResponse>>(users);
        
        _logger.LogInformation("Retrieved {UserCount} users", users.Count());
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Attempt to create user with existing email {Email}", request.Email);
            return Result.Failure<UserResponse>("Email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, request.Role, request.CompanyId);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        
        _logger.LogInformation("User created successfully with ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for update with ID {UserId}", id);
            return Result.Failure<UserResponse>("User not found");
        }

        if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && 
            await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Attempt to update user {UserId} with existing email {Email}", id, request.Email);
            return Result.Failure<UserResponse>("Email already exists");
        }

        user.UpdateProfile(request.Name, request.Email);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        
        _logger.LogInformation("User updated successfully with ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public async Task<Result> ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for password change with ID {UserId}", id);
            return Result.Failure("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password for user {UserId}", id);
            return Result.Failure("Current password is incorrect");
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password changed successfully for user {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("User not found for deletion with ID {UserId}", id);
            return Result.Failure("User not found");
        }

        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User deleted successfully with ID {UserId}", id);
        return Result.Success();
    }

    public async Task<Result<LoginWithUserResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for email {Email}", request.Email);
            return Result.Failure<LoginWithUserResponse>("Email ou senha inválidos");
        }

        var userResponse = _mapper.Map<UserResponse>(user);
        var expiresAt = DateTime.UtcNow.AddHours(1);
        
        var response = new LoginWithUserResponse(
            AccessToken: "jwt-token-placeholder",
            RefreshToken: "refresh-token-placeholder", 
            ExpiresAt: expiresAt,
            User: userResponse,
            UserEntity: user
        );

        _logger.LogInformation("User logged in successfully with ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public Task<Result> LogoutAsync(Guid userId)
    {
        _logger.LogInformation("User logged out with ID {UserId}", userId);
        return Task.FromResult(Result.Success());
    }

    public async Task<Result<UserResponse>> RegisterCompanyAdminAsync(RegisterCompanyAdminRequest request)
    {
        // Verificações em paralelo para melhor performance
        var emailExistsTask = _unitOfWork.Users.EmailExistsAsync(request.Email);
        var cnpjExistsTask = _unitOfWork.Companies.CnpjExistsAsync(request.CompanyCnpj);
        
        await Task.WhenAll(emailExistsTask, cnpjExistsTask);
        
        if (await emailExistsTask)
        {
            _logger.LogWarning("Attempt to register admin with existing email {Email}", request.Email);
            return Result.Failure<UserResponse>("Email já está em uso");
        }

        if (await cnpjExistsTask)
        {
            _logger.LogWarning("Attempt to register company with existing CNPJ {Cnpj}", request.CompanyCnpj);
            return Result.Failure<UserResponse>("CNPJ já está cadastrado");
        }

        // Normalizar CNPJ (remover formatação e reformatar)
        var cleanCnpj = Domain.Common.ValidationHelpers.RemoveCnpjFormatting(request.CompanyCnpj);
        var formattedCnpj = Domain.Common.ValidationHelpers.FormatCnpj(cleanCnpj);

        // Validar CNPJ externamente e verificar nome da empresa (com cache)
        var validationResult = await _cnpjValidationService.ValidateAsync(cleanCnpj);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("CNPJ validation failed for {Cnpj}: {Error}", request.CompanyCnpj, validationResult.ErrorMessage);
            return Result.Failure<UserResponse>(validationResult.ErrorMessage ?? "CNPJ inválido");
        }

        // Verificar se o nome da empresa corresponde aos dados da Receita Federal
        if (!string.IsNullOrEmpty(validationResult.CompanyName))
        {
            var normalizedProvidedName = request.CompanyName.Trim().ToUpperInvariant();
            var normalizedOfficialName = validationResult.CompanyName!.Trim().ToUpperInvariant();
            var normalizedTradeName = validationResult.TradeName?.Trim().ToUpperInvariant();

            bool isNameMatch = normalizedOfficialName.Contains(normalizedProvidedName) || 
                               normalizedProvidedName.Contains(normalizedOfficialName);

            bool isTradeNameMatch = !string.IsNullOrEmpty(normalizedTradeName) && 
                                   (normalizedTradeName!.Contains(normalizedProvidedName) || 
                                    normalizedProvidedName.Contains(normalizedTradeName));

            if (!isNameMatch && !isTradeNameMatch)
            {
                _logger.LogWarning("Company name mismatch for CNPJ {Cnpj}. Provided: {ProvidedName}, Official: {OfficialName}, Trade: {TradeName}", 
                    request.CompanyCnpj, request.CompanyName, validationResult.CompanyName, validationResult.TradeName);
                return Result.Failure<UserResponse>($"O nome da empresa não corresponde aos registros oficiais. Nome oficial: {validationResult.CompanyName}");
            }
        }

        // Criar empresa primeiro
        var company = new Company(request.CompanyName, formattedCnpj, request.CompanyType);
        await _unitOfWork.Companies.AddAsync(company);

        // Criar usuário admin
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, UserRole.Admin, company.Id);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        _logger.LogInformation("Company admin registered successfully with ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> InviteUserAsync(InviteUserRequest request, Guid currentUserId, string currentUserRole)
    {
        // Verificar se email já existe
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Attempt to invite user with existing email {Email}", request.Email);
            return Result.Failure<UserResponse>("Email already exists");
        }

        // Obter company do usuário atual
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        if (currentUser?.CompanyId == null)
        {
            return Result.Failure<UserResponse>("Current user is not associated with a company");
        }

        // Criar usuário com senha temporária
        var tempPassword = Guid.NewGuid().ToString()[..8];
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        var user = new User(request.Name, request.Email, passwordHash, request.Role, currentUser.CompanyId);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        _logger.LogInformation("User invited successfully with ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public Task<Result<UserResponse>> AcceptInviteAsync(string inviteToken, AcceptInviteRequest request)
    {
        // TODO: Implementar lógica de aceitar convite
        // Por enquanto, placeholder
        _logger.LogInformation("Accept invite called with token {Token}", inviteToken);
        return Task.FromResult(Result.Failure<UserResponse>("Ainda não implementado"));
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllByCompanyAsync(Guid companyId)
    {
        var users = await _unitOfWork.Users.GetByCompanyIdAsync(companyId);
        var response = _mapper.Map<IEnumerable<UserResponse>>(users);
        
        _logger.LogInformation("Retrieved {UserCount} users for company {CompanyId}", users.Count(), companyId);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetByIdAndCompanyAsync(Guid id, Guid companyId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null || user.CompanyId != companyId)
        {
            _logger.LogWarning("User not found or not in company. UserId: {UserId}, CompanyId: {CompanyId}", id, companyId);
            return Result.Failure<UserResponse>("User not found");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetByEmailAndCompanyAsync(string email, Guid companyId)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        
        if (user == null || user.CompanyId != companyId)
        {
            _logger.LogWarning("User not found or not in company. Email: {Email}, CompanyId: {CompanyId}", email, companyId);
            return Result.Failure<UserResponse>("User not found");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }
}