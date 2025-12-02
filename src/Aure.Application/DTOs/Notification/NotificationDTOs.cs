using System.ComponentModel.DataAnnotations;

namespace Aure.Application.DTOs.Notification
{
    public class NotificarCompletarCadastroRequest
    {
        [Required(ErrorMessage = "ID do usuário é obrigatório")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Lista de campos faltando é obrigatória")]
        [MinLength(1, ErrorMessage = "Deve haver pelo menos um campo faltando")]
        public List<string> CamposFaltando { get; set; } = new();
    }

    public class NotificarCompletarCadastroResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class NotificationHistoryDto
    {
        public Guid UserId { get; set; }
        public string TipoNotificacao { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public string CamposNotificados { get; set; } = string.Empty;
    }
}
