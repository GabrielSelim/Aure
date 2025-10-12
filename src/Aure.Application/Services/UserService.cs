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
            _logger.LogWarning("Usuário não encontrado com ID {UserId}", id);
            return Result.Failure<UserResponse>("Usuário não encontrado");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado com email {Email}", email);
            return Result.Failure<UserResponse>("Usuário não encontrado");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var response = _mapper.Map<IEnumerable<UserResponse>>(users);

        _logger.LogInformation("Recuperados {UserCount} usuários", users.Count());
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Tentativa de criar usuário com email existente {Email}", request.Email);
            return Result.Failure<UserResponse>("Email já existe");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, request.Role, request.CompanyId);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);

        _logger.LogInformation("Usuário criado com sucesso com ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para atualização com ID {UserId}", id);
            return Result.Failure<UserResponse>("Usuário não encontrado");
        }

        if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && 
            await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Tentativa de atualizar usuário {UserId} com email existente {Email}", id, request.Email);
            return Result.Failure<UserResponse>("Email já existe");
        }

        user.UpdateProfile(request.Name, request.Email);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);

        _logger.LogInformation("Usuário atualizado com sucesso com ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public async Task<Result> ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para alteração de senha com ID {UserId}", id);
            return Result.Failure("Usuário não encontrado");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Senha atual inválida para o usuário {UserId}", id);
            return Result.Failure("A senha atual está incorreta");
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Senha alterada com sucesso para o usuário {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("Usuário não encontrado para exclusão com ID {UserId}", id);
            return Result.Failure("Usuário não encontrado");
        }

        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Usuário deletado com sucesso com ID {UserId}", id);
        return Result.Success();
    }

    public async Task<Result<LoginWithUserResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Tentativa de login inválida para email {Email}", request.Email);
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

        _logger.LogInformation("Usuário logado com sucesso com ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public Task<Result> LogoutAsync(Guid userId)
    {
        _logger.LogInformation("Usuário deslogado com ID {UserId}", userId);
        return Task.FromResult(Result.Success());
    }

    public async Task<Result<UserResponse>> RegisterCompanyAdminAsync(RegisterCompanyAdminRequest request)
    {
        // Verificações sequenciais para evitar problemas de concorrência no EF
        var emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email).ConfigureAwait(false);
        if (emailExists)
        {
            _logger.LogWarning("Tentativa de registrar admin com email existente {Email}", request.Email);
            return Result.Failure<UserResponse>("Email já está em uso");
        }

        var cnpjExists = await _unitOfWork.Companies.CnpjExistsAsync(request.CompanyCnpj).ConfigureAwait(false);
        if (cnpjExists)
        {
            _logger.LogWarning("Tentativa de registrar empresa com CNPJ existente {Cnpj}", request.CompanyCnpj);
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
                _logger.LogWarning("Incompatibilidade de nome da empresa para CNPJ {Cnpj}. Fornecido: {ProvidedName}, Oficial: {OfficialName}, Fantasia: {TradeName}", 
                    request.CompanyCnpj, request.CompanyName, validationResult.CompanyName, validationResult.TradeName);
                return Result.Failure<UserResponse>($"O nome da empresa não corresponde aos registros oficiais. Nome oficial: {validationResult.CompanyName}");
            }
        }

        // Criar empresa primeiro
        var company = new Company(request.CompanyName, formattedCnpj, request.CompanyType, request.BusinessModel);
        await _unitOfWork.Companies.AddAsync(company);

        // Criar usuário admin
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, UserRole.Admin, company.Id);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        _logger.LogInformation("Usuário admin da empresa registrado com sucesso com ID {UserId}", user.Id);
        return Result.Success(response);
    }

    public async Task<Result<InviteResponse>> InviteUserAsync(InviteUserRequest request, Guid currentUserId, string currentUserRole)
    {
        // Verificar se email já existe como usuário
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            _logger.LogWarning("Tentativa de convidar usuário com email existente {Email}", request.Email);
            return Result.Failure<InviteResponse>("Email já existe como usuário registrado");
        }

        // Verificar se email já tem convite pendente
        if (await _unitOfWork.UserInvites.EmailHasPendingInviteAsync(request.Email))
        {
            _logger.LogWarning("Email {Email} já tem um convite pendente", request.Email);
            return Result.Failure<InviteResponse>("Email já tem um convite pendente");
        }

        // Obter dados do usuário atual
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        if (currentUser?.CompanyId == null)
        {
            return Result.Failure<InviteResponse>("Usuário atual não está associado a uma empresa");
        }

        // Validações específicas por tipo de convite
        if (request.InviteType == InviteType.ContractedPJ)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName) || 
                string.IsNullOrWhiteSpace(request.Cnpj) ||
                request.CompanyType == null ||
                request.BusinessModel == null)
            {
                return Result.Failure<InviteResponse>("Dados da empresa são obrigatórios para convites PJ");
            }

            // Validar CNPJ
            var cnpjValidation = await _cnpjValidationService.ValidateAsync(request.Cnpj);
            if (!cnpjValidation.IsValid)
            {
                return Result.Failure<InviteResponse>(cnpjValidation.ErrorMessage ?? "CNPJ inválido");
            }

            // Verificar se CNPJ já existe (limpar CNPJ primeiro)
            var cleanCnpj = System.Text.RegularExpressions.Regex.Replace(request.Cnpj, @"\D", "");
            if (await _unitOfWork.Companies.CnpjExistsAsync(cleanCnpj))
            {
                return Result.Failure<InviteResponse>("CNPJ já cadastrado");
            }
        }

        // Definir role baseado no tipo de convite
        var userRole = request.InviteType == InviteType.ContractedPJ 
            ? UserRole.Provider 
            : request.Role ?? UserRole.Provider;

        // Criar convite
        var invite = new UserInvite(
            inviterName: currentUser.Name,
            inviteeEmail: request.Email,
            inviteeName: request.Name,
            role: userRole,
            companyId: currentUser.CompanyId.Value,
            invitedByUserId: currentUserId,
            inviteType: request.InviteType,
            businessModel: request.BusinessModel,
            companyName: request.CompanyName,
            cnpj: request.Cnpj,
            companyType: request.CompanyType
        );

        await _unitOfWork.UserInvites.AddAsync(invite);
        await _unitOfWork.SaveChangesAsync();

        var response = new InviteResponse(
            InviteId: invite.Id,
            Message: $"Convite enviado para o email: {request.Email}",
            InviteToken: invite.Token,
            ExpiresAt: invite.ExpiresAt,
            InviteType: invite.InviteType
        );

        _logger.LogInformation("Convite criado com sucesso com ID {InviteId} para o email {Email}", 
            invite.Id, request.Email);
        
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> AcceptInviteAsync(string inviteToken, AcceptInviteRequest request)
    {
        // Buscar convite pelo token
        var invite = await _unitOfWork.UserInvites.GetByTokenAsync(inviteToken);
        if (invite == null)
        {
            _logger.LogWarning("Token de convite inválido: {Token}", inviteToken);
            return Result.Failure<UserResponse>("Token de convite inválido");
        }

        // Verificar se convite já foi aceito
        if (invite.IsAccepted)
        {
            _logger.LogWarning("Convite já aceito: {InviteId}", invite.Id);
            return Result.Failure<UserResponse>("Convite já aceito");
        }

        // Verificar se convite expirou
        if (DateTime.UtcNow > invite.ExpiresAt)
        {
            _logger.LogWarning("Invite expired: {InviteId}", invite.Id);
            return Result.Failure<UserResponse>("Invite expired");
        }

        // Verificar se email já existe como usuário
        if (await _unitOfWork.Users.EmailExistsAsync(invite.InviteeEmail))
        {
            _logger.LogWarning("Email já existe como usuário: {Email}", invite.InviteeEmail);
            return Result.Failure<UserResponse>("Email já registrado como usuário");
        }

        try
        {
            var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                Company? pjCompany = null;

                // Se for convite para PJ contratado, criar empresa PJ primeiro
                if (invite.InviteType == InviteType.ContractedPJ)
                {
                    if (string.IsNullOrEmpty(invite.CompanyName) || 
                        string.IsNullOrEmpty(invite.Cnpj) ||
                        invite.BusinessModel == null ||
                        invite.CompanyType == null)
                    {
                        throw new InvalidOperationException("Informações de convite PJ inválidas");
                    }

                    // Criar empresa PJ
                    pjCompany = new Company(
                        name: invite.CompanyName,
                        cnpj: invite.Cnpj,
                        type: invite.CompanyType.Value,
                        businessModel: invite.BusinessModel.Value
                    );

                    await _unitOfWork.Companies.AddAsync(pjCompany);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Criar usuário
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                
                // Para PJ contratado: usuário fica vinculado à própria empresa PJ
                // Para funcionário: usuário fica vinculado à empresa contratante
                var companyId = pjCompany?.Id ?? invite.CompanyId;
                
                var user = new User(
                    name: invite.InviteeName,
                    email: invite.InviteeEmail,
                    passwordHash: passwordHash,
                    role: invite.Role,
                    companyId: companyId
                );

                // Se for PJ contratado, criar relacionamento entre empresas
                if (invite.InviteType == InviteType.ContractedPJ && pjCompany != null)
                {
                    var relationship = new CompanyRelationship(
                        clientCompanyId: invite.CompanyId,  // Empresa que contratou
                        providerCompanyId: pjCompany.Id,    // Empresa PJ contratada
                        type: RelationshipType.ContractedPJ,
                        notes: $"PJ contratado via convite - Usuário: {invite.InviteeName}"
                    );

                    await _unitOfWork.CompanyRelationships.AddAsync(relationship);
                }

                await _unitOfWork.Users.AddAsync(user);
                
                // Marcar convite como aceito
                invite.MarkAsAccepted();
                await _unitOfWork.UserInvites.UpdateAsync(invite);
                
                await _unitOfWork.SaveChangesAsync();

                return user;
            });

            var response = _mapper.Map<UserResponse>(result);

            _logger.LogInformation("Convite aceito com sucesso. Usuário {UserId} criado para a empresa {CompanyId}",
                result.Id, result.CompanyId);
            
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar convite {InviteId}", invite.Id);
            return Result.Failure<UserResponse>("Erro ao aceitar convite");
        }
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllByCompanyAsync(Guid companyId)
    {
        var users = await _unitOfWork.Users.GetByCompanyIdAsync(companyId);
        var response = _mapper.Map<IEnumerable<UserResponse>>(users);

        _logger.LogInformation("Recuperados {UserCount} usuários para a empresa {CompanyId}", users.Count(), companyId);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetByIdAndCompanyAsync(Guid id, Guid companyId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        
        if (user == null || user.CompanyId != companyId)
        {
            _logger.LogWarning("Usuário não encontrado ou não pertence à empresa. UserId: {UserId}, CompanyId: {CompanyId}", id, companyId);
            return Result.Failure<UserResponse>("Usuário não encontrado");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> GetByEmailAndCompanyAsync(string email, Guid companyId)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        
        if (user == null || user.CompanyId != companyId)
        {
            _logger.LogWarning("Usuário não encontrado ou não pertence à empresa. Email: {Email}, CompanyId: {CompanyId}", email, companyId);
            return Result.Failure<UserResponse>("Usuário não encontrado");
        }

        var response = _mapper.Map<UserResponse>(user);
        return Result.Success(response);
    }
}