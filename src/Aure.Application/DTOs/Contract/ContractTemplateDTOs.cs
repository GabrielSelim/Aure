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
