using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Aure.Application.Interfaces;
using Aure.Infrastructure.Configuration;
using Aure.Domain.Interfaces;

namespace Aure.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IUnitOfWork unitOfWork)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> SendInviteEmailAsync(string recipientEmail, string recipientName, string inviteToken, string inviterName, string companyName)
    {
        try
        {
            var message = new MimeMessage();
            
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = $"Convite para participar do Sistema Aure - {companyName}";

            var inviteLink = $"{_emailSettings.BaseUrl}/api/Registration/aceitar-convite/{inviteToken}";
            
            var htmlBody = await GetEmailTemplate(recipientName, recipientEmail, inviterName, companyName, inviteLink);
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = GetTextEmailBody(recipientName, inviterName, companyName, inviteLink)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Para Gmail na porta 587, usar STARTTLS
            var secureSocketOptions = _emailSettings.SmtpPort == 587 
                ? MailKit.Security.SecureSocketOptions.StartTls 
                : (_emailSettings.UseSsl ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.None);
            
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, secureSocketOptions);
            
            if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email de convite enviado com sucesso para {Email}", recipientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de convite para {Email}", recipientEmail);
            return false;
        }
    }

    private async Task<string> GetEmailTemplate(string recipientName, string recipientEmail, string inviterName, string companyName, string inviteLink)
    {
        try
        {
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "InviteEmailTemplate.html");
            
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Template de email não encontrado em {Path}", templatePath);
                return GetFallbackTemplate(recipientName, inviterName, companyName, inviteLink);
            }

            var template = await File.ReadAllTextAsync(templatePath);
            
            return template
                .Replace("{{RECIPIENT_NAME}}", recipientName)
                .Replace("{{RECIPIENT_EMAIL}}", recipientEmail)
                .Replace("{{INVITER_NAME}}", inviterName)
                .Replace("{{COMPANY_NAME}}", companyName)
                .Replace("{{INVITE_LINK}}", inviteLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar template de email");
            return GetFallbackTemplate(recipientName, inviterName, companyName, inviteLink);
        }
    }

    private string GetFallbackTemplate(string recipientName, string inviterName, string companyName, string inviteLink)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Convite - Sistema Aure</h2>
                <p>Olá {recipientName},</p>
                <p>Você foi convidado por <strong>{inviterName}</strong> da empresa <strong>{companyName}</strong> para participar do Sistema Aure.</p>
                <p>Para aceitar este convite, clique no link abaixo:</p>
                <p><a href='{inviteLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Aceitar Convite</a></p>
                <p>Ou copie e cole este link no seu navegador:</p>
                <p>{inviteLink}</p>
                <br>
                <p>Atenciosamente,<br>Sistema Aure</p>
            </body>
            </html>";
    }

    public async Task<bool> SendNotificationEmailAsync(Guid notificationId)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null) return false;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", notification.RecipientEmail));
            message.Subject = notification.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = notification.Content,
                TextBody = StripHtml(notification.Content)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _emailSettings.SmtpPort == 587 
                ? MailKit.Security.SecureSocketOptions.StartTls 
                : (_emailSettings.UseSsl ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.None);
            
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, secureSocketOptions);
            
            if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            notification.MarkAsSent();
            await _unitOfWork.Notifications.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notificação {NotificationId} enviada com sucesso para {Email}", notificationId, notification.RecipientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<bool> SendPaymentNotificationAsync(string recipientEmail, string recipientName, decimal amount, DateTime paymentDate, string contractReference, string companyName)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "Pagamento Recebido - Sistema Aure";

            var htmlBody = GetPaymentNotificationTemplate(recipientName, amount, paymentDate, contractReference, companyName);
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = GetPaymentNotificationTextBody(recipientName, amount, paymentDate, contractReference, companyName)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _emailSettings.SmtpPort == 587 
                ? MailKit.Security.SecureSocketOptions.StartTls 
                : (_emailSettings.UseSsl ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.None);
            
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, secureSocketOptions);
            
            if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Notificação de pagamento enviada com sucesso para {Email}", recipientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pagamento para {Email}", recipientEmail);
            return false;
        }
    }

    private string GetPaymentNotificationTemplate(string recipientName, decimal amount, DateTime paymentDate, string contractReference, string companyName)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #28a745;'>Pagamento Recebido</h2>
                    <p>Olá {recipientName},</p>
                    <p>Seu pagamento no valor de <strong>R$ {amount:F2}</strong> foi processado com sucesso.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Detalhes do Pagamento:</h3>
                        <ul>
                            <li><strong>Valor:</strong> R$ {amount:F2}</li>
                            <li><strong>Data do Pagamento:</strong> {paymentDate:dd/MM/yyyy}</li>
                            <li><strong>Referência:</strong> {contractReference}</li>
                            <li><strong>Empresa Pagadora:</strong> {companyName}</li>
                        </ul>
                    </div>
                    
                    <p>Atenciosamente,<br>Sistema Aure</p>
                </div>
            </body>
            </html>";
    }

    private string GetPaymentNotificationTextBody(string recipientName, decimal amount, DateTime paymentDate, string contractReference, string companyName)
    {
        return $@"
Pagamento Recebido - Sistema Aure

Olá {recipientName},

Seu pagamento no valor de R$ {amount:F2} foi processado com sucesso.

Detalhes do Pagamento:
- Valor: R$ {amount:F2}
- Data do Pagamento: {paymentDate:dd/MM/yyyy}
- Referência: {contractReference}
- Empresa Pagadora: {companyName}

Atenciosamente,
Sistema Aure
        ";
    }

    private string StripHtml(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
    }

    private string GetTextEmailBody(string recipientName, string inviterName, string companyName, string inviteLink)
    {
        return $@"
Convite - Sistema Aure

Olá {recipientName},

Você foi convidado por {inviterName} da empresa {companyName} para participar do Sistema Aure.

Para aceitar este convite, acesse o link abaixo:
{inviteLink}

Atenciosamente,
Sistema Aure
        ";
    }
}