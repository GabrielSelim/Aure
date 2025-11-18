using Aure.Application.DTOs.Contract;
using Aure.Application.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Aure.Domain.Exceptions;
using Aure.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aure.Application.Services;

public class ContractTemplateService : IContractTemplateService
{
    private readonly IContractTemplateRepository _templateRepository;
    private readonly IContractDocumentRepository _documentRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<ContractTemplateService> _logger;

    public ContractTemplateService(
        IContractTemplateRepository templateRepository,
        IContractDocumentRepository documentRepository,
        IContractRepository contractRepository,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ILogger<ContractTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _documentRepository = documentRepository;
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<ContractTemplateResponse> CreateTemplateAsync(
        CreateContractTemplateRequest request, 
        Guid companyId, 
        Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (user.Role != UserRole.DonoEmpresaPai && user.Role != UserRole.Juridico))
        {
            throw new UnauthorizedException("Apenas Dono da Empresa ou Jurídico podem criar templates");
        }

        if (request.DefinirComoPadrao)
        {
            var existePadrao = await _templateRepository.ExisteTemplatePadraoAsync(companyId, request.Tipo);
            if (existePadrao)
            {
                throw new BusinessException($"Já existe um template padrão para o tipo {request.Tipo}");
            }
        }

        var template = new ContractTemplate(
            request.Nome,
            request.Descricao,
            request.Tipo,
            request.ConteudoHtml,
            companyId,
            request.VariaveisDisponiveis
        );

        if (request.DefinirComoPadrao)
        {
            template.DefinirComoPadrao();
        }

        if (!string.IsNullOrEmpty(request.ConteudoDocxBase64))
        {
            template.DefinirComoTemplateDocx(request.ConteudoDocxBase64);
        }

        await _templateRepository.AddAsync(template);

        _logger.LogInformation(
            "Template {TemplateNome} criado por {UserName} para empresa {CompanyId}",
            template.Nome, user.Name, companyId);

        return MapToResponse(template);
    }

    public async Task<ContractTemplateResponse> UpdateTemplateAsync(
        Guid templateId, 
        UpdateContractTemplateRequest request, 
        Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (user.Role != UserRole.DonoEmpresaPai && user.Role != UserRole.Juridico))
        {
            throw new UnauthorizedException("Apenas Dono da Empresa ou Jurídico podem atualizar templates");
        }

        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
        {
            throw new NotFoundException("Template não encontrado");
        }

        if (template.CompanyId != user.CompanyId)
        {
            throw new UnauthorizedException("Template não pertence à sua empresa");
        }

        template.AtualizarInformacoes(request.Nome, request.Descricao);
        template.AtualizarConteudo(request.ConteudoHtml, request.VariaveisDisponiveis);

        if (!string.IsNullOrEmpty(request.ConteudoDocxBase64))
        {
            template.DefinirComoTemplateDocx(request.ConteudoDocxBase64);
        }

        await _templateRepository.UpdateAsync(template);

        _logger.LogInformation(
            "Template {TemplateNome} atualizado por {UserName}",
            template.Nome, user.Name);

