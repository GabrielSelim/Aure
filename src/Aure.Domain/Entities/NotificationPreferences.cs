using Aure.Domain.Common;

namespace Aure.Domain.Entities;

public class NotificationPreferences : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    // Notificações de Contrato
    public bool ReceberEmailNovoContrato { get; private set; } = true;
    public bool ReceberEmailContratoAssinado { get; private set; } = true;
    public bool ReceberEmailContratoVencendo { get; private set; } = true;

    // Notificações de Pagamento
    public bool ReceberEmailPagamentoProcessado { get; private set; } = true;
    public bool ReceberEmailPagamentoRecebido { get; private set; } = true;

    // Notificações Operacionais
    public bool ReceberEmailNovoFuncionario { get; private set; } = true;
    public bool ReceberEmailAlertasFinanceiros { get; private set; } = true;

    // Notificações de Sistema
    public bool ReceberEmailAtualizacoesSistema { get; private set; } = true;

    private NotificationPreferences() { }

    public NotificationPreferences(Guid userId)
    {
        UserId = userId;
        // Valores padrão já definidos nas propriedades
    }

    public void UpdatePreferences(
        bool? receberEmailNovoContrato = null,
        bool? receberEmailContratoAssinado = null,
        bool? receberEmailContratoVencendo = null,
        bool? receberEmailPagamentoProcessado = null,
        bool? receberEmailPagamentoRecebido = null,
        bool? receberEmailNovoFuncionario = null,
        bool? receberEmailAlertasFinanceiros = null,
        bool? receberEmailAtualizacoesSistema = null)
    {
        if (receberEmailNovoContrato.HasValue)
            ReceberEmailNovoContrato = receberEmailNovoContrato.Value;
            
        if (receberEmailContratoAssinado.HasValue)
            ReceberEmailContratoAssinado = receberEmailContratoAssinado.Value;
            
        if (receberEmailContratoVencendo.HasValue)
            ReceberEmailContratoVencendo = receberEmailContratoVencendo.Value;
            
        if (receberEmailPagamentoProcessado.HasValue)
            ReceberEmailPagamentoProcessado = receberEmailPagamentoProcessado.Value;
            
        if (receberEmailPagamentoRecebido.HasValue)
            ReceberEmailPagamentoRecebido = receberEmailPagamentoRecebido.Value;
            
        if (receberEmailNovoFuncionario.HasValue)
            ReceberEmailNovoFuncionario = receberEmailNovoFuncionario.Value;
            
        if (receberEmailAlertasFinanceiros.HasValue)
            ReceberEmailAlertasFinanceiros = receberEmailAlertasFinanceiros.Value;
            
        if (receberEmailAtualizacoesSistema.HasValue)
            ReceberEmailAtualizacoesSistema = receberEmailAtualizacoesSistema.Value;

        UpdateTimestamp();
    }
}
