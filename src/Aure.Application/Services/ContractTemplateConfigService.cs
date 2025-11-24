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
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return Result.Failure<List<ContractTemplateConfigResponse>>("Usuário não encontrado");

                if (!user.CompanyId.HasValue)
                    return Result.Failure<List<ContractTemplateConfigResponse>>("Usuário não pertence a uma empresa");

                var configs = await _unitOfWork.ContractTemplateConfigs.GetAllByCompanyIdAsync(user.CompanyId.Value);
                
                var responses = configs.Select(config => new ContractTemplateConfigResponse
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    NomeEmpresa = config.Company.Name,
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

                return Result.Success(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configurações da empresa para usuário {UserId}", userId);
                return Result.Failure<List<ContractTemplateConfigResponse>>("Erro ao buscar configurações");
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

                var response = new ContractTemplateConfigResponse
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    NomeEmpresa = config.Company.Name,
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

                var funcionarioPJ = await _unitOfWork.Users.GetByIdAsync(request.FuncionarioPJId);
                if (funcionarioPJ == null)
                    return Result.Failure<string>("Funcionário PJ não encontrado");

                var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
                if (company == null)
                    return Result.Failure<string>("Empresa não encontrada");

                var empresaPJ = funcionarioPJ.CompanyId.HasValue 
                    ? await _unitOfWork.Companies.GetByIdAsync(funcionarioPJ.CompanyId.Value)
                    : null;

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

                var cpfContratado = string.Empty;
                if (!string.IsNullOrEmpty(funcionarioPJ.CPFEncrypted))
                {
                    cpfContratado = _encryptionService.Decrypt(funcionarioPJ.CPFEncrypted);
                }

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
                html = html.Replace("{{NIRE_CONTRATANTE}}", "");

                html = html.Replace("{{NOME_REPRESENTANTE_CONTRATANTE}}", user.Name);
                html = html.Replace("{{NACIONALIDADE_REPRESENTANTE}}", "Brasileiro(a)");
                html = html.Replace("{{ESTADO_CIVIL_REPRESENTANTE}}", "");
                html = html.Replace("{{DATA_NASCIMENTO_REPRESENTANTE}}", user.DataNascimento?.ToString("dd/MM/yyyy") ?? "");
                html = html.Replace("{{PROFISSAO_REPRESENTANTE}}", user.Cargo ?? "Empresário");
                html = html.Replace("{{CPF_REPRESENTANTE}}", user.CPFEncrypted != null ? FormatCpf(_encryptionService.Decrypt(user.CPFEncrypted)) : "");
                html = html.Replace("{{RG_REPRESENTANTE}}", user.RGEncrypted != null ? _encryptionService.Decrypt(user.RGEncrypted) : "");
                html = html.Replace("{{ORGAO_EXPEDIDOR_REPRESENTANTE}}", "SSP");
                html = html.Replace("{{ENDERECO_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoRua ?? "");
                html = html.Replace("{{NUMERO_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoNumero ?? "");
                html = html.Replace("{{BAIRRO_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoBairro ?? "");
                html = html.Replace("{{CIDADE_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoCidade ?? "");
                html = html.Replace("{{UF_RESIDENCIAL_REPRESENTANTE}}", user.EnderecoEstado ?? "");
                html = html.Replace("{{CEP_RESIDENCIAL_REPRESENTANTE}}", FormatCep(user.EnderecoCep ?? ""));

                html = html.Replace("{{RAZAO_SOCIAL_CONTRATADO}}", empresaPJ?.Name ?? funcionarioPJ.Name);
                html = html.Replace("{{CNPJ_CONTRATADO}}", empresaPJ != null ? FormatCnpj(empresaPJ.Cnpj) : "");
                html = html.Replace("{{ENDERECO_CONTRATADO}}", empresaPJ?.AddressStreet ?? funcionarioPJ.EnderecoRua ?? "");
                html = html.Replace("{{NUMERO_CONTRATADO}}", empresaPJ?.AddressNumber ?? funcionarioPJ.EnderecoNumero ?? "");
                html = html.Replace("{{BAIRRO_CONTRATADO}}", empresaPJ?.AddressNeighborhood ?? funcionarioPJ.EnderecoBairro ?? "");
                html = html.Replace("{{CIDADE_CONTRATADO}}", empresaPJ?.AddressCity ?? funcionarioPJ.EnderecoCidade ?? "");
                html = html.Replace("{{ESTADO_CONTRATADO}}", empresaPJ?.AddressState ?? funcionarioPJ.EnderecoEstado ?? "");

                html = html.Replace("{{NOME_CONTRATADO}}", funcionarioPJ.Name);
                html = html.Replace("{{NACIONALIDADE_CONTRATADO}}", "Brasileiro(a)");
                html = html.Replace("{{ESTADO_CIVIL_CONTRATADO}}", "");
                html = html.Replace("{{DATA_NASCIMENTO_CONTRATADO}}", funcionarioPJ.DataNascimento?.ToString("dd/MM/yyyy") ?? "");
                html = html.Replace("{{PROFISSAO_CONTRATADO}}", funcionarioPJ.Cargo ?? "Prestador de Serviços");
                html = html.Replace("{{CPF_CONTRATADO}}", FormatCpf(cpfContratado));
                html = html.Replace("{{ENDERECO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoRua ?? "");
                html = html.Replace("{{NUMERO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoNumero ?? "");
                html = html.Replace("{{BAIRRO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoBairro ?? "");
                html = html.Replace("{{CIDADE_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoCidade ?? "");
                html = html.Replace("{{ESTADO_RESIDENCIAL_CONTRATADO}}", funcionarioPJ.EnderecoEstado ?? "");
                html = html.Replace("{{CEP_RESIDENCIAL_CONTRATADO}}", FormatCep(funcionarioPJ.EnderecoCep ?? ""));

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

                return Result.Success(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar preview de contrato");
                return Result.Failure<string>($"Erro ao gerar preview: {ex.Message}");
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

        private string FormatCnpj(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14) return cnpj;
            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
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
