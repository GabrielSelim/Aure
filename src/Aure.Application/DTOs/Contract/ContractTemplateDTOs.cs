using System.ComponentModel.DataAnnotations;
using Aure.Domain.Entities;

namespace Aure.Application.DTOs.Contract;

public class CreateContractTemplateRequest
{
    [Required(ErrorMessage = "Nome do template é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string Descricao { get; set; }

    [Required(ErrorMessage = "Tipo do contrato é obrigatório")]
    public ContractTemplateType Tipo { get; set; }

    [Required(ErrorMessage = "Conteúdo HTML é obrigatório")]
    public string ConteudoHtml { get; set; }

    public string? ConteudoDocxBase64 { get; set; }

    public List<string> VariaveisDisponiveis { get; set; } = new();

    public bool DefinirComoPadrao { get; set; }
}

public class UpdateContractTemplateRequest
{
    [Required(ErrorMessage = "Nome do template é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string Descricao { get; set; }

    [Required(ErrorMessage = "Conteúdo HTML é obrigatório")]
    public string ConteudoHtml { get; set; }

    public string? ConteudoDocxBase64 { get; set; }

    public List<string> VariaveisDisponiveis { get; set; } = new();
}

public class ContractTemplateResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Tipo { get; set; }
    public string ConteudoHtml { get; set; }
    public bool TemDocx { get; set; }
    public bool EhPadrao { get; set; }
    public bool Ativo { get; set; }
    public bool EhSistema { get; set; }
    public bool PodeEditar { get; set; }
    public bool PodeDeletar { get; set; }
    public List<string> VariaveisDisponiveis { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DataDesativacao { get; set; }
    public string? MotivoDesativacao { get; set; }
}

public class ContractTemplateListResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Tipo { get; set; }
    public bool EhPadrao { get; set; }
    public bool Ativo { get; set; }
    public bool EhSistema { get; set; }
    public int QuantidadeVariaveis { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GenerateContractFromTemplateRequest
{
    [Required(ErrorMessage = "ID do template é obrigatório")]
    public Guid TemplateId { get; set; }

    [Required(ErrorMessage = "ID do contrato é obrigatório")]
    public Guid ContractId { get; set; }

    [Required(ErrorMessage = "Dados para preenchimento são obrigatórios")]
    public Dictionary<string, string> DadosPreenchimento { get; set; } = new();

    public bool GerarPdf { get; set; } = true;
    public bool GerarDocx { get; set; } = false;
}

public class UploadCustomContractRequest
{
    [Required(ErrorMessage = "ID do contrato é obrigatório")]
    public Guid ContractId { get; set; }

    [Required(ErrorMessage = "Conteúdo HTML é obrigatório")]
    public string ConteudoHtml { get; set; }

    public string? ConteudoPdfBase64 { get; set; }
    public string? ConteudoDocxBase64 { get; set; }

    [Required(ErrorMessage = "Dados preenchidos são obrigatórios")]
    public Dictionary<string, string> DadosPreenchidos { get; set; } = new();
}

public class ContractDocumentResponse
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? NomeTemplate { get; set; }
    public string ConteudoHtml { get; set; }
    public string? ConteudoPdfBase64 { get; set; }
    public string? ConteudoDocxBase64 { get; set; }
    public string Versao { get; set; }
    public DateTime DataGeracao { get; set; }
    public string? GeradoPorUsuarioNome { get; set; }
    public Dictionary<string, string> DadosPreenchidos { get; set; }
    public bool EhVersaoFinal { get; set; }
    public string? HashDocumento { get; set; }
}

public class AvailableVariablesResponse
{
    public List<VariableInfo> Variaveis { get; set; } = new();
}

public class VariableInfo
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Exemplo { get; set; }
    public string Categoria { get; set; }
}

public class ContractTemplateConfigRequest
{
    [Required(ErrorMessage = "Título do serviço é obrigatório")]
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string TituloServico { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição do serviço é obrigatória")]
    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
    public string DescricaoServico { get; set; } = string.Empty;

    [Required(ErrorMessage = "Local de prestação é obrigatório")]
    [StringLength(500, ErrorMessage = "Local deve ter no máximo 500 caracteres")]
    public string LocalPrestacaoServico { get; set; } = string.Empty;

    [Required(ErrorMessage = "Detalhamento dos serviços é obrigatório")]
    public List<string> DetalhamentoServicos { get; set; } = new();

    public string? ClausulaAjudaCusto { get; set; }

    [Required(ErrorMessage = "Obrigações do contratado são obrigatórias")]
    public List<string> ObrigacoesContratado { get; set; } = new();

    [Required(ErrorMessage = "Obrigações da contratante são obrigatórias")]
    public List<string> ObrigacoesContratante { get; set; } = new();
}

public class ContractTemplateConfigResponse
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string NomeEmpresa { get; set; } = string.Empty;
    public string TituloServico { get; set; } = string.Empty;
    public string DescricaoServico { get; set; } = string.Empty;
    public string LocalPrestacaoServico { get; set; } = string.Empty;
    public List<string> DetalhamentoServicos { get; set; } = new();
    public string? ClausulaAjudaCusto { get; set; }
    public List<string> ObrigacoesContratado { get; set; } = new();
    public List<string> ObrigacoesContratante { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PreviewTemplateRequest
{
    [Required(ErrorMessage = "ID do funcionário PJ é obrigatório")]
    public Guid FuncionarioPJId { get; set; }

    [Required(ErrorMessage = "Configuração de template é obrigatória")]
    public ContractTemplateConfigRequest TemplateConfig { get; set; } = new();

    [Required(ErrorMessage = "Valor mensal é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor mensal deve ser maior que zero")]
    public decimal ValorMensal { get; set; }

    [Required(ErrorMessage = "Prazo de vigência é obrigatório")]
    [Range(1, 120, ErrorMessage = "Prazo deve ser entre 1 e 120 meses")]
    public int PrazoVigenciaMeses { get; set; }

    [Required(ErrorMessage = "Dia de vencimento da NF é obrigatório")]
    [Range(1, 31, ErrorMessage = "Dia deve ser entre 1 e 31")]
    public int DiaVencimentoNF { get; set; }

    [Required(ErrorMessage = "Dia de pagamento é obrigatório")]
    [Range(1, 31, ErrorMessage = "Dia deve ser entre 1 e 31")]
    public int DiaPagamento { get; set; }
}

public class ContractTemplatePresetResponse
{
    public string Tipo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public ContractTemplateConfigRequest Configuracao { get; set; } = new();
}