        return MapToResponse(template);
    }

    public async Task<ContractTemplateResponse> GetTemplateByIdAsync(Guid templateId, Guid companyId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
        {
            throw new NotFoundException("Template não encontrado");
        }

        return MapToResponse(template);
    }

    public async Task<List<ContractTemplateListResponse>> GetAllTemplatesAsync(Guid companyId, bool apenasAtivos = true)
    {
        var templates = await _templateRepository.GetAllByCompanyIdAsync(companyId, apenasAtivos);

        return templates.Select(t => new ContractTemplateListResponse
        {
            Id = t.Id,
            Nome = t.Nome,
            Descricao = t.Descricao,
            Tipo = t.Tipo.ToString(),
            EhPadrao = t.EhPadrao,
            Ativo = t.Ativo,
            QuantidadeVariaveis = t.VariaveisDisponiveis.Count,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<ContractTemplateResponse> GetTemplatePadraoAsync(Guid companyId, string tipo)
    {
        if (!Enum.TryParse<ContractTemplateType>(tipo, out var tipoEnum))
        {
            throw new BusinessException("Tipo de contrato inválido");
        }

        var template = await _templateRepository.GetTemplatePadraoAsync(companyId, tipoEnum);
        if (template == null)
        {
            throw new NotFoundException($"Nenhum template padrão encontrado para o tipo {tipo}");
        }

        return MapToResponse(template);
    }

    public async Task DefinirComoPadraoAsync(Guid templateId, Guid companyId, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.DonoEmpresaPai)
        {
            throw new UnauthorizedException("Apenas o Dono da Empresa pode definir templates padrão");
        }

        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
        {
            throw new NotFoundException("Template não encontrado");
        }

        var existePadrao = await _templateRepository.ExisteTemplatePadraoAsync(companyId, template.Tipo, templateId);
        if (existePadrao)
        {
            throw new BusinessException($"Já existe um template padrão para o tipo {template.Tipo}. Remova o padrão atual primeiro.");
        }

        template.DefinirComoPadrao();
        await _templateRepository.UpdateAsync(template);

        _logger.LogInformation(
            "Template {TemplateNome} definido como padrão por {UserName}",
            template.Nome, user.Name);
    }

    public async Task RemoverPadraoAsync(Guid templateId, Guid companyId, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.DonoEmpresaPai)
        {
            throw new UnauthorizedException("Apenas o Dono da Empresa pode remover templates padrão");
        }

        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
        {
            throw new NotFoundException("Template não encontrado");
        }

        template.RemoverPadrao();
        await _templateRepository.UpdateAsync(template);

        _logger.LogInformation(
            "Template {TemplateNome} removido como padrão por {UserName}",
            template.Nome, user.Name);
    }

    public async Task AtivarTemplateAsync(Guid templateId, Guid companyId, Guid userId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
        {
            throw new NotFoundException("Template não encontrado");
        }

        template.Ativar();
        await _templateRepository.UpdateAsync(template);
    }

    public async Task DesativarTemplateAsync(Guid templateId, string motivo, Guid companyId, Guid userId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null || template.CompanyId != companyId)
        {
            throw new NotFoundException("Template não encontrado");
        }

        template.Desativar(motivo);
        await _templateRepository.UpdateAsync(template);
    }

    public Task<AvailableVariablesResponse> GetVariaveisDisponiveisAsync()
    {
        var variaveis = new List<VariableInfo>
        {
            new() { Nome = "{{NOME_CONTRATANTE}}", Descricao = "Nome da empresa contratante", Exemplo = "Tech Solutions LTDA", Categoria = "Empresa" },
            new() { Nome = "{{CNPJ_CONTRATANTE}}", Descricao = "CNPJ da empresa contratante", Exemplo = "12.345.678/0001-99", Categoria = "Empresa" },
            new() { Nome = "{{ENDERECO_CONTRATANTE}}", Descricao = "Endereço completo da empresa", Exemplo = "Rua das Flores, 123 - São Paulo/SP", Categoria = "Empresa" },
            new() { Nome = "{{NOME_CONTRATADO}}", Descricao = "Nome completo do prestador", Exemplo = "João Silva Santos", Categoria = "Prestador" },
            new() { Nome = "{{CPF_CONTRATADO}}", Descricao = "CPF do prestador", Exemplo = "123.456.789-00", Categoria = "Prestador" },
            new() { Nome = "{{RG_CONTRATADO}}", Descricao = "RG do prestador", Exemplo = "12.345.678-9", Categoria = "Prestador" },
            new() { Nome = "{{ENDERECO_CONTRATADO}}", Descricao = "Endereço completo do prestador", Exemplo = "Av. Paulista, 1000 - São Paulo/SP", Categoria = "Prestador" },
            new() { Nome = "{{CNPJ_CONTRATADO}}", Descricao = "CNPJ da empresa do prestador PJ", Exemplo = "98.765.432/0001-11", Categoria = "Prestador" },
            new() { Nome = "{{RAZAO_SOCIAL_CONTRATADO}}", Descricao = "Razão social da empresa PJ", Exemplo = "Silva Consultoria LTDA", Categoria = "Prestador" },
            new() { Nome = "{{CARGO}}", Descricao = "Cargo ou função do prestador", Exemplo = "Desenvolvedor Full Stack", Categoria = "Contrato" },
            new() { Nome = "{{VALOR_MENSAL}}", Descricao = "Valor mensal do contrato", Exemplo = "R$ 8.000,00", Categoria = "Contrato" },
            new() { Nome = "{{VALOR_EXTENSO}}", Descricao = "Valor por extenso", Exemplo = "oito mil reais", Categoria = "Contrato" },
            new() { Nome = "{{DATA_INICIO}}", Descricao = "Data de início do contrato", Exemplo = "01/01/2025", Categoria = "Contrato" },
            new() { Nome = "{{DATA_FIM}}", Descricao = "Data de término do contrato", Exemplo = "31/12/2025", Categoria = "Contrato" },
            new() { Nome = "{{DURACAO_MESES}}", Descricao = "Duração em meses", Exemplo = "12 meses", Categoria = "Contrato" },
            new() { Nome = "{{DATA_ATUAL}}", Descricao = "Data atual de geração", Exemplo = "02/11/2025", Categoria = "Sistema" },
            new() { Nome = "{{NUMERO_CONTRATO}}", Descricao = "Número único do contrato", Exemplo = "CTR-2025-001", Categoria = "Sistema" },
            new() { Nome = "{{DESCRICAO_SERVICOS}}", Descricao = "Descrição detalhada dos serviços", Exemplo = "Desenvolvimento de software...", Categoria = "Contrato" },
            new() { Nome = "{{DIAS_PAGAMENTO}}", Descricao = "Dia(s) de pagamento", Exemplo = "Todo dia 5 de cada mês", Categoria = "Contrato" },
            new() { Nome = "{{FORMA_PAGAMENTO}}", Descricao = "Forma de pagamento", Exemplo = "Transferência bancária", Categoria = "Contrato" }
        };

        return Task.FromResult(new AvailableVariablesResponse { Variaveis = variaveis });
    }

    public async Task<ContractDocumentResponse> GerarContratoDeTemplateAsync(
        GenerateContractFromTemplateRequest request, 
        Guid userId)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId);
        if (template == null)
        {
            throw new NotFoundException("Template não encontrado");
        }

        var contract = await _contractRepository.GetByIdAsync(request.ContractId);
        if (contract == null)
        {
            throw new NotFoundException("Contrato não encontrado");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId))
        {
            throw new UnauthorizedException("Você não tem permissão para gerar este contrato");
        }

        var conteudoPreenchido = PreencherVariaveis(template.ConteudoHtml, request.DadosPreenchimento);

        var document = new ContractDocument(
            request.ContractId,
            request.TemplateId,
            conteudoPreenchido,
            request.DadosPreenchimento,
            userId
        );

        if (request.GerarPdf)
        {
        }

        await _documentRepository.AddAsync(document);

        _logger.LogInformation(
            "Documento gerado para contrato {ContractId} usando template {TemplateName}",
            contract.Id, template.Nome);

        return MapToDocumentResponse(document, template.Nome);
    }

    public async Task<ContractDocumentResponse> UploadContratoPersonalizadoAsync(
        UploadCustomContractRequest request, 
        Guid userId)
    {
        var contract = await _contractRepository.GetByIdAsync(request.ContractId);
        if (contract == null)
        {
            throw new NotFoundException("Contrato não encontrado");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || (contract.ClientId != user.CompanyId && contract.ProviderId != user.CompanyId))
        {
            throw new UnauthorizedException("Você não tem permissão para fazer upload neste contrato");
        }

        if (user.Role != UserRole.DonoEmpresaPai && user.Role != UserRole.Juridico)
        {
            throw new UnauthorizedException("Apenas Dono da Empresa ou Jurídico podem fazer upload de contratos personalizados");
        }

        var document = new ContractDocument(
            request.ContractId,
            null,
            request.ConteudoHtml,
            request.DadosPreenchidos,
            userId
        );

        if (!string.IsNullOrEmpty(request.ConteudoPdfBase64))
        {
            document.GerarPdf(request.ConteudoPdfBase64);
        }

        if (!string.IsNullOrEmpty(request.ConteudoDocxBase64))
        {
            document.GerarDocx(request.ConteudoDocxBase64);
        }

        await _documentRepository.AddAsync(document);

        _logger.LogInformation(
            "Contrato personalizado enviado para contrato {ContractId} por {UserName}",
            contract.Id, user.Name);

        return MapToDocumentResponse(document, "Contrato Personalizado");
    }

    public async Task<List<ContractDocumentResponse>> GetDocumentosByContractIdAsync(Guid contractId, Guid companyId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null || (contract.ClientId != companyId && contract.ProviderId != companyId))
        {
            throw new NotFoundException("Contrato não encontrado");
        }

        var documents = await _documentRepository.GetByContractIdAsync(contractId);

        return documents.Select(d => MapToDocumentResponse(d, d.Template?.Nome)).ToList();
    }

    public async Task<ContractDocumentResponse> GetVersaoFinalAsync(Guid contractId, Guid companyId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null || (contract.ClientId != companyId && contract.ProviderId != companyId))
        {
            throw new NotFoundException("Contrato não encontrado");
        }

        var document = await _documentRepository.GetVersaoFinalAsync(contractId);
        if (document == null)
        {
            throw new NotFoundException("Nenhuma versão final encontrada para este contrato");
        }

        return MapToDocumentResponse(document, document.Template?.Nome);
    }

    public async Task DefinirComoVersaoFinalAsync(Guid documentId, Guid companyId, Guid userId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new NotFoundException("Documento não encontrado");
        }

        var contract = await _contractRepository.GetByIdAsync(document.ContractId);
        if (contract == null || (contract.ClientId != companyId && contract.ProviderId != companyId))
        {
            throw new UnauthorizedException("Você não tem permissão para esta ação");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Role != UserRole.DonoEmpresaPai)
        {
            throw new UnauthorizedException("Apenas o Dono da Empresa pode definir a versão final");
        }

        document.DefinirComoVersaoFinal();
        await _documentRepository.UpdateAsync(document);

        _logger.LogInformation(
            "Documento {DocumentId} definido como versão final por {UserName}",
            documentId, user.Name);
    }

    private string PreencherVariaveis(string template, Dictionary<string, string> dados)
    {
        var resultado = template;

        foreach (var (chave, valor) in dados)
        {
            var placeholder = chave.StartsWith("{{") ? chave : $"{{{{{chave}}}}}";
            resultado = resultado.Replace(placeholder, valor ?? string.Empty);
        }

        return resultado;
    }

    private ContractTemplateResponse MapToResponse(ContractTemplate template)
    {
        return new ContractTemplateResponse
        {
            Id = template.Id,
            Nome = template.Nome,
            Descricao = template.Descricao,
            Tipo = template.Tipo.ToString(),
            ConteudoHtml = template.ConteudoHtml,
            TemDocx = !string.IsNullOrEmpty(template.ConteudoDocx),
            EhPadrao = template.EhPadrao,
            Ativo = template.Ativo,
            VariaveisDisponiveis = template.VariaveisDisponiveis,
            CreatedAt = template.CreatedAt,
            DataDesativacao = template.DataDesativacao,
            MotivoDesativacao = template.MotivoDesativacao
        };
    }

    private ContractDocumentResponse MapToDocumentResponse(ContractDocument document, string? nomeTemplate)
    {
        return new ContractDocumentResponse
        {
            Id = document.Id,
            ContractId = document.ContractId,
            TemplateId = document.TemplateId,
            NomeTemplate = nomeTemplate,
            ConteudoHtml = document.ConteudoHtml,
            ConteudoPdfBase64 = document.ConteudoPdf,
            ConteudoDocxBase64 = document.ConteudoDocx,
            Versao = document.GetVersao(),
            DataGeracao = document.DataGeracao,
            GeradoPorUsuarioNome = document.GeradoPorUsuario?.Name,
            DadosPreenchidos = document.DadosPreenchidos,
            EhVersaoFinal = document.EhVersaoFinal,
            HashDocumento = document.HashDocumento
        };
    }
}
