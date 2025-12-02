namespace Aure.Domain.Entities
{
    public class NotificationHistory
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public string TipoNotificacao { get; private set; } = string.Empty;
        public string CamposNotificados { get; private set; } = string.Empty;
        public DateTime DataEnvio { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private NotificationHistory() { }

        public NotificationHistory(Guid userId, string tipoNotificacao, List<string> camposNotificados)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TipoNotificacao = tipoNotificacao;
            CamposNotificados = string.Join(", ", camposNotificados);
            DataEnvio = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }

        public bool FoiEnviadoRecentemente(int horasLimite = 24)
        {
            return DataEnvio.AddHours(horasLimite) > DateTime.UtcNow;
        }

        public bool MesmosCampos(List<string> campos)
        {
            var camposOrdenados = string.Join(", ", campos.OrderBy(c => c));
            var camposExistentesOrdenados = string.Join(", ", CamposNotificados.Split(", ").OrderBy(c => c));
            return camposOrdenados == camposExistentesOrdenados;
        }
    }
}
