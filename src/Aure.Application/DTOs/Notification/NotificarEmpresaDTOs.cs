using System.ComponentModel.DataAnnotations;

namespace Aure.Application.DTOs.Notification
{
    public class NotificarEmpresaIncompletaRequest
    {
        [Required(ErrorMessage = "ID da empresa é obrigatório")]
        public Guid EmpresaId { get; set; }

        [Required(ErrorMessage = "Lista de campos faltando é obrigatória")]
        [MinLength(1, ErrorMessage = "Deve haver pelo menos um campo faltando")]
        public List<string> CamposFaltando { get; set; } = new();

        public bool TentouGerarContrato { get; set; }
        public string? UsuarioSolicitante { get; set; }
    }

    public class NotificarEmpresaIncompletaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Destinatarios { get; set; } = new();
    }
}
