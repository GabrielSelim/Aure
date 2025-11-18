using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class ContractTemplate : BaseEntity
{
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public ContractTemplateType Tipo { get; private set; }
    public string ConteudoHtml { get; private set; }
    public string? ConteudoDocx { get; private set; }
    public bool EhPadrao { get; private set; }
    public bool Ativo { get; private set; }
    public bool EhSistema { get; private set; }
    public Guid CompanyId { get; private set; }
    public Company Company { get; private set; }
    public List<string> VariaveisDisponiveis { get; private set; }
    public DateTime? DataDesativacao { get; private set; }
    public string? MotivoDesativacao { get; private set; }

    private ContractTemplate() 
    {
        VariaveisDisponiveis = new List<string>();
    }

    public ContractTemplate(
        string nome,
        string descricao,
        ContractTemplateType tipo,
        string conteudoHtml,
        Guid companyId,
        List<string> variaveisDisponiveis)
    {
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
        Tipo = tipo;
        ConteudoHtml = conteudoHtml ?? throw new ArgumentNullException(nameof(conteudoHtml));
        CompanyId = companyId;
        VariaveisDisponiveis = variaveisDisponiveis ?? new List<string>();
        Ativo = true;
        EhPadrao = false;
        EhSistema = false;
    }

    public void AtualizarConteudo(string conteudoHtml, List<string> variaveisDisponiveis)
    {
        if (EhSistema)
            throw new InvalidOperationException("Templates do sistema não podem ser editados");
        
        ConteudoHtml = conteudoHtml ?? throw new ArgumentNullException(nameof(conteudoHtml));
        VariaveisDisponiveis = variaveisDisponiveis ?? new List<string>();
    }

    public void DefinirComoTemplateDocx(string conteudoBase64)
    {
        ConteudoDocx = conteudoBase64 ?? throw new ArgumentNullException(nameof(conteudoBase64));
    }

    public void DefinirComoPadrao()
    {
        EhPadrao = true;
    }

    public void RemoverPadrao()
    {
        EhPadrao = false;
    }

    public void Ativar()
    {
        Ativo = true;
        DataDesativacao = null;
        MotivoDesativacao = null;
    }

    public void Desativar(string motivo)
    {
        if (EhSistema)
            throw new InvalidOperationException("Templates do sistema não podem ser desativados");
        
        Ativo = false;
        DataDesativacao = DateTime.UtcNow;
        MotivoDesativacao = motivo;
    }

    public void AtualizarInformacoes(string nome, string descricao)
    {
        if (EhSistema)
            throw new InvalidOperationException("Templates do sistema não podem ser editados");
        
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        Descricao = descricao ?? throw new ArgumentNullException(nameof(descricao));
    }

    public void MarcarComoSistema()
    {
        EhSistema = true;
    }

    public bool PodeSerEditado() => !EhSistema;
    public bool PodeSerDeletado() => !EhSistema;
}

public enum ContractTemplateType
{
    PrestacaoServicoPJ = 1,
    CLT = 2,
    Estagio = 3,
    Temporario = 4,
    Autonomo = 5,
    Outro = 99
}
