using System.ComponentModel.DataAnnotations;

namespace Aure.Application.DTOs.Auth;

public class RequestPasswordResetRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Token é obrigatório")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "Senha deve conter maiúscula, minúscula, número e caractere especial")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("NovaSenha", ErrorMessage = "Senhas não conferem")]
    public string ConfirmacaoSenha { get; set; } = string.Empty;
}

public class PasswordResetResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
