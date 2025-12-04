using Microsoft.Extensions.Logging;
using Aure.Application.DTOs.Contract;
using Aure.Application.Interfaces;
using Aure.Domain.Common;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Interfaces;
using System.Globalization;

namespace Aure.Application.Services
{
    public class ContractTemplateConfigService : IContractTemplateConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContractTemplateConfigService> _logger;
        private readonly ContractTemplatePresetService _presetService;
        private readonly IEncryptionService _encryptionService;

        public ContractTemplateConfigService(
            IUnitOfWork unitOfWork,
            ILogger<ContractTemplateConfigService> logger,
            IEncryptionService encryptionService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _encryptionService = encryptionService;
            _presetService = new ContractTemplatePresetService();
        }

        public Task<Result<List<ContractTemplatePresetResponse>>> GetPresetsAsync()
        {
            try
            {
                var presets = _presetService.GetAllPresets();
                return Task.FromResult(Result.Success(presets));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar presets de template");
                return Task.FromResult(Result.Failure<List<ContractTemplatePresetResponse>>("Erro ao buscar presets"));
            }
        }

        public Task<Result<ContractTemplatePresetResponse?>> GetPresetByTipoAsync(string tipo)
        {
            try
            {
                var preset = _presetService.GetPresetByTipo(tipo);
                if (preset == null)
                    return Task.FromResult(Result.Failure<ContractTemplatePresetResponse?>("Preset não encontrado"));

                return Task.FromResult(Result.Success<ContractTemplatePresetResponse?>(preset));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar preset {Tipo}", tipo);
                return Task.FromResult(Result.Failure<ContractTemplatePresetResponse?>("Erro ao buscar preset"));
            }
        }

