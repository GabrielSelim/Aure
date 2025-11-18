using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class ContractDocument : BaseEntity
{
    public Guid ContractId { get; private set; }
    public Contract Contract { get; private set; }
    public Guid? TemplateId { get; private set; }
    public ContractTemplate? Template { get; private set; }
    public string ConteudoHtml { get; private set; }
    public string? ConteudoPdf { get; private set; }
    public string? ConteudoDocx { get; private set; }
    public int VersaoMajor { get; private set; }
    public int VersaoMinor { get; private set; }
    public DateTime DataGeracao { get; private set; }
    public Guid? GeradoPorUsuarioId { get; private set; }
    public User? GeradoPorUsuario { get; private set; }
    public Dictionary<string, string> DadosPreenchidos { get; private set; }
    public bool EhVersaoFinal { get; private set; }
    public string? HashDocumento { get; private set; }

    private ContractDocument()
    {
        DadosPreenchidos = new Dictionary<string, string>();
    }

    public ContractDocument(
        Guid contractId,
        Guid? templateId,
        string conteudoHtml,
        Dictionary<string, string> dadosPreenchidos,
        Guid geradoPorUsuarioId)
    {
        ContractId = contractId;
        TemplateId = templateId;
        ConteudoHtml = conteudoHtml ?? throw new ArgumentNullException(nameof(conteudoHtml));
        DadosPreenchidos = dadosPreenchidos ?? new Dictionary<string, string>();
        GeradoPorUsuarioId = geradoPorUsuarioId;
        DataGeracao = DateTime.UtcNow;
        VersaoMajor = 1;
        VersaoMinor = 0;
        EhVersaoFinal = false;
    }

    public void GerarPdf(string conteudoBase64)
    {
        ConteudoPdf = conteudoBase64 ?? throw new ArgumentNullException(nameof(conteudoBase64));
        GerarHash();
    }

    public void GerarDocx(string conteudoBase64)
    {
        ConteudoDocx = conteudoBase64 ?? throw new ArgumentNullException(nameof(conteudoBase64));
    }

    public void DefinirComoVersaoFinal()
    {
        EhVersaoFinal = true;
        GerarHash();
    }

    public void IncrementarVersaoMenor()
    {
        VersaoMinor++;
        DataGeracao = DateTime.UtcNow;
        EhVersaoFinal = false;
    }

    public void IncrementarVersaoMaior()
    {
        VersaoMajor++;
        VersaoMinor = 0;
        DataGeracao = DateTime.UtcNow;
        EhVersaoFinal = false;
    }

    public string GetVersao() => $"{VersaoMajor}.{VersaoMinor}";

    private void GerarHash()
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var conteudoParaHash = ConteudoHtml + (ConteudoPdf ?? string.Empty);
        var bytes = System.Text.Encoding.UTF8.GetBytes(conteudoParaHash);
        var hash = sha256.ComputeHash(bytes);
        HashDocumento = Convert.ToBase64String(hash);
    }
}
