using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Aure.Application.Interfaces;
using Aure.Infrastructure.Configuration;

namespace Aure.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
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