        public async Task<Result<List<ContractTemplateConfigResponse>>> GetAllCompanyConfigsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Iniciando busca de configurações para usuário {UserId}", userId);
                
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Usuário {UserId} não encontrado", userId);
                    return Result.Failure<List<ContractTemplateConfigResponse>>("Usuário não encontrado");
                }

                if (!user.CompanyId.HasValue)
                {
                    _logger.LogWarning("Usuário {UserId} não possui CompanyId", userId);
                    return Result.Failure<List<ContractTemplateConfigResponse>>("Usuário não pertence a uma empresa");
                }

                _logger.LogInformation("Buscando configurações para empresa {CompanyId}", user.CompanyId.Value);
                
                var configs = await _unitOfWork.ContractTemplateConfigs.GetAllByCompanyIdAsync(user.CompanyId.Value);
                
                _logger.LogInformation("Encontradas {Count} configurações", configs.Count());
                
                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                var nomeEmpresa = company?.Name ?? "Empresa não encontrada";
                
                var responses = configs.Select(config => new ContractTemplateConfigResponse
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    NomeEmpresa = nomeEmpresa,
                    NomeConfig = config.NomeConfig,
                    Categoria = config.Categoria,
                    TituloServico = config.TituloServico,
                    DescricaoServico = config.DescricaoServico,
                    LocalPrestacaoServico = config.LocalPrestacaoServico,
                    DetalhamentoServicos = config.DetalhamentoServicos,
                    ClausulaAjudaCusto = config.ClausulaAjudaCusto,
                    ObrigacoesContratado = config.ObrigacoesContratado,
                    ObrigacoesContratante = config.ObrigacoesContratante,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                }).ToList();

                _logger.LogInformation("Retornando {Count} configurações convertidas", responses.Count);
                
                return Result.Success(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configurações da empresa para usuário {UserId}. Mensagem: {Message}, StackTrace: {StackTrace}", userId, ex.Message, ex.StackTrace);
                return Result.Failure<List<ContractTemplateConfigResponse>>($"Erro ao buscar configurações: {ex.Message}");
            }
        }

        public async Task<Result<ContractTemplateConfigResponse?>> GetCompanyConfigByNomeAsync(Guid userId, string nomeConfig)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<ContractTemplateConfigResponse?>("Usuário não encontrado");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<ContractTemplateConfigResponse?>("Usuário não pertence a uma empresa");

                var config = await _unitOfWork.ContractTemplateConfigs.GetByCompanyIdAndNomeAsync(user.CompanyId.Value, nomeConfig);
                
                if (config == null)
                    return Result.Success<ContractTemplateConfigResponse?>(null);

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                var nomeEmpresa = company?.Name ?? "Empresa não encontrada";

                var response = new ContractTemplateConfigResponse
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    NomeEmpresa = nomeEmpresa,
                    NomeConfig = config.NomeConfig,
                    Categoria = config.Categoria,
                    TituloServico = config.TituloServico,
                    DescricaoServico = config.DescricaoServico,
                    LocalPrestacaoServico = config.LocalPrestacaoServico,
                    DetalhamentoServicos = config.DetalhamentoServicos,
                    ClausulaAjudaCusto = config.ClausulaAjudaCusto,
                    ObrigacoesContratado = config.ObrigacoesContratado,
                    ObrigacoesContratante = config.ObrigacoesContratante,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return Result.Success<ContractTemplateConfigResponse?>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configuração {NomeConfig} da empresa para usuário {UserId}", nomeConfig, userId);
                return Result.Failure<ContractTemplateConfigResponse?>("Erro ao buscar configuração");
            }
        }

        public async Task<Result<ContractTemplateConfigResponse>> CreateOrUpdateConfigAsync(Guid userId, ContractTemplateConfigRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<ContractTemplateConfigResponse>("Usuário não encontrado");

                if (user.Role != UserRole.DonoEmpresaPai)
                    return Result.Failure<ContractTemplateConfigResponse>("Apenas o dono da empresa pode configurar templates");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<ContractTemplateConfigResponse>("Usuário não pertence a uma empresa");

                var existingConfig = await _unitOfWork.ContractTemplateConfigs.GetByCompanyIdAndNomeAsync(user.CompanyId.Value, request.NomeConfig);

                ContractTemplateConfig config;

                if (existingConfig != null)
                {
                    existingConfig.Update(
                        request.NomeConfig,
                        request.Categoria,
                        request.TituloServico,
                        request.DescricaoServico,
                        request.LocalPrestacaoServico,
                        request.DetalhamentoServicos,
                        request.ObrigacoesContratado,
                        request.ObrigacoesContratante,
                        request.ClausulaAjudaCusto
                    );
                    await _unitOfWork.ContractTemplateConfigs.UpdateAsync(existingConfig);
                    config = existingConfig;
                }
                else
                {
                    config = new ContractTemplateConfig(
                        user.CompanyId.Value,
                        request.NomeConfig,
                        request.Categoria,
                        request.TituloServico,
                        request.DescricaoServico,
                        request.LocalPrestacaoServico,
                        request.DetalhamentoServicos,
                        request.ObrigacoesContratado,
                        request.ObrigacoesContratante,
                        request.ClausulaAjudaCusto
                    );
                    await _unitOfWork.ContractTemplateConfigs.AddAsync(config);
                }

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);

                var response = new ContractTemplateConfigResponse
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    NomeEmpresa = company?.Name ?? string.Empty,
                    NomeConfig = config.NomeConfig,
                    Categoria = config.Categoria,
                    TituloServico = config.TituloServico,
                    DescricaoServico = config.DescricaoServico,
                    LocalPrestacaoServico = config.LocalPrestacaoServico,
                    DetalhamentoServicos = config.DetalhamentoServicos,
                    ClausulaAjudaCusto = config.ClausulaAjudaCusto,
                    ObrigacoesContratado = config.ObrigacoesContratado,
                    ObrigacoesContratante = config.ObrigacoesContratante,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar/atualizar configuração de template para usuário {UserId}", userId);
                return Result.Failure<ContractTemplateConfigResponse>($"Erro ao salvar configuração: {ex.Message}");
            }
        }

        public async Task<Result<ContractTemplateConfigResponse>> ClonarPresetAsync(Guid userId, string tipoPreset, string nomeConfig)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<ContractTemplateConfigResponse>("Usuário não encontrado");

                if (user.Role != UserRole.DonoEmpresaPai)
                    return Result.Failure<ContractTemplateConfigResponse>("Apenas o dono da empresa pode clonar presets");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<ContractTemplateConfigResponse>("Usuário não pertence a uma empresa");

                var preset = _presetService.GetPresetByTipo(tipoPreset);
                if (preset == null)
                    return Result.Failure<ContractTemplateConfigResponse>("Preset não encontrado");

                var existingConfig = await _unitOfWork.ContractTemplateConfigs.GetByCompanyIdAndNomeAsync(user.CompanyId.Value, nomeConfig);
                if (existingConfig != null)
                    return Result.Failure<ContractTemplateConfigResponse>($"Já existe uma configuração com o nome '{nomeConfig}'");

                var config = new ContractTemplateConfig(
                    user.CompanyId.Value,
                    nomeConfig,
                    preset.Tipo,
                    preset.Configuracao.TituloServico,
                    preset.Configuracao.DescricaoServico,
                    preset.Configuracao.LocalPrestacaoServico,
                    preset.Configuracao.DetalhamentoServicos,
                    preset.Configuracao.ObrigacoesContratado,
                    preset.Configuracao.ObrigacoesContratante,
                    preset.Configuracao.ClausulaAjudaCusto
                );

                await _unitOfWork.ContractTemplateConfigs.AddAsync(config);

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);

                var response = new ContractTemplateConfigResponse
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    NomeEmpresa = company?.Name ?? string.Empty,
                    NomeConfig = config.NomeConfig,
                    Categoria = config.Categoria,
                    TituloServico = config.TituloServico,
                    DescricaoServico = config.DescricaoServico,
                    LocalPrestacaoServico = config.LocalPrestacaoServico,
                    DetalhamentoServicos = config.DetalhamentoServicos,
                    ClausulaAjudaCusto = config.ClausulaAjudaCusto,
                    ObrigacoesContratado = config.ObrigacoesContratado,
                    ObrigacoesContratante = config.ObrigacoesContratante,
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao clonar preset {TipoPreset} para usuário {UserId}", tipoPreset, userId);
                return Result.Failure<ContractTemplateConfigResponse>($"Erro ao clonar preset: {ex.Message}");
            }
        }

        public async Task<Result<string>> PreviewContractHtmlAsync(Guid userId, PreviewTemplateRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<string>("Usuário não encontrado");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<string>("Usuário não pertence a uma empresa");

                if (!request.FuncionarioPJId.HasValue && request.DadosContratadoManual == null)
                    return Result.Failure<string>("Informe o funcionário PJ ou os dados manuais do contratado");

                if (request.FuncionarioPJId.HasValue && request.DadosContratadoManual != null)
                    return Result.Failure<string>("Informe apenas o funcionário PJ OU os dados manuais, não ambos");

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                if (company == null)
                    return Result.Failure<string>("Empresa não encontrada");

                var validacaoEmpresa = ValidarDadosEmpresaContratante(company);
                if (!validacaoEmpresa.IsSuccess)
                    return Result.Failure<string>(validacaoEmpresa.Error);

                var validacaoRepresentante = ValidarDadosRepresentante(user);
                if (!validacaoRepresentante.IsSuccess)
                    return Result.Failure<string>(validacaoRepresentante.Error);

                if (string.IsNullOrWhiteSpace(company.AddressStreet) || 
                    string.IsNullOrWhiteSpace(company.AddressNumber) ||
                    string.IsNullOrWhiteSpace(company.AddressCity) ||
                    string.IsNullOrWhiteSpace(company.AddressState))
                {
                    return Result.Failure<string>("Dados de endereço da empresa contratante estão incompletos. Por favor, complete o cadastro da empresa.");
                }

                if (string.IsNullOrWhiteSpace(user.CPFEncrypted))
                {
                    return Result.Failure<string>("CPF do representante não cadastrado. Por favor, complete seu perfil.");
                }

                if (!user.DataNascimento.HasValue)
                {
                    return Result.Failure<string>("Data de nascimento do representante não cadastrada. Por favor, complete seu perfil.");
                }

                if (string.IsNullOrWhiteSpace(user.EnderecoRua) || 
                    string.IsNullOrWhiteSpace(user.EnderecoCidade) ||
                    string.IsNullOrWhiteSpace(user.EnderecoEstado))
                {
                    return Result.Failure<string>("Dados de endereço residencial do representante estão incompletos. Por favor, complete seu perfil.");
                }

                User? funcionarioPJ = null;
                Company? empresaPJ = null;
                DadosContratadoManualRequest? dadosManual = null;

                if (request.FuncionarioPJId.HasValue)
                {
                    funcionarioPJ = await _unitOfWork.Users.GetByIdAsync(request.FuncionarioPJId.Value);
                    if (funcionarioPJ == null)
                        return Result.Failure<string>("Funcionário PJ não encontrado");

                    var validacaoContratadoPJ = ValidarDadosContratadoPJ(funcionarioPJ);
                    if (!validacaoContratadoPJ.IsSuccess)
                        return Result.Failure<string>(validacaoContratadoPJ.Error);

                    empresaPJ = funcionarioPJ.CompanyId.HasValue 
                        ? await _unitOfWork.Companies.GetByIdAsync(funcionarioPJ.CompanyId.Value)
                        : null;

                    if (empresaPJ != null)
                    {
                        if (string.IsNullOrWhiteSpace(empresaPJ.AddressStreet) && !string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoRua))
                        {
                            empresaPJ.UpdateAddress(
                                funcionarioPJ.EnderecoRua,
                                funcionarioPJ.EnderecoNumero ?? "S/N",
                                funcionarioPJ.EnderecoComplemento,
                                funcionarioPJ.EnderecoBairro ?? "",
                                funcionarioPJ.EnderecoCidade ?? "",
                                funcionarioPJ.EnderecoEstado ?? "",
                                funcionarioPJ.EnderecoPais ?? "Brasil",
                                funcionarioPJ.EnderecoCep ?? ""
                            );
                            await _unitOfWork.Companies.UpdateAsync(empresaPJ);
                            await _unitOfWork.SaveChangesAsync();
                        }

                        var validacaoEmpresaPJ = ValidarDadosEmpresaPJ(empresaPJ);
                        if (!validacaoEmpresaPJ.IsSuccess)
                            return Result.Failure<string>(validacaoEmpresaPJ.Error);
                    }
                }
                else
                {
                    dadosManual = request.DadosContratadoManual;
                    
                    var validacaoDadosManual = ValidarDadosContratadoManual(dadosManual!);
                    if (!validacaoDadosManual.IsSuccess)
                        return Result.Failure<string>(validacaoDadosManual.Error);
                }

                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "ContratoPrestacaoServicosGenerico.html");
                if (!File.Exists(templatePath))
                {
                    templatePath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Aure.Infrastructure", "Templates", "ContratoPrestacaoServicosGenerico.html");
                }

                if (!File.Exists(templatePath))
                    return Result.Failure<string>("Template não encontrado");

                var html = await File.ReadAllTextAsync(templatePath);

                html = html.Replace("{{TITULO_SERVICO}}", request.TemplateConfig.TituloServico);
                html = html.Replace("{{DESCRICAO_SERVICO}}", request.TemplateConfig.DescricaoServico);
                html = html.Replace("{{LOCAL_PRESTACAO_SERVICO}}", request.TemplateConfig.LocalPrestacaoServico);
                
                var detalhamentoHtml = "<ol type=\"I\">";
                foreach (var item in request.TemplateConfig.DetalhamentoServicos)
                {
                    detalhamentoHtml += $"<li>{item}</li>";
                }
                detalhamentoHtml += "</ol>";
                html = html.Replace("{{DETALHAMENTO_SERVICOS}}", detalhamentoHtml);

                html = html.Replace("{{CLAUSULA_AJUDA_CUSTO}}", request.TemplateConfig.ClausulaAjudaCusto ?? string.Empty);

                var obrigacoesContratadoHtml = "<ol type=\"I\">";
                foreach (var item in request.TemplateConfig.ObrigacoesContratado)
                {
                    obrigacoesContratadoHtml += $"<li>{item}</li>";
                }
                obrigacoesContratadoHtml += "</ol>";
                html = html.Replace("{{OBRIGACOES_CONTRATADO}}", obrigacoesContratadoHtml);

                var obrigacoesContratanteHtml = "<ol type=\"I\">";
                foreach (var item in request.TemplateConfig.ObrigacoesContratante)
                {
                    obrigacoesContratanteHtml += $"<li>{item}</li>";
                }
                obrigacoesContratanteHtml += "</ol>";
                html = html.Replace("{{OBRIGACOES_CONTRATANTE}}", obrigacoesContratanteHtml);

                var dataAtual = DateTime.UtcNow;
                var cultura = new CultureInfo("pt-BR");

                html = html.Replace("{{NOME_EMPRESA_CONTRATANTE}}", company.Name);
                html = html.Replace("{{CNPJ_CONTRATANTE}}", FormatCnpj(company.Cnpj));
                html = html.Replace("{{ENDERECO_CONTRATANTE}}", company.AddressStreet ?? "");
                html = html.Replace("{{NUMERO_CONTRATANTE}}", company.AddressNumber ?? "");
                html = html.Replace("{{BAIRRO_CONTRATANTE}}", company.AddressNeighborhood ?? "");
                html = html.Replace("{{CIDADE_CONTRATANTE}}", company.AddressCity ?? "");
                html = html.Replace("{{UF_CONTRATANTE}}", company.AddressState ?? "");
                html = html.Replace("{{CEP_CONTRATANTE}}", FormatCep(company.AddressZipCode ?? ""));
                html = html.Replace("{{ESTADO_REGISTRO_CONTRATANTE}}", company.AddressState ?? "");
                html = html.Replace("{{NIRE_CONTRATANTE}}", company.Nire ?? "");

                html = html.Replace("{{NOME_REPRESENTANTE_CONTRATANTE}}", user.Name);
                html = html.Replace("{{NACIONALIDADE_REPRESENTANTE}}", user.Nacionalidade ?? "Brasileiro(a)");
                html = html.Replace("{{ESTADO_CIVIL_REPRESENTANTE}}", user.EstadoCivil ?? "");
                html = html.Replace("{{DATA_NASCIMENTO_REPRESENTANTE}}", user.DataNascimento?.ToString("dd/MM/yyyy") ?? "");
                html = html.Replace("{{PROFISSAO_REPRESENTANTE}}", user.Cargo ?? "Empresário");
                html = html.Replace("{{CPF_REPRESENTANTE}}", user.CPFEncrypted != null ? FormatCpf(_encryptionService.Decrypt(user.CPFEncrypted)) : "");
                html = html.Replace("{{RG_REPRESENTANTE}}", user.RGEncrypted != null ? _encryptionService.Decrypt(user.RGEncrypted) : "");
                html = html.Replace("{{ORGAO_EXPEDIDOR_REPRESENTANTE}}", user.OrgaoExpedidorRG ?? "SSP");
                html = html.Replace("{{ENDERECO_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoRua ?? "");
                html = html.Replace("{{NUMERO_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoNumero ?? "");
                html = html.Replace("{{BAIRRO_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoBairro ?? "");
                html = html.Replace("{{CIDADE_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoCidade ?? "");
                html = html.Replace("{{UF_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoEstado ?? "");
                html = html.Replace("{{CEP_RESIDENCIAL_REPRESENTANTE}}", FormatCep(user.EnderecoCep ?? ""));

                if (funcionarioPJ != null)
                {
                    var cpfContratado = string.Empty;
                    if (!string.IsNullOrEmpty(funcionarioPJ.CPFEncrypted))
                    {
                        cpfContratado = _encryptionService.Decrypt(funcionarioPJ.CPFEncrypted);
                    }

                    html = html.Replace("{{RAZAO_SOCIAL_CONTRATADO}}", empresaPJ?.Name ?? funcionarioPJ.Name);
                    html = html.Replace("{{CNPJ_CONTRATADO}}", empresaPJ != null ? FormatCnpj(empresaPJ.Cnpj) : "");
                    html = html.Replace("{{ENDERECO_CONTRATADO}}", empresaPJ?.AddressStreet ?? funcionarioPJ.EnderecoRua ?? "");
                    html = html.Replace("{{NUMERO_CONTRATADO}}", empresaPJ?.AddressNumber ?? funcionarioPJ.EnderecoNumero ?? "");
                    html = html.Replace("{{BAIRRO_CONTRATADO}}", empresaPJ?.AddressNeighborhood ?? funcionarioPJ.EnderecoBairro ?? "");
                    html = html.Replace("{{CIDADE_CONTRATADO}}", empresaPJ?.AddressCity ?? funcionarioPJ.EnderecoCidade ?? "");
                    html = html.Replace("{{ESTADO_CONTRATADO}}", empresaPJ?.AddressState ?? funcionarioPJ.EnderecoEstado ?? "");

                    html = html.Replace("{{NOME_CONTRATADO}}", funcionarioPJ.Name);
                    html = html.Replace("{{NACIONALIDADE_CONTRATADO}}", funcionarioPJ.Nacionalidade ?? "Brasileiro(a)");
                    html = html.Replace("{{ESTADO_CIVIL_CONTRATADO}}", funcionarioPJ.EstadoCivil ?? "");
                    html = html.Replace("{{DATA_NASCIMENTO_CONTRATADO}}", funcionarioPJ.DataNascimento?.ToString("dd/MM/yyyy") ?? "");
                    html = html.Replace("{{PROFISSAO_CONTRATADO}}", funcionarioPJ.Cargo ?? "Prestador de Serviços");
                    html = html.Replace("{{CPF_CONTRATADO}}", FormatCpf(cpfContratado));
                    html = html.Replace("{{ENDERECO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoRua ?? "");
                    html = html.Replace("{{NUMERO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoNumero ?? "");
                    html = html.Replace("{{BAIRRO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoBairro ?? "");
                    html = html.Replace("{{CIDADE_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoCidade ?? "");
                    html = html.Replace("{{ESTADO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoEstado ?? "");
                    html = html.Replace("{{CEP_RESIDENCIAL_CONTRATADO}}", FormatCep(funcionarioPJ.EnderecoCep ?? ""));
                }
                else if (dadosManual != null)
                {
                    html = html.Replace("{{RAZAO_SOCIAL_CONTRATADO}}", dadosManual.RazaoSocial);
                    html = html.Replace("{{CNPJ_CONTRATADO}}", FormatCnpj(dadosManual.Cnpj));
                    html = html.Replace("{{ENDERECO_CONTRATADO}}", dadosManual.Rua);
                    html = html.Replace("{{NUMERO_CONTRATADO}}", dadosManual.Numero);
                    html = html.Replace("{{BAIRRO_CONTRATADO}}", dadosManual.Bairro);
                    html = html.Replace("{{CIDADE_CONTRATADO}}", dadosManual.Cidade);
                    html = html.Replace("{{ESTADO_CONTRATADO}}", dadosManual.Estado);

                    html = html.Replace("{{NOME_CONTRATADO}}", dadosManual.NomeCompleto);
                    html = html.Replace("{{NACIONALIDADE_CONTRATADO}}", dadosManual.Nacionalidade ?? "Brasileiro(a)");
                    html = html.Replace("{{ESTADO_CIVIL_CONTRATADO}}", dadosManual.EstadoCivil ?? "");
                    html = html.Replace("{{DATA_NASCIMENTO_CONTRATADO}}", dadosManual.DataNascimento?.ToString("dd/MM/yyyy") ?? "");
                    html = html.Replace("{{PROFISSAO_CONTRATADO}}", dadosManual.Profissao ?? "Prestador de Serviços");
                    html = html.Replace("{{CPF_CONTRATADO}}", FormatCpf(dadosManual.Cpf));
                    html = html.Replace("{{ENDERECO_RESIDENCIAL_CONTRATADO}}", dadosManual.Rua);
                    html = html.Replace("{{NUMERO_RESIDENCIAL_CONTRATADO}}", dadosManual.Numero);
                    html = html.Replace("{{BAIRRO_RESIDENCIAL_CONTRATADO}}", dadosManual.Bairro);
                    html = html.Replace("{{CIDADE_RESIDENCIAL_CONTRATADO}}", dadosManual.Cidade);
                    html = html.Replace("{{ESTADO_RESIDENCIAL_CONTRATADO}}", dadosManual.Estado);
                    html = html.Replace("{{CEP_RESIDENCIAL_CONTRATADO}}", FormatCep(dadosManual.Cep));
                }

                html = html.Replace("{{DATA_PROPOSTA}}", dataAtual.ToString("dd/MM/yyyy"));
                html = html.Replace("{{PRAZO_VIGENCIA}}", $"{request.PrazoVigenciaMeses} meses");
                html = html.Replace("{{PRAZO_VIGENCIA_EXTENSO}}", NumeroPorExtenso(request.PrazoVigenciaMeses) + " meses");
                
                html = html.Replace("{{VALOR_MENSAL}}", $"R$ {request.ValorMensal:N2}");
                html = html.Replace("{{VALOR_MENSAL_EXTENSO}}", ValorPorExtenso(request.ValorMensal));
                
                html = html.Replace("{{DIA_VENCIMENTO_NF}}", request.DiaVencimentoNF.ToString());
                html = html.Replace("{{DIA_VENCIMENTO_NF_EXTENSO}}", NumeroPorExtenso(request.DiaVencimentoNF));
                html = html.Replace("{{DIA_PAGAMENTO}}", request.DiaPagamento.ToString());

                html = html.Replace("{{CIDADE_FORO}}", company.AddressCity ?? "");
                html = html.Replace("{{ESTADO_FORO}}", company.AddressState ?? "");
                html = html.Replace("{{CIDADE_ASSINATURA}}", company.AddressCity ?? "");
                html = html.Replace("{{DATA_ASSINATURA}}", dataAtual.ToString("dd 'de' MMMM 'de' yyyy", cultura));

                html = System.Text.RegularExpressions.Regex.Replace(html, @",\s*,", ",");
                html = System.Text.RegularExpressions.Regex.Replace(html, @"\s+", " ");

                return Result.Success(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar preview de contrato");
                return Result.Failure<string>($"Erro ao gerar preview: {ex.Message}");
            }
        }

        public async Task<Result<Guid>> GerarContratoComConfigAsync(Guid userId, GerarContratoComConfigRequest request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<Guid>("Usuário não encontrado");

                if (user.Role != UserRole.DonoEmpresaPai && user.Role != UserRole.Juridico)
                    return Result.Failure<Guid>("Apenas o dono da empresa ou jurídico podem gerar contratos");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<Guid>("Usuário não pertence a uma empresa");

                var config = await _unitOfWork.ContractTemplateConfigs.GetByCompanyIdAndNomeAsync(user.CompanyId.Value, request.NomeConfig);
                if (config == null)
                    return Result.Failure<Guid>("Configuração de template não encontrada");

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                if (company == null)
                    return Result.Failure<Guid>("Empresa não encontrada");

                var validacaoEmpresa = ValidarDadosEmpresaContratante(company);
                if (!validacaoEmpresa.IsSuccess)
                    return Result.Failure<Guid>(validacaoEmpresa.Error);

                var validacaoRepresentante = ValidarDadosRepresentante(user);
                if (!validacaoRepresentante.IsSuccess)
                    return Result.Failure<Guid>(validacaoRepresentante.Error);

                if (string.IsNullOrWhiteSpace(company.AddressStreet) || 
                    string.IsNullOrWhiteSpace(company.AddressNumber) ||
                    string.IsNullOrWhiteSpace(company.AddressCity) ||
                    string.IsNullOrWhiteSpace(company.AddressState))
                {
                    return Result.Failure<Guid>("Dados de endereço da empresa contratante estão incompletos. Por favor, complete o cadastro da empresa.");
                }

                if (string.IsNullOrWhiteSpace(user.CPFEncrypted))
                {
                    return Result.Failure<Guid>("CPF do representante não cadastrado. Por favor, complete seu perfil.");
                }

                if (!user.DataNascimento.HasValue)
                {
                    return Result.Failure<Guid>("Data de nascimento do representante não cadastrada. Por favor, complete seu perfil.");
                }

                if (string.IsNullOrWhiteSpace(user.EnderecoRua) || 
                    string.IsNullOrWhiteSpace(user.EnderecoCidade) ||
                    string.IsNullOrWhiteSpace(user.EnderecoEstado))
                {
                    return Result.Failure<Guid>("Dados de endereço residencial do representante estão incompletos. Por favor, complete seu perfil.");
                }

                if (!request.FuncionarioPJId.HasValue && request.DadosContratadoManual == null)
                    return Result.Failure<Guid>("Informe o funcionário PJ ou os dados manuais do contratado");

                if (request.FuncionarioPJId.HasValue && request.DadosContratadoManual != null)
                    return Result.Failure<Guid>("Informe apenas o funcionário PJ OU os dados manuais, não ambos");

                Guid providerId;

                if (request.FuncionarioPJId.HasValue)
                {
                    var funcionarioPJ = await _unitOfWork.Users.GetByIdAsync(request.FuncionarioPJId.Value);
                    if (funcionarioPJ == null)
                        return Result.Failure<Guid>("Funcionário PJ não encontrado");

                    if (funcionarioPJ.Role != UserRole.FuncionarioPJ)
                        return Result.Failure<Guid>("Usuário selecionado não é um funcionário PJ");

                    var allRelationships = await _unitOfWork.CompanyRelationships.GetAllAsync();
                    var temRelacionamento = allRelationships.Any(r => 
                        r.ClientCompanyId == user.CompanyId 
                        && r.ProviderCompanyId == funcionarioPJ.CompanyId 
                        && r.Status == RelationshipStatus.Active);

                    if (!temRelacionamento)
                        return Result.Failure<Guid>("Funcionário PJ não possui relacionamento ativo com sua empresa");

                    var validacaoContratadoPJ = ValidarDadosContratadoPJ(funcionarioPJ);
                    if (!validacaoContratadoPJ.IsSuccess)
                        return Result.Failure<Guid>(validacaoContratadoPJ.Error);

                    if (funcionarioPJ.CompanyId.HasValue)
                    {
                        var empresaPJ = await _unitOfWork.Companies.GetByIdAsync(funcionarioPJ.CompanyId.Value);
                        if (empresaPJ != null)
                        {
                            if (string.IsNullOrWhiteSpace(empresaPJ.AddressStreet) && !string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoRua))
                            {
                                empresaPJ.UpdateAddress(
                                    funcionarioPJ.EnderecoRua,
                                    funcionarioPJ.EnderecoNumero ?? "S/N",
                                    funcionarioPJ.EnderecoComplemento,
                                    funcionarioPJ.EnderecoBairro ?? "",
                                    funcionarioPJ.EnderecoCidade ?? "",
                                    funcionarioPJ.EnderecoEstado ?? "",
                                    funcionarioPJ.EnderecoPais ?? "Brasil",
                                    funcionarioPJ.EnderecoCep ?? ""
                                );
                                await _unitOfWork.Companies.UpdateAsync(empresaPJ);
                                await _unitOfWork.SaveChangesAsync();
                            }

                            var validacaoEmpresaPJ = ValidarDadosEmpresaPJ(empresaPJ);
                            if (!validacaoEmpresaPJ.IsSuccess)
                                return Result.Failure<Guid>(validacaoEmpresaPJ.Error);
                        }
                    }

                    var existingContract = await _unitOfWork.Contracts.GetActivePJContractByUserIdAsync(request.FuncionarioPJId.Value);
                    if (existingContract != null)
                        return Result.Failure<Guid>("Funcionário PJ já possui um contrato ativo");

                    providerId = funcionarioPJ.Id;
                }
                else
                {
                    var validacaoDadosManual = ValidarDadosContratadoManual(request.DadosContratadoManual!);
                    if (!validacaoDadosManual.IsSuccess)
                        return Result.Failure<Guid>(validacaoDadosManual.Error);

                    var tempCompany = new Company(
                        request.DadosContratadoManual!.RazaoSocial,
                        request.DadosContratadoManual.Cnpj,
                        CompanyType.Provider,
                        BusinessModel.ContractedPJ
                    );

                    await _unitOfWork.Companies.AddAsync(tempCompany);

                    var tempUser = new User(
                        request.DadosContratadoManual.NomeCompleto,
                        request.DadosContratadoManual.Email,
                        Guid.NewGuid().ToString(),
                        UserRole.FuncionarioPJ,
                        tempCompany.Id
                    );

                    tempUser.SetCpf(request.DadosContratadoManual.Cpf);
                    
                    if (!string.IsNullOrWhiteSpace(request.DadosContratadoManual.Rg))
                        tempUser.SetRg(request.DadosContratadoManual.Rg);

                    if (request.DadosContratadoManual.DataNascimento.HasValue)
                        tempUser.SetBirthDate(request.DadosContratadoManual.DataNascimento.Value);

                    if (!string.IsNullOrWhiteSpace(request.DadosContratadoManual.Profissao))
                        tempUser.SetPosition(request.DadosContratadoManual.Profissao);

                    tempUser.UpdateProfile(
                        request.DadosContratadoManual.NomeCompleto,
                        request.DadosContratadoManual.Email,
                        request.DadosContratadoManual.TelefoneCelular,
                        request.DadosContratadoManual.TelefoneFixo
                    );

                    tempUser.UpdateAddress(
                        request.DadosContratadoManual.Rua,
                        request.DadosContratadoManual.Numero,
                        request.DadosContratadoManual.Complemento,
                        request.DadosContratadoManual.Bairro,
                        request.DadosContratadoManual.Cidade,
                        request.DadosContratadoManual.Estado,
                        request.DadosContratadoManual.Pais,
                        request.DadosContratadoManual.Cep
                    );

                    await _unitOfWork.Users.AddAsync(tempUser);

                    providerId = tempUser.Id;
                }

                var dataInicio = request.DataInicioVigencia ?? DateTime.UtcNow.Date;
                var dataFim = dataInicio.AddMonths(request.PrazoVigenciaMeses);

                var contract = new Contract(
                    user.CompanyId.Value,
                    providerId,
                    dataInicio,
                    dataFim,
                    request.ValorMensal,
                    ContractType.PJ
                );

                contract.SetPaymentDetails(
                    request.DiaVencimentoNF,
                    request.DiaPagamento
                );

                await _unitOfWork.Contracts.AddAsync(contract);

                _logger.LogInformation(
                    "Contrato PJ criado com sucesso. ContractId: {ContractId}, ProviderId: {ProviderId}, Config: {NomeConfig}, Modo: {Modo}",
                    contract.Id,
                    providerId,
                    request.NomeConfig,
                    request.FuncionarioPJId.HasValue ? "Funcionário Cadastrado" : "Dados Manuais"
                );

                return Result.Success(contract.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar contrato com config {NomeConfig}", request.NomeConfig);
                return Result.Failure<Guid>($"Erro ao gerar contrato: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteCompanyConfigAsync(Guid userId, string nomeConfig)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<bool>("Usuário não encontrado");

                if (user.Role != UserRole.DonoEmpresaPai)
                    return Result.Failure<bool>("Apenas o dono da empresa pode deletar configurações");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<bool>("Usuário não pertence a uma empresa");

                var config = await _unitOfWork.ContractTemplateConfigs.GetByCompanyIdAndNomeAsync(user.CompanyId.Value, nomeConfig);
                if (config == null)
                    return Result.Failure<bool>("Configuração não encontrada");

                await _unitOfWork.ContractTemplateConfigs.DeleteAsync(config.Id);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar configuração {NomeConfig} para usuário {UserId}", nomeConfig, userId);
                return Result.Failure<bool>("Erro ao deletar configuração");
            }
        }

        public async Task<Result<ValidacaoContratoResponse>> ValidarDadosContratoAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<ValidacaoContratoResponse>("Usuário não encontrado");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<ValidacaoContratoResponse>("Usuário não pertence a uma empresa");

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                if (company == null)
                    return Result.Failure<ValidacaoContratoResponse>("Empresa não encontrada");

                var response = new ValidacaoContratoResponse
                {
                    NomeRepresentante = user.Name,
                    CargoRepresentante = user.Role == UserRole.DonoEmpresaPai ? "Proprietário" : "Jurídico",
                    NomeEmpresa = company.Name
                };

                if (string.IsNullOrWhiteSpace(company.AddressStreet))
                    response.CamposEmpresaFaltando.Add("Rua");
                
                if (string.IsNullOrWhiteSpace(company.AddressNumber))
                    response.CamposEmpresaFaltando.Add("Número do endereço");
                
                if (string.IsNullOrWhiteSpace(company.AddressCity))
                    response.CamposEmpresaFaltando.Add("Cidade");
                
                if (string.IsNullOrWhiteSpace(company.AddressState))
                    response.CamposEmpresaFaltando.Add("Estado");
                
                if (string.IsNullOrWhiteSpace(company.AddressNeighborhood))
                    response.CamposEmpresaFaltando.Add("Bairro");
                
                if (string.IsNullOrWhiteSpace(company.AddressZipCode))
                    response.CamposEmpresaFaltando.Add("CEP");

                if (string.IsNullOrWhiteSpace(user.CPFEncrypted))
                    response.CamposRepresentanteFaltando.Add("CPF");
                
                if (!user.DataNascimento.HasValue)
                    response.CamposRepresentanteFaltando.Add("Data de nascimento");
                
                if (string.IsNullOrWhiteSpace(user.EnderecoRua))
                    response.CamposRepresentanteFaltando.Add("Rua (endereço residencial)");
                
                if (string.IsNullOrWhiteSpace(user.EnderecoNumero))
                    response.CamposRepresentanteFaltando.Add("Número (endereço residencial)");
                
                if (string.IsNullOrWhiteSpace(user.EnderecoCidade))
                    response.CamposRepresentanteFaltando.Add("Cidade (endereço residencial)");
                
                if (string.IsNullOrWhiteSpace(user.EnderecoEstado))
                    response.CamposRepresentanteFaltando.Add("Estado (endereço residencial)");
                
                if (string.IsNullOrWhiteSpace(user.EnderecoBairro))
                    response.CamposRepresentanteFaltando.Add("Bairro (endereço residencial)");
                
                if (string.IsNullOrWhiteSpace(user.EnderecoCep))
                    response.CamposRepresentanteFaltando.Add("CEP (endereço residencial)");

                response.EmpresaCompleta = response.CamposEmpresaFaltando.Count == 0;
                response.PerfilCompleto = response.CamposRepresentanteFaltando.Count == 0;
                response.PodeGerarContrato = response.EmpresaCompleta && response.PerfilCompleto;

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar dados do contrato para usuário {UserId}", userId);
                return Result.Failure<ValidacaoContratoResponse>("Erro ao validar dados");
            }
        }

        private string FormatCnpj(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14) return cnpj;
            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        private Result<bool> ValidarDadosEmpresaContratante(Company company)
        {
            var camposFaltando = new List<string>();

            if (string.IsNullOrWhiteSpace(company.AddressStreet))
                camposFaltando.Add("Rua");
            if (string.IsNullOrWhiteSpace(company.AddressNumber))
                camposFaltando.Add("Número");
            if (string.IsNullOrWhiteSpace(company.AddressNeighborhood))
                camposFaltando.Add("Bairro");
            if (string.IsNullOrWhiteSpace(company.AddressCity))
                camposFaltando.Add("Cidade");
            if (string.IsNullOrWhiteSpace(company.AddressState))
                camposFaltando.Add("Estado");
            if (string.IsNullOrWhiteSpace(company.AddressZipCode))
                camposFaltando.Add("CEP");

            if (camposFaltando.Any())
            {
                var mensagem = $"Dados da empresa contratante estão incompletos. Campos faltando: {string.Join(", ", camposFaltando)}";
                return Result.Failure<bool>(mensagem);
            }

            return Result.Success(true);
        }

        private Result<bool> ValidarDadosRepresentante(User user)
        {
            var camposFaltando = new List<string>();

            if (string.IsNullOrWhiteSpace(user.CPFEncrypted))
                camposFaltando.Add("CPF");
            if (string.IsNullOrWhiteSpace(user.RGEncrypted))
                camposFaltando.Add("RG");
            if (!user.DataNascimento.HasValue)
                camposFaltando.Add("Data de Nascimento");
            if (string.IsNullOrWhiteSpace(user.Nacionalidade))
                camposFaltando.Add("Nacionalidade");
            if (string.IsNullOrWhiteSpace(user.EstadoCivil))
                camposFaltando.Add("Estado Civil");
            if (string.IsNullOrWhiteSpace(user.EnderecoRua))
                camposFaltando.Add("Rua (endereço residencial)");
            if (string.IsNullOrWhiteSpace(user.EnderecoNumero))
                camposFaltando.Add("Número (endereço residencial)");
            if (string.IsNullOrWhiteSpace(user.EnderecoBairro))
                camposFaltando.Add("Bairro (endereço residencial)");
            if (string.IsNullOrWhiteSpace(user.EnderecoCidade))
                camposFaltando.Add("Cidade (endereço residencial)");
            if (string.IsNullOrWhiteSpace(user.EnderecoEstado))
                camposFaltando.Add("Estado (endereço residencial)");
            if (string.IsNullOrWhiteSpace(user.EnderecoCep))
                camposFaltando.Add("CEP (endereço residencial)");

            if (camposFaltando.Any())
            {
                var mensagem = $"Dados do representante estão incompletos. Campos faltando: {string.Join(", ", camposFaltando)}";
                return Result.Failure<bool>(mensagem);
            }

            return Result.Success(true);
        }

        private Result<bool> ValidarDadosContratadoPJ(User funcionarioPJ)
        {
            var camposFaltando = new List<string>();

            if (string.IsNullOrWhiteSpace(funcionarioPJ.CPFEncrypted))
                camposFaltando.Add("CPF");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.RGEncrypted))
                camposFaltando.Add("RG");
            if (!funcionarioPJ.DataNascimento.HasValue)
                camposFaltando.Add("Data de Nascimento");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.Nacionalidade))
                camposFaltando.Add("Nacionalidade");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EstadoCivil))
                camposFaltando.Add("Estado Civil");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.Cargo))
                camposFaltando.Add("Profissão");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoRua))
                camposFaltando.Add("Rua (endereço residencial)");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoNumero))
                camposFaltando.Add("Número (endereço residencial)");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoBairro))
                camposFaltando.Add("Bairro (endereço residencial)");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoCidade))
                camposFaltando.Add("Cidade (endereço residencial)");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoEstado))
                camposFaltando.Add("Estado (endereço residencial)");
            if (string.IsNullOrWhiteSpace(funcionarioPJ.EnderecoCep))
                camposFaltando.Add("CEP (endereço residencial)");

            if (camposFaltando.Any())
            {
                var mensagem = $"Dados do contratado (funcionário PJ) estão incompletos. Campos faltando: {string.Join(", ", camposFaltando)}";
                return Result.Failure<bool>(mensagem);
            }

            return Result.Success(true);
        }

        private Result<bool> ValidarDadosContratadoManual(DadosContratadoManualRequest dadosManual)
        {
            var camposFaltando = new List<string>();

            if (string.IsNullOrWhiteSpace(dadosManual.NomeCompleto))
                camposFaltando.Add("Nome Completo");
            if (string.IsNullOrWhiteSpace(dadosManual.RazaoSocial))
                camposFaltando.Add("Razão Social");
            if (string.IsNullOrWhiteSpace(dadosManual.Cnpj))
                camposFaltando.Add("CNPJ");
            if (string.IsNullOrWhiteSpace(dadosManual.Cpf))
                camposFaltando.Add("CPF");
            if (string.IsNullOrWhiteSpace(dadosManual.Rg))
                camposFaltando.Add("RG");
            if (!dadosManual.DataNascimento.HasValue)
                camposFaltando.Add("Data de Nascimento");
            if (string.IsNullOrWhiteSpace(dadosManual.Nacionalidade))
                camposFaltando.Add("Nacionalidade");
            if (string.IsNullOrWhiteSpace(dadosManual.EstadoCivil))
                camposFaltando.Add("Estado Civil");
            if (string.IsNullOrWhiteSpace(dadosManual.Profissao))
                camposFaltando.Add("Profissão");
            if (string.IsNullOrWhiteSpace(dadosManual.Email))
                camposFaltando.Add("Email");
            if (string.IsNullOrWhiteSpace(dadosManual.TelefoneCelular))
                camposFaltando.Add("Telefone Celular");
            if (string.IsNullOrWhiteSpace(dadosManual.Rua))
                camposFaltando.Add("Rua");
            if (string.IsNullOrWhiteSpace(dadosManual.Numero))
                camposFaltando.Add("Número");
            if (string.IsNullOrWhiteSpace(dadosManual.Bairro))
                camposFaltando.Add("Bairro");
            if (string.IsNullOrWhiteSpace(dadosManual.Cidade))
                camposFaltando.Add("Cidade");
            if (string.IsNullOrWhiteSpace(dadosManual.Estado))
                camposFaltando.Add("Estado");
            if (string.IsNullOrWhiteSpace(dadosManual.Pais))
                camposFaltando.Add("País");
            if (string.IsNullOrWhiteSpace(dadosManual.Cep))
                camposFaltando.Add("CEP");

            if (camposFaltando.Any())
            {
                var mensagem = $"Dados do contratado estão incompletos. Campos faltando: {string.Join(", ", camposFaltando)}";
                return Result.Failure<bool>(mensagem);
            }

            return Result.Success(true);
        }

        private Result<bool> ValidarDadosEmpresaPJ(Company empresaPJ)
        {
            var camposFaltando = new List<string>();

            if (string.IsNullOrWhiteSpace(empresaPJ.AddressStreet))
                camposFaltando.Add("Rua (empresa PJ)");
            if (string.IsNullOrWhiteSpace(empresaPJ.AddressNumber))
                camposFaltando.Add("Número (empresa PJ)");
            if (string.IsNullOrWhiteSpace(empresaPJ.AddressNeighborhood))
                camposFaltando.Add("Bairro (empresa PJ)");
            if (string.IsNullOrWhiteSpace(empresaPJ.AddressCity))
                camposFaltando.Add("Cidade (empresa PJ)");
            if (string.IsNullOrWhiteSpace(empresaPJ.AddressState))
                camposFaltando.Add("Estado (empresa PJ)");

            if (camposFaltando.Any())
            {
                var mensagem = $"Dados da empresa do contratado (PJ) estão incompletos. Campos faltando: {string.Join(", ", camposFaltando)}";
                return Result.Failure<bool>(mensagem);
            }

            return Result.Success(true);
        }

        private string FormatCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length != 11) return cpf;
            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        private string FormatCep(string cep)
        {
            if (string.IsNullOrEmpty(cep) || cep.Length != 8) return cep;
            return $"{cep.Substring(0, 5)}-{cep.Substring(5, 3)}";
        }

        private string NumeroPorExtenso(int numero)
        {
            var unidades = new[] { "", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove" };
            var dezenas = new[] { "", "dez", "vinte", "trinta", "quarenta", "cinquenta", "sessenta", "setenta", "oitenta", "noventa" };
            var especiais = new[] { "dez", "onze", "doze", "treze", "quatorze", "quinze", "dezesseis", "dezessete", "dezoito", "dezenove" };

            if (numero < 10) return unidades[numero];
            if (numero >= 10 && numero < 20) return especiais[numero - 10];
            if (numero >= 20 && numero < 100)
            {
                var dezena = numero / 10;
                var unidade = numero % 10;
                return unidade == 0 ? dezenas[dezena] : $"{dezenas[dezena]} e {unidades[unidade]}";
            }
            return numero.ToString();
        }

        private string ValorPorExtenso(decimal valor)
        {
            return $"{valor:N2} reais";
        }
    }
}
