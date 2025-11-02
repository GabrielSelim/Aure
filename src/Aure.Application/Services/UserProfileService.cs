using Microsoft.Extensions.Logging;
using Aure.Application.DTOs.User;
using Aure.Application.DTOs.Company;
using Aure.Application.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;

namespace Aure.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICnpjValidationService _cnpjValidationService;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ICnpjValidationService cnpjValidationService,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork,
        ILogger<UserProfileService> logger)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _cnpjValidationService = cnpjValidationService;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserProfileResponse> GetUserProfileAsync(Guid userId, Guid? requestingUserId = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        var isDono = requestingUserId.HasValue && 
                     await IsDonoEmpresaPai(requestingUserId.Value);

        var isOwnProfile = requestingUserId == userId;

        return new UserProfileResponse
        {
            Id = user.Id,
            Nome = user.Name,
            Email = user.Email,
            Role = (int)user.Role,
            RoleDescricao = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            DataNascimento = user.DataNascimento,
            CPFMascarado = !string.IsNullOrEmpty(user.CPFEncrypted) 
                ? _encryptionService.MaskCPF(_encryptionService.Decrypt(user.CPFEncrypted))
                : null,
            CPF = (isDono || isOwnProfile) && !string.IsNullOrEmpty(user.CPFEncrypted)
                ? _encryptionService.Decrypt(user.CPFEncrypted)
                : null,
            RG = (isDono || isOwnProfile) && !string.IsNullOrEmpty(user.RGEncrypted)
                ? _encryptionService.Decrypt(user.RGEncrypted)
                : null,
            Cargo = user.Cargo,
            TelefoneCelular = user.TelefoneCelular,
            TelefoneFixo = user.TelefoneFixo,
            EnderecoRua = user.EnderecoRua,
            EnderecoNumero = user.EnderecoNumero,
            EnderecoComplemento = user.EnderecoComplemento,
            EnderecoBairro = user.EnderecoBairro,
            EnderecoCidade = user.EnderecoCidade,
            EnderecoEstado = user.EnderecoEstado,
            EnderecoPais = user.EnderecoPais,
            EnderecoCep = user.EnderecoCep,
            EnderecoCompleto = user.GetEnderecoCompleto(),
            AceitouTermosUso = user.AceitouTermosUso,
            DataAceiteTermosUso = user.DataAceiteTermosUso,
            VersaoTermosUsoAceita = user.VersaoTermosUsoAceita,
            AceitouPoliticaPrivacidade = user.AceitouPoliticaPrivacidade,
            DataAceitePoliticaPrivacidade = user.DataAceitePoliticaPrivacidade,
            VersaoPoliticaPrivacidadeAceita = user.VersaoPoliticaPrivacidadeAceita
        };
    }

    public async Task<UserProfileResponse> UpdateFullProfileAsync(Guid userId, UpdateFullProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        if (!string.IsNullOrEmpty(request.CPF))
        {
            var cpfEncrypted = _encryptionService.Encrypt(request.CPF);
            var allUsers = await _userRepository.GetAllAsync();
            var existingUser = allUsers.FirstOrDefault(u => 
                u.CPFEncrypted == cpfEncrypted && u.Id != userId && !u.IsDeleted);
            
            if (existingUser != null)
                throw new InvalidOperationException("CPF já cadastrado no sistema");
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email já cadastrado no sistema");
        }

        if (!string.IsNullOrEmpty(request.SenhaAtual) && !string.IsNullOrEmpty(request.NovaSenha))
        {
            if (!BCrypt.Net.BCrypt.Verify(request.SenhaAtual, user.PasswordHash))
                throw new InvalidOperationException("Senha atual incorreta");

            user.UpdatePassword(request.NovaSenha);
        }

        user.UpdateProfile(
            request.Nome ?? user.Name,
            request.Email ?? user.Email,
            request.Cargo,
            request.TelefoneCelular,
            request.TelefoneFixo,
            request.DataNascimento
        );

        user.UpdateAddress(
            request.EnderecoRua,
            request.EnderecoNumero,
            request.EnderecoComplemento,
            request.EnderecoBairro,
            request.EnderecoCidade,
            request.EnderecoEstado,
            request.EnderecoPais,
            request.EnderecoCep
        );

        if (!string.IsNullOrEmpty(request.CPF) || !string.IsNullOrEmpty(request.RG))
        {
            var cpfEncrypted = !string.IsNullOrEmpty(request.CPF) 
                ? _encryptionService.Encrypt(request.CPF) 
                : user.CPFEncrypted;
            
            var rgEncrypted = !string.IsNullOrEmpty(request.RG) 
                ? _encryptionService.Encrypt(request.RG) 
                : user.RGEncrypted;

            user.UpdateDocuments(cpfEncrypted, rgEncrypted);
        }

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Perfil do usuário {UserId} atualizado com sucesso", userId);

        return await GetUserProfileAsync(userId, userId);
    }

    public async Task<NotificationPreferencesDTO> GetNotificationPreferencesAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        _logger.LogInformation("Usuario {UserId} carregado. NotificationPreferences is null? {IsNull}", userId, user.NotificationPreferences == null);

        if (user.NotificationPreferences == null)
        {
            _logger.LogWarning("Criando preferencias de notificacao para usuario {UserId}", userId);
            var newPreferences = new Domain.Entities.NotificationPreferences(userId);
            user.SetNotificationPreferences(newPreferences);
            await _userRepository.UpdateAsync(user);
            
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar preferências de notificação para usuário {UserId}", userId);
                throw;
            }
        }

        return new NotificationPreferencesDTO
        {
            ReceberEmailNovoContrato = user.NotificationPreferences?.ReceberEmailNovoContrato ?? true,
            ReceberEmailContratoAssinado = user.NotificationPreferences?.ReceberEmailContratoAssinado ?? true,
            ReceberEmailContratoVencendo = user.NotificationPreferences?.ReceberEmailContratoVencendo ?? true,
            ReceberEmailPagamentoProcessado = user.NotificationPreferences?.ReceberEmailPagamentoProcessado ?? true,
            ReceberEmailPagamentoRecebido = user.NotificationPreferences?.ReceberEmailPagamentoRecebido ?? true,
            ReceberEmailNovoFuncionario = user.NotificationPreferences?.ReceberEmailNovoFuncionario ?? true,
            ReceberEmailAlertasFinanceiros = user.NotificationPreferences?.ReceberEmailAlertasFinanceiros ?? true,
            ReceberEmailAtualizacoesSistema = user.NotificationPreferences?.ReceberEmailAtualizacoesSistema ?? true
        };
    }

    public async Task<NotificationPreferencesDTO> UpdateNotificationPreferencesAsync(
        Guid userId, 
        NotificationPreferencesDTO preferences)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        if (user.NotificationPreferences == null)
        {
            user.SetNotificationPreferences(new Domain.Entities.NotificationPreferences(userId));
        }

        if (user.NotificationPreferences != null)
        {
            user.NotificationPreferences.UpdatePreferences(
                preferences.ReceberEmailNovoContrato,
                preferences.ReceberEmailContratoAssinado,
                preferences.ReceberEmailContratoVencendo,
                preferences.ReceberEmailPagamentoProcessado,
                preferences.ReceberEmailPagamentoRecebido,
                preferences.ReceberEmailNovoFuncionario,
                preferences.ReceberEmailAlertasFinanceiros,
                preferences.ReceberEmailAtualizacoesSistema
            );
        }

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Preferências de notificação do usuário {UserId} atualizadas", userId);

        return preferences;
    }

    public async Task AcceptTermsAsync(Guid userId, AcceptTermsRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        if (request.AceitouTermosUso)
        {
            user.AcceptTermsOfUse(request.VersaoTermosUso);
        }

        if (request.AceitouPoliticaPrivacidade)
        {
            user.AcceptPrivacyPolicy(request.VersaoPoliticaPrivacidade);
        }

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Usuário {UserId} aceitou termos - TermosUso: {TermosUso}, Privacidade: {Privacidade}",
            userId, request.AceitouTermosUso, request.AceitouPoliticaPrivacidade);
    }

    public async Task<TermsVersionsResponse> GetTermsVersionsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado");

        const string versaoTermosAtual = "1.0.0";
        const string versaoPrivacidadeAtual = "1.0.0";

        var precisaAceitar = !user.AceitouTermosUso || 
                           user.VersaoTermosUsoAceita != versaoTermosAtual ||
                           !user.AceitouPoliticaPrivacidade ||
                           user.VersaoPoliticaPrivacidadeAceita != versaoPrivacidadeAtual;

        return new TermsVersionsResponse
        {
            VersaoTermosUsoAtual = versaoTermosAtual,
            VersaoPoliticaPrivacidadeAtual = versaoPrivacidadeAtual,
            UsuarioPrecisaAceitar = precisaAceitar
        };
    }

    public async Task<UpdateCompanyPJResponse> UpdateCompanyPJAsync(Guid userId, UpdateCompanyPJRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            throw new ArgumentException("Usuário não encontrado");

        if (user.Role != UserRole.FuncionarioPJ)
            throw new InvalidOperationException("Apenas funcionários PJ podem atualizar empresa PJ");

        if (!user.CompanyId.HasValue)
            throw new ArgumentException("Usuário não possui empresa PJ vinculada");

        var userCompany = await _companyRepository.GetByIdAsync(user.CompanyId.Value);

        if (userCompany == null)
            throw new ArgumentException("Empresa PJ não encontrada");

        if (!string.IsNullOrEmpty(request.Cnpj))
        {
            var cnpjLimpo = System.Text.RegularExpressions.Regex.Replace(request.Cnpj, @"\D", "");
            
            if (cnpjLimpo.Length != 14)
            {
                return new UpdateCompanyPJResponse
                {
                    Sucesso = false,
                    Mensagem = "CNPJ inválido. Deve conter 14 dígitos."
                };
            }

            var companies = await _companyRepository.GetAllAsync();
            var existingCompany = companies.FirstOrDefault(c => 
                c.Id != userCompany.Id && 
                c.Cnpj == cnpjLimpo);

            if (existingCompany != null)
            {
                return new UpdateCompanyPJResponse
                {
                    Sucesso = false,
                    Mensagem = "CNPJ já cadastrado no sistema"
                };
            }

            var validationResult = await _cnpjValidationService.ValidateAsync(cnpjLimpo);
            
            if (!validationResult.IsValid)
            {
                return new UpdateCompanyPJResponse
                {
                    Sucesso = false,
                    Mensagem = validationResult.ErrorMessage ?? "CNPJ inválido na Receita Federal"
                };
            }

            if (!string.IsNullOrEmpty(validationResult.CompanyName) && 
                !string.IsNullOrEmpty(request.RazaoSocial) &&
                !request.ConfirmarDivergenciaRazaoSocial)
            {
                var similarity = CalculateSimilarity(
                    validationResult.CompanyName.ToUpper(), 
                    request.RazaoSocial.ToUpper());

                if (similarity < 0.85)
                {
                    return new UpdateCompanyPJResponse
                    {
                        Sucesso = false,
                        Mensagem = "Divergência encontrada entre a Razão Social informada e a registrada na Receita Federal",
                        DivergenciaRazaoSocial = true,
                        RazaoSocialReceita = validationResult.CompanyName,
                        RazaoSocialInformada = request.RazaoSocial,
                        RequerConfirmacao = true
                    };
                }
            }
        }

        var razaoSocial = request.RazaoSocial ?? userCompany.Name;
        var cnpj = string.IsNullOrEmpty(request.Cnpj) 
            ? userCompany.Cnpj 
            : System.Text.RegularExpressions.Regex.Replace(request.Cnpj, @"\D", "");
        var companyType = request.CompanyType ?? userCompany.Type;

        userCompany.UpdateCompanyInfo(razaoSocial, companyType, userCompany.BusinessModel);

        await _companyRepository.UpdateAsync(userCompany);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Empresa PJ {CompanyId} atualizada pelo usuário {UserId}", userCompany.Id, userId);

        return new UpdateCompanyPJResponse
        {
            Sucesso = true,
            Mensagem = "Empresa PJ atualizada com sucesso",
            Empresa = new CompanyPJData
            {
                Id = userCompany.Id,
                RazaoSocial = userCompany.Name,
                Cnpj = userCompany.Cnpj,
                CompanyType = userCompany.Type
            }
        };
    }

    private async Task<bool> IsDonoEmpresaPai(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.Role == UserRole.DonoEmpresaPai;
    }

    private double CalculateSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            return 0;

        if (source == target)
            return 1;

        int distance = LevenshteinDistance(source, target);
        int maxLength = Math.Max(source.Length, target.Length);
        
        return 1.0 - (double)distance / maxLength;
    }

    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        int[,] distance = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= target.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[source.Length, target.Length];
    }
}
