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
    private readonly IEmailService _emailService;
    private readonly IEncryptionService _encryptionService;

    public UserService(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ILogger<UserService> logger, 
        ICnpjValidationService cnpjValidationService, 
        IEmailService emailService,
        IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cnpjValidationService = cnpjValidationService;
        _emailService = emailService;
        _encryptionService = encryptionService;
        _emailService = emailService;
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

        // Criar usuário dono da empresa pai
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, UserRole.DonoEmpresaPai, company.Id);

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

        // Obter dados da empresa
        var currentCompany = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value);
        if (currentCompany == null)
        {
            return Result.Failure<InviteResponse>("Empresa do usuário atual não encontrada");
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
            ? UserRole.FuncionarioPJ 
            : request.Role ?? UserRole.FuncionarioCLT;

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
        
        var userInvitation = new UserInvitation(
            name: request.Name,
            email: request.Email,
            role: userRole,
            cargo: request.Cargo,
            companyId: currentUser.CompanyId.Value,
            invitedByUserId: currentUserId,
            invitationToken: invite.Token,
            expirationDays: 7
        );

        await _unitOfWork.UserInvitations.AddAsync(userInvitation);
        await _unitOfWork.SaveChangesAsync();

        // Enviar email de convite
        var emailSent = await _emailService.SendInviteEmailAsync(
            recipientEmail: request.Email,
            recipientName: request.Name,
            inviteToken: invite.Token,
            inviterName: currentUser.Name,
            companyName: currentCompany.Name
        );

        var message = emailSent 
            ? $"Convite enviado com sucesso para o email: {request.Email}"
            : $"Convite criado, mas houve falha ao enviar email para: {request.Email}";

        if (!emailSent)
        {
            _logger.LogWarning("Falha ao enviar email de convite para {Email}, mas convite foi salvo com ID {InviteId}", 
                request.Email, invite.Id);
        }

        var response = new InviteResponse
        {
            Id = invite.Id,
            InviterName = invite.InviterName,
            InviteeEmail = invite.InviteeEmail,
            InviteeName = invite.InviteeName,
            Role = invite.Role,
            CompanyId = invite.CompanyId,
            InvitedByUserId = invite.InvitedByUserId,
            Token = invite.Token,
            ExpiresAt = invite.ExpiresAt,
            IsAccepted = invite.IsAccepted,
            InviteType = invite.InviteType,
            BusinessModel = invite.BusinessModel,
            CompanyName = invite.CompanyName,
            Cnpj = invite.Cnpj,
            CompanyType = invite.CompanyType
        };

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
                
                invite.MarkAsAccepted();
                await _unitOfWork.UserInvites.UpdateAsync(invite);
                
                var userInvitation = await _unitOfWork.UserInvitations.GetByTokenAsync(inviteToken);
                if (userInvitation != null)
                {
                    userInvitation.MarkAsAccepted(user.Id);
                    await _unitOfWork.UserInvitations.UpdateAsync(userInvitation);
                }
                
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

    public async Task<Result<InviteResponse>> ResendInviteEmailAsync(Guid inviteId, Guid currentUserId)
    {
        var invite = await _unitOfWork.UserInvites.GetByIdAsync(inviteId);
        if (invite == null)
        {
            _logger.LogWarning("Convite não encontrado com ID {InviteId}", inviteId);
            return Result.Failure<InviteResponse>("Convite não encontrado");
        }

        if (invite.IsAccepted)
        {
            _logger.LogWarning("Não é possível reenviar convite já aceito: {InviteId}", inviteId);
            return Result.Failure<InviteResponse>("Convite já aceito");
        }

        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        if (currentUser == null || currentUser.CompanyId != invite.CompanyId)
        {
            _logger.LogWarning("Usuário {UserId} não autorizado a reenviar convite {InviteId}", currentUserId, inviteId);
            return Result.Failure<InviteResponse>("Não autorizado a reenviar este convite");
        }

        try
        {
            invite.RegenerateToken();
            await _unitOfWork.UserInvites.UpdateAsync(invite);
            await _unitOfWork.SaveChangesAsync();

            var company = await _unitOfWork.Companies.GetByIdAsync(invite.CompanyId);
            var inviteUrl = $"https://aure.com/aceitar-convite?token={invite.Token}";
            await _emailService.SendInviteEmailAsync(
                invite.InviteeEmail, 
                invite.InviteeName, 
                invite.Token, 
                invite.InviterName, 
                company?.Name ?? "Aure");

            var response = _mapper.Map<InviteResponse>(invite);
            
            _logger.LogInformation("Convite {InviteId} reenviado com sucesso para {Email}", inviteId, invite.InviteeEmail);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reenviar convite {InviteId}", inviteId);
            return Result.Failure<InviteResponse>("Erro ao reenviar convite");
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

    public async Task<PagedResult<EmployeeListItemResponse>> GetEmployeesAsync(Guid requestingUserId, EmployeeListRequest request)
    {
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId);
        
        if (requestingUser == null)
            throw new ArgumentException("Usuário solicitante não encontrado");

        if (!requestingUser.CompanyId.HasValue)
            throw new ArgumentException("Usuário não possui empresa vinculada");

        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var query = allUsers
            .Where(u => u.CompanyId == requestingUser.CompanyId.Value)
            .AsQueryable();

        if (requestingUser.Role == UserRole.Financeiro || requestingUser.Role == UserRole.Juridico)
        {
            query = query.Where(u => 
                u.Role == UserRole.FuncionarioCLT || 
                u.Role == UserRole.FuncionarioPJ);
        }

        if (request.Role.HasValue)
        {
            query = query.Where(u => u.Role == request.Role.Value);
        }

        if (!string.IsNullOrEmpty(request.Cargo))
        {
            query = query.Where(u => u.Cargo != null && u.Cargo.Contains(request.Cargo, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(request.Busca))
        {
            var busca = request.Busca.ToLower();
            query = query.Where(u => 
                u.Name.ToLower().Contains(busca) || 
                u.Email.ToLower().Contains(busca));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = request.Status.ToLower();
            if (status == "ativo")
            {
                query = query.Where(u => !u.IsDeleted);
            }
            else if (status == "inativo")
            {
                query = query.Where(u => u.IsDeleted);
            }
        }
        else
        {
            query = query.Where(u => !u.IsDeleted);
        }

        var totalCount = query.Count();

        var items = query
            .OrderBy(u => u.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var allCompanies = await _unitOfWork.Companies.GetAllAsync();

        var response = items.Select(u => new EmployeeListItemResponse
        {
            Id = u.Id,
            Nome = u.Name,
            Email = u.Email,
            Role = u.Role.ToString(),
            Cargo = u.Cargo,
            Status = u.IsDeleted ? "Inativo" : "Ativo",
            DataEntrada = u.CreatedAt,
            TelefoneCelular = u.TelefoneCelular,
            EmpresaPJ = u.Role == UserRole.FuncionarioPJ && u.CompanyId.HasValue
                ? allCompanies.FirstOrDefault(c => c.Id == u.CompanyId.Value)?.Name
                : null
        }).ToList();

        return new PagedResult<EmployeeListItemResponse>
        {
            Items = response,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<UserDataExportResponse> ExportUserDataAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var userContracts = allContracts.Where(c => 
            c.ProviderId == user.CompanyId || 
            c.ClientId == user.CompanyId).ToList();

        var allPayments = await _unitOfWork.Payments.GetAllAsync();
        var userPayments = allPayments.Where(p => 
            userContracts.Any(c => c.Id == p.ContractId)).ToList();

        Company? userCompany = null;
        if (user.CompanyId.HasValue)
        {
            userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
        }

        var response = new UserDataExportResponse
        {
            DadosPessoais = new DadosPessoais
            {
                Id = user.Id,
                Nome = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                TelefoneCelular = user.TelefoneCelular,
                TelefoneFixo = user.TelefoneFixo,
                CPF = !string.IsNullOrEmpty(user.CPFEncrypted) 
                    ? _encryptionService.Decrypt(user.CPFEncrypted) 
                    : null,
                RG = !string.IsNullOrEmpty(user.RGEncrypted) 
                    ? _encryptionService.Decrypt(user.RGEncrypted) 
                    : null,
                DataNascimento = user.DataNascimento,
                Cargo = user.Cargo,
                Endereco = new EnderecoInfo
                {
                    Rua = user.EnderecoRua,
                    Numero = user.EnderecoNumero,
                    Complemento = user.EnderecoComplemento,
                    Bairro = user.EnderecoBairro,
                    Cidade = user.EnderecoCidade,
                    Estado = user.EnderecoEstado,
                    Pais = user.EnderecoPais,
                    Cep = user.EnderecoCep
                },
                AvatarUrl = user.AvatarUrl,
                DataCriacao = user.CreatedAt,
                AceitouTermosUso = user.AceitouTermosUso,
                DataAceiteTermosUso = user.DataAceiteTermosUso,
                AceitouPoliticaPrivacidade = user.AceitouPoliticaPrivacidade,
                DataAceitePoliticaPrivacidade = user.DataAceitePoliticaPrivacidade
            },
            DadosEmpresa = userCompany != null ? new DadosEmpresa
            {
                Id = userCompany.Id,
                RazaoSocial = userCompany.Name,
                CNPJ = userCompany.Cnpj,
                Tipo = userCompany.Type.ToString()
            } : null,
            Contratos = userContracts.Select(c => new ContratoInfo
            {
                Id = c.Id,
                Titulo = c.Title,
                ValorTotal = c.ValueTotal,
                ValorMensal = c.MonthlyValue,
                DataInicio = c.StartDate,
                DataExpiracao = c.ExpirationDate,
                DataAssinatura = c.SignedDate,
                Status = c.Status.ToString()
            }).ToList(),
            Pagamentos = userPayments.Select(p => new PagamentoInfo
            {
                Id = p.Id,
                Valor = p.Amount,
                DataPagamento = p.PaymentDate ?? p.CreatedAt,
                Status = p.Status.ToString()
            }).ToList(),
            PreferenciasNotificacao = new NotificationPreferencesDTO(),
            DataExportacao = DateTime.UtcNow
        };

        _logger.LogInformation("Dados exportados para o usuário {UserId} conforme LGPD", userId);
        return response;
    }

    public async Task<AccountDeletionResponse> RequestAccountDeletionAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        if (user.IsDeleted)
        {
            return new AccountDeletionResponse
            {
                Sucesso = false,
                Mensagem = "Conta já foi solicitada para exclusão anteriormente"
            };
        }

        var allContracts = await _unitOfWork.Contracts.GetAllAsync();
        var activeContracts = allContracts.Where(c => 
            (c.ProviderId == user.CompanyId || c.ClientId == user.CompanyId) && 
            c.Status == ContractStatus.Active).ToList();

        if (activeContracts.Any())
        {
            return new AccountDeletionResponse
            {
                Sucesso = false,
                Mensagem = "Não é possível excluir conta com contratos ativos. Encerre ou transfira os contratos primeiro."
            };
        }

        var anonymizedName = $"Usuário Removido {user.Id.ToString().Substring(0, 8)}";
        var anonymizedEmail = $"removed_{user.Id.ToString().Substring(0, 8)}@aure.deleted";

        user.AnonymizeForDeletion(anonymizedName, anonymizedEmail);
        
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Conta do usuário {UserId} anonimizada conforme LGPD. Nome: {Name}, Email: {Email}", 
            userId, anonymizedName, anonymizedEmail);

        return new AccountDeletionResponse
        {
            Sucesso = true,
            Mensagem = "Sua conta foi anonimizada com sucesso. Seus dados pessoais foram removidos do sistema.",
            Aviso = "Documentos fiscais (contratos, notas fiscais, pagamentos) foram mantidos por 5 anos conforme legislação brasileira (Lei 8.934/94 e Código Civil). CPF e RG criptografados foram mantidos apenas para fins de auditoria fiscal."
        };
    }

    public async Task<Result<IEnumerable<UserInvitationListResponse>>> GetInvitationsAsync(Guid requestingUserId)
    {
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId);

        if (requestingUser == null)
            return Result.Failure<IEnumerable<UserInvitationListResponse>>("Usuário não encontrado");

        if (requestingUser.Role != UserRole.DonoEmpresaPai && 
            requestingUser.Role != UserRole.Financeiro && 
            requestingUser.Role != UserRole.Juridico)
            return Result.Failure<IEnumerable<UserInvitationListResponse>>("Sem permissão para visualizar convites");

        if (!requestingUser.CompanyId.HasValue)
            return Result.Failure<IEnumerable<UserInvitationListResponse>>("Usuário não vinculado a empresa");

        var invitations = await _unitOfWork.UserInvitations.GetByCompanyIdAsync(requestingUser.CompanyId.Value);

        var response = invitations.Select(inv => new UserInvitationListResponse
        {
            Id = inv.Id,
            Name = inv.Name,
            Email = inv.Email,
            Role = inv.Role,
            Cargo = inv.Cargo,
            Status = inv.Status,
            CreatedAt = inv.CreatedAt,
            ExpiresAt = inv.ExpiresAt,
            AcceptedAt = inv.AcceptedAt,
            InvitedByName = inv.InvitedByUser?.Name ?? "Sistema",
            AcceptedByName = inv.AcceptedByUser?.Name,
            IsExpired = inv.IsExpired(),
            CanBeEdited = inv.CanBeEdited()
        });

        return Result.Success(response);
    }

    public async Task<Result<UserInvitationListResponse>> GetInvitationByIdAsync(Guid invitationId, Guid requestingUserId)
    {
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId);

        if (requestingUser == null)
            return Result.Failure<UserInvitationListResponse>("Usuário não encontrado");

        if (!requestingUser.CompanyId.HasValue)
            return Result.Failure<UserInvitationListResponse>("Usuário não vinculado a empresa");

        var invitation = await _unitOfWork.UserInvitations.GetByIdAsync(invitationId);

        if (invitation == null)
            return Result.Failure<UserInvitationListResponse>("Convite não encontrado");

        if (invitation.CompanyId != requestingUser.CompanyId.Value)
            return Result.Failure<UserInvitationListResponse>("Convite não pertence à sua empresa");

        var response = new UserInvitationListResponse
        {
            Id = invitation.Id,
            Name = invitation.Name,
            Email = invitation.Email,
            Role = invitation.Role,
            Cargo = invitation.Cargo,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            InvitedByName = invitation.InvitedByUser?.Name ?? "Sistema",
            AcceptedByName = invitation.AcceptedByUser?.Name,
            IsExpired = invitation.IsExpired(),
            CanBeEdited = invitation.CanBeEdited()
        };

        return Result.Success(response);
    }

    public async Task<Result<UpdateInvitationResponse>> UpdateInvitationAsync(Guid invitationId, UpdateInvitationRequest request, Guid requestingUserId)
    {
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId);

        if (requestingUser == null)
            return Result.Failure<UpdateInvitationResponse>("Usuário não encontrado");

        if (requestingUser.Role != UserRole.DonoEmpresaPai && 
            requestingUser.Role != UserRole.Financeiro)
            return Result.Failure<UpdateInvitationResponse>("Apenas DonoEmpresaPai e Financeiro podem editar convites");

        if (!requestingUser.CompanyId.HasValue)
            return Result.Failure<UpdateInvitationResponse>("Usuário não vinculado a empresa");

        var invitation = await _unitOfWork.UserInvitations.GetByIdAsync(invitationId);

        if (invitation == null)
            return Result.Failure<UpdateInvitationResponse>("Convite não encontrado");

        if (invitation.CompanyId != requestingUser.CompanyId.Value)
            return Result.Failure<UpdateInvitationResponse>("Convite não pertence à sua empresa");

        if (!invitation.CanBeEdited())
            return Result.Failure<UpdateInvitationResponse>($"Convite não pode ser editado. Status: {invitation.Status}");

        var emailExistente = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (emailExistente != null)
            return Result.Failure<UpdateInvitationResponse>("Email já cadastrado no sistema");

        if (request.Email != invitation.Email)
        {
            var emailTemConvitePendente = await _unitOfWork.UserInvitations.EmailHasPendingInvitationAsync(request.Email, requestingUser.CompanyId.Value);
            if (emailTemConvitePendente)
                return Result.Failure<UpdateInvitationResponse>("Já existe convite pendente para este email");
        }

        invitation.UpdateInvitationInfo(request.Name, request.Email, request.Role, request.Cargo);
        await _unitOfWork.UserInvitations.UpdateAsync(invitation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Convite {InvitationId} atualizado por {UserId}. Novo email: {Email}", invitationId, requestingUserId, request.Email);

        var response = new UpdateInvitationResponse
        {
            Success = true,
            Message = "Convite atualizado com sucesso",
            Invitation = new UserInvitationListResponse
            {
                Id = invitation.Id,
                Name = invitation.Name,
                Email = invitation.Email,
                Role = invitation.Role,
                Cargo = invitation.Cargo,
                Status = invitation.Status,
                CreatedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt,
                AcceptedAt = invitation.AcceptedAt,
                InvitedByName = invitation.InvitedByUser?.Name ?? "Sistema",
                AcceptedByName = invitation.AcceptedByUser?.Name,
                IsExpired = invitation.IsExpired(),
                CanBeEdited = invitation.CanBeEdited()
            }
        };

        return Result.Success(response);
    }

    public async Task<Result<CancelInvitationResponse>> CancelInvitationAsync(Guid invitationId, Guid requestingUserId)
    {
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(requestingUserId);

        if (requestingUser == null)
            return Result.Failure<CancelInvitationResponse>("Usuário não encontrado");

        if (requestingUser.Role != UserRole.DonoEmpresaPai && 
            requestingUser.Role != UserRole.Financeiro)
            return Result.Failure<CancelInvitationResponse>("Apenas DonoEmpresaPai e Financeiro podem cancelar convites");

        if (!requestingUser.CompanyId.HasValue)
            return Result.Failure<CancelInvitationResponse>("Usuário não vinculado a empresa");

        var invitation = await _unitOfWork.UserInvitations.GetByIdAsync(invitationId);

        if (invitation == null)
            return Result.Failure<CancelInvitationResponse>("Convite não encontrado");

        if (invitation.CompanyId != requestingUser.CompanyId.Value)
            return Result.Failure<CancelInvitationResponse>("Convite não pertence à sua empresa");

        if (invitation.Status != InvitationStatus.Pending)
            return Result.Failure<CancelInvitationResponse>($"Apenas convites pendentes podem ser cancelados. Status atual: {invitation.Status}");

        invitation.MarkAsCancelled();
        await _unitOfWork.UserInvitations.UpdateAsync(invitation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Convite {InvitationId} cancelado por {UserId}", invitationId, requestingUserId);

        return Result.Success(new CancelInvitationResponse
        {
            Success = true,
            Message = "Convite cancelado com sucesso"
        });
    }
}
