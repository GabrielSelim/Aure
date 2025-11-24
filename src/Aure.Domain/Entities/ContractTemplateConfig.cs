using Aure.Domain.Common;

namespace Aure.Domain.Entities
{
    public class ContractTemplateConfig : BaseEntity
    {
        public Guid CompanyId { get; private set; }
        public string TituloServico { get; private set; } = string.Empty;
        public string DescricaoServico { get; private set; } = string.Empty;
        public string LocalPrestacaoServico { get; private set; } = string.Empty;
        public List<string> DetalhamentoServicos { get; private set; } = new();
        public string? ClausulaAjudaCusto { get; private set; }
        public List<string> ObrigacoesContratado { get; private set; } = new();
        public List<string> ObrigacoesContratante { get; private set; } = new();
        public new DateTime UpdatedAt { get; private set; }

        public Company Company { get; set; } = null!;

        private ContractTemplateConfig()
        {
            TituloServico = string.Empty;
            DescricaoServico = string.Empty;
            LocalPrestacaoServico = string.Empty;
            DetalhamentoServicos = new List<string>();
            ObrigacoesContratado = new List<string>();
            ObrigacoesContratante = new List<string>();
        }

        public ContractTemplateConfig(
            Guid companyId,
            string tituloServico,
            string descricaoServico,
            string localPrestacaoServico,
            List<string> detalhamentoServicos,
            List<string> obrigacoesContratado,
            List<string> obrigacoesContratante,
            string? clausulaAjudaCusto = null)
        {
            CompanyId = companyId;
            TituloServico = tituloServico ?? throw new ArgumentNullException(nameof(tituloServico));
            DescricaoServico = descricaoServico ?? throw new ArgumentNullException(nameof(descricaoServico));
            LocalPrestacaoServico = localPrestacaoServico ?? throw new ArgumentNullException(nameof(localPrestacaoServico));
            DetalhamentoServicos = detalhamentoServicos ?? throw new ArgumentNullException(nameof(detalhamentoServicos));
            ObrigacoesContratado = obrigacoesContratado ?? throw new ArgumentNullException(nameof(obrigacoesContratado));
            ObrigacoesContratante = obrigacoesContratante ?? throw new ArgumentNullException(nameof(obrigacoesContratante));
            ClausulaAjudaCusto = clausulaAjudaCusto;
            UpdatedAt = DateTime.UtcNow;

            ValidateEntity();
        }

        public void Update(
            string tituloServico,
            string descricaoServico,
            string localPrestacaoServico,
            List<string> detalhamentoServicos,
            List<string> obrigacoesContratado,
            List<string> obrigacoesContratante,
            string? clausulaAjudaCusto = null)
        {
            TituloServico = tituloServico ?? throw new ArgumentNullException(nameof(tituloServico));
            DescricaoServico = descricaoServico ?? throw new ArgumentNullException(nameof(descricaoServico));
            LocalPrestacaoServico = localPrestacaoServico ?? throw new ArgumentNullException(nameof(localPrestacaoServico));
            DetalhamentoServicos = detalhamentoServicos ?? throw new ArgumentNullException(nameof(detalhamentoServicos));
            ObrigacoesContratado = obrigacoesContratado ?? throw new ArgumentNullException(nameof(obrigacoesContratado));
            ObrigacoesContratante = obrigacoesContratante ?? throw new ArgumentNullException(nameof(obrigacoesContratante));
            ClausulaAjudaCusto = clausulaAjudaCusto;
            UpdatedAt = DateTime.UtcNow;

            ValidateEntity();
        }

        private void ValidateEntity()
        {
            if (string.IsNullOrWhiteSpace(TituloServico))
                throw new ArgumentException("Título do serviço não pode ser vazio", nameof(TituloServico));

            if (TituloServico.Length > 200)
                throw new ArgumentException("Título do serviço deve ter no máximo 200 caracteres", nameof(TituloServico));

            if (string.IsNullOrWhiteSpace(DescricaoServico))
                throw new ArgumentException("Descrição do serviço não pode ser vazia", nameof(DescricaoServico));

            if (DescricaoServico.Length > 1000)
                throw new ArgumentException("Descrição do serviço deve ter no máximo 1000 caracteres", nameof(DescricaoServico));

            if (string.IsNullOrWhiteSpace(LocalPrestacaoServico))
                throw new ArgumentException("Local de prestação não pode ser vazio", nameof(LocalPrestacaoServico));

            if (LocalPrestacaoServico.Length > 500)
                throw new ArgumentException("Local de prestação deve ter no máximo 500 caracteres", nameof(LocalPrestacaoServico));

            if (DetalhamentoServicos == null || !DetalhamentoServicos.Any())
                throw new ArgumentException("Detalhamento dos serviços não pode ser vazio", nameof(DetalhamentoServicos));

            if (ObrigacoesContratado == null || !ObrigacoesContratado.Any())
                throw new ArgumentException("Obrigações do contratado não podem ser vazias", nameof(ObrigacoesContratado));

            if (ObrigacoesContratante == null || !ObrigacoesContratante.Any())
                throw new ArgumentException("Obrigações da contratante não podem ser vazias", nameof(ObrigacoesContratante));
        }

        public string GerarDetalhamentoServicosHtml()
        {
            var html = "<ol type=\"I\">";
            foreach (var servico in DetalhamentoServicos)
            {
                html += $"<li>{servico}</li>";
            }
            html += "</ol>";
            return html;
        }

        public string GerarObrigacoesContratadoHtml()
        {
            var html = "<ol type=\"I\">";
            foreach (var obrigacao in ObrigacoesContratado)
            {
                html += $"<li>{obrigacao}</li>";
            }
            html += "</ol>";
            return html;
        }

        public string GerarObrigacoesContratanteHtml()
        {
            var html = "<ol type=\"I\">";
            foreach (var obrigacao in ObrigacoesContratante)
            {
                html += $"<li>{obrigacao}</li>";
            }
            html += "</ol>";
            return html;
        }
    }
}
