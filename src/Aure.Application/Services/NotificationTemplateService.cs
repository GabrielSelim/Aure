using Aure.Application.Interfaces;
using Aure.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Aure.Application.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationTemplateService> _logger;
    private readonly string _templatesPath;
    private readonly string _baseUrl;

    public NotificationTemplateService(
        IConfiguration configuration,
        ILogger<NotificationTemplateService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        _baseUrl = _configuration["BaseUrl"] ?? "https://aure.com.br";
    }

    public async Task<string> GeneratePaymentReceivedTemplateAsync(Payment payment, User recipient)
    {
        try
        {
            var baseTemplate = await LoadTemplateAsync("base-template.html");
            var iconSection = await LoadTemplateAsync("payment-received-icon.html");
            var highlightSection = await LoadTemplateAsync("payment-received-highlight.html");
            var detailsSection = await LoadTemplateAsync("payment-received-details.html");
            var buttonSection = await LoadTemplateAsync("payment-received-button.html");
            var infoSection = await LoadTemplateAsync("payment-received-info.html");

            var clientCompany = payment.Contract?.Client;
            var paymentDate = payment.CreatedAt.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-BR"));

            var template = baseTemplate
                .Replace("{{TITLE}}", "Pagamento Recebido")
                .Replace("{{COMPANY_LOGO}}", GetCompanyLogoUrl(clientCompany))
                .Replace("{{COMPANY_NAME}}", clientCompany?.Name ?? "Aure")
                .Replace("{{HEADER_TITLE}}", "Pagamento Recebido")
                .Replace("{{ICON_SECTION}}", iconSection)
                .Replace("{{GREETING}}", $"Olá, {recipient.Name}!")
                .Replace("{{MAIN_MESSAGE}}", $"Seu pagamento foi processado com sucesso pela <strong>{clientCompany?.Name}</strong>.")
                .Replace("{{HIGHLIGHT_SECTION}}", highlightSection)
                .Replace("{{DETAILS_SECTION}}", detailsSection)
                .Replace("{{BUTTON_SECTION}}", buttonSection)
                .Replace("{{INFO_BOX_SECTION}}", infoSection)
                .Replace("{{PORTAL_URL}}", $"{_baseUrl}")
                .Replace("{{SUPPORT_URL}}", $"{_baseUrl}/suporte")
                .Replace("{{PRIVACY_URL}}", $"{_baseUrl}/privacidade");

            // Replace payment-specific placeholders
            template = template
                .Replace("{{PAYMENT_AMOUNT}}", payment.Amount.ToString("C", new CultureInfo("pt-BR")))
                .Replace("{{PAYMENT_DATE}}", paymentDate)
                .Replace("{{CONTRACT_REFERENCE}}", $"#{payment.Contract?.Id.ToString().Substring(0, 8).ToUpper()}")
                .Replace("{{PAYMENT_METHOD}}", "PIX")
                .Replace("{{COMPANY_PAYER}}", clientCompany?.Name ?? "N/A")
                .Replace("{{DASHBOARD_URL}}", $"{_baseUrl}/dashboard/pagamentos");

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar template de pagamento recebido para usuário {UserId}", recipient.Id);
            return GenerateFallbackTemplate("Pagamento Recebido", $"Seu pagamento de {payment.Amount:C} foi processado com sucesso.");
        }
    }

    public async Task<string> GenerateContractCreatedTemplateAsync(Contract contract, User recipient)
    {
        try
        {
            var baseTemplate = await LoadTemplateAsync("base-template.html");
            var iconSection = await LoadTemplateAsync("contract-created-icon.html");
            var highlightSection = await LoadTemplateAsync("contract-created-highlight.html");
            var detailsSection = await LoadTemplateAsync("contract-created-details.html");
            var buttonSection = await LoadTemplateAsync("contract-created-button.html");
            var infoSection = await LoadTemplateAsync("contract-created-info.html");

            var clientCompany = contract.Client;
            var startDate = contract.StartDate.ToString("dd/MM/yyyy");
            var expirationDate = contract.ExpirationDate?.ToString("dd/MM/yyyy") ?? "Indeterminado";

            var template = baseTemplate
                .Replace("{{TITLE}}", "Novo Contrato Disponível")
                .Replace("{{COMPANY_LOGO}}", GetCompanyLogoUrl(clientCompany))
                .Replace("{{COMPANY_NAME}}", clientCompany?.Name ?? "Aure")
                .Replace("{{HEADER_TITLE}}", "Novo Contrato Disponível")
                .Replace("{{ICON_SECTION}}", iconSection)
                .Replace("{{GREETING}}", $"Olá, {recipient.Name}!")
                .Replace("{{MAIN_MESSAGE}}", "Um novo contrato está disponível para sua assinatura.")
                .Replace("{{HIGHLIGHT_SECTION}}", highlightSection)
                .Replace("{{DETAILS_SECTION}}", detailsSection)
                .Replace("{{BUTTON_SECTION}}", buttonSection)
                .Replace("{{INFO_BOX_SECTION}}", infoSection)
                .Replace("{{PORTAL_URL}}", $"{_baseUrl}")
                .Replace("{{SUPPORT_URL}}", $"{_baseUrl}/suporte")
                .Replace("{{PRIVACY_URL}}", $"{_baseUrl}/privacidade");

            // Replace contract-specific placeholders
            template = template
                .Replace("{{CONTRACT_VALUE}}", contract.ValueTotal.ToString("C", new CultureInfo("pt-BR")))
                .Replace("{{CONTRACT_TITLE}}", contract.Title)
                .Replace("{{CONTRACT_ID}}", contract.Id.ToString().Substring(0, 8).ToUpper())
                .Replace("{{START_DATE}}", startDate)
                .Replace("{{EXPIRATION_DATE}}", expirationDate)
                .Replace("{{MONTHLY_VALUE}}", contract.MonthlyValue?.ToString("C", new CultureInfo("pt-BR")) ?? "N/A")
                .Replace("{{CLIENT_COMPANY}}", clientCompany?.Name ?? "N/A")
                .Replace("{{CONTRACT_SIGN_URL}}", $"{_baseUrl}/contratos/{contract.Id}/assinar");

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar template de contrato criado para usuário {UserId}", recipient.Id);
            return GenerateFallbackTemplate("Novo Contrato", $"Contrato {contract.Title} está disponível para assinatura.");
        }
    }

    public async Task<string> GenerateContractSignedTemplateAsync(Contract contract, User recipient)
    {
        try
        {
            var baseTemplate = await LoadTemplateAsync("base-template.html");
            var iconSection = await LoadTemplateAsync("payment-received-icon.html"); // Reusing success icon
            
            var clientCompany = contract.Client;
            var signedDate = contract.SignedDate?.ToString("dd/MM/yyyy HH:mm") ?? "Recentemente";

            var template = baseTemplate
                .Replace("{{TITLE}}", "Contrato Assinado")
                .Replace("{{COMPANY_LOGO}}", GetCompanyLogoUrl(clientCompany))
                .Replace("{{COMPANY_NAME}}", clientCompany?.Name ?? "Aure")
                .Replace("{{HEADER_TITLE}}", "Contrato Assinado")
                .Replace("{{ICON_SECTION}}", iconSection)
                .Replace("{{GREETING}}", $"Olá, {recipient.Name}!")
                .Replace("{{MAIN_MESSAGE}}", $"O contrato <strong>{contract.Title}</strong> foi assinado com sucesso!")
                .Replace("{{HIGHLIGHT_SECTION}}", $@"
                    <div class=""highlight-card"">
                        <p class=""highlight-label"">Valor Mensal</p>
                        <p class=""highlight-value"">{contract.MonthlyValue?.ToString("C", new CultureInfo("pt-BR")) ?? "N/A"}</p>
                    </div>")
                .Replace("{{DETAILS_SECTION}}", $@"
                    <table class=""details-table"">
                        <tr>
                            <td>
                                <div class=""detail-label"">Funcionário:</div>
                                <div class=""detail-value"">{contract.Provider?.Name ?? "N/A"}</div>
                            </td>
                            <td class=""detail-right"">
                                <div class=""detail-label"">Data de Assinatura:</div>
                                <div class=""detail-value"">{signedDate}</div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div class=""detail-label"">Data de Início:</div>
                                <div class=""detail-value"">{contract.StartDate:dd/MM/yyyy}</div>
                            </td>
                            <td class=""detail-right"">
                                <div class=""detail-label"">Data de Vencimento:</div>
                                <div class=""detail-value"">{contract.ExpirationDate?.ToString("dd/MM/yyyy") ?? "Indeterminado"}</div>
                            </td>
                        </tr>
                    </table>")
                .Replace("{{BUTTON_SECTION}}", $@"
                    <div class=""btn-center"">
                        <a href=""{_baseUrl}/contratos/{contract.Id}"" class=""btn"">Ver Contrato</a>
                    </div>")
                .Replace("{{INFO_BOX_SECTION}}", "")
                .Replace("{{PORTAL_URL}}", $"{_baseUrl}")
                .Replace("{{SUPPORT_URL}}", $"{_baseUrl}/suporte")
                .Replace("{{PRIVACY_URL}}", $"{_baseUrl}/privacidade");

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar template de contrato assinado para usuário {UserId}", recipient.Id);
            return GenerateFallbackTemplate("Contrato Assinado", $"O contrato {contract.Title} foi assinado com sucesso.");
        }
    }

    public async Task<string> GenerateContractExpirationTemplateAsync(Contract contract, User recipient, int daysUntilExpiration)
    {
        try
        {
            var baseTemplate = await LoadTemplateAsync("base-template.html");
            var iconSection = await LoadTemplateAsync("contract-expiration-icon.html");
            var highlightSection = await LoadTemplateAsync("contract-expiration-highlight.html");
            var detailsSection = await LoadTemplateAsync("contract-expiration-details.html");
            var buttonSection = await LoadTemplateAsync("contract-expiration-button.html");
            var infoSection = await LoadTemplateAsync("contract-expiration-info.html");

            var clientCompany = contract.Client;
            var expirationDate = contract.ExpirationDate?.ToString("dd/MM/yyyy") ?? "N/A";

            var template = baseTemplate
                .Replace("{{TITLE}}", "Contrato Próximo ao Vencimento")
                .Replace("{{COMPANY_LOGO}}", GetCompanyLogoUrl(clientCompany))
                .Replace("{{COMPANY_NAME}}", clientCompany?.Name ?? "Aure")
                .Replace("{{HEADER_TITLE}}", "Contrato Próximo ao Vencimento")
                .Replace("{{ICON_SECTION}}", iconSection)
                .Replace("{{GREETING}}", $"Olá, {recipient.Name}!")
                .Replace("{{MAIN_MESSAGE}}", $"O contrato <strong>{contract.Title}</strong> vence em <strong>{daysUntilExpiration} dias</strong>.")
                .Replace("{{HIGHLIGHT_SECTION}}", highlightSection)
                .Replace("{{DETAILS_SECTION}}", detailsSection)
                .Replace("{{BUTTON_SECTION}}", buttonSection)
                .Replace("{{INFO_BOX_SECTION}}", infoSection)
                .Replace("{{PORTAL_URL}}", $"{_baseUrl}")
                .Replace("{{SUPPORT_URL}}", $"{_baseUrl}/suporte")
                .Replace("{{PRIVACY_URL}}", $"{_baseUrl}/privacidade");

            // Replace expiration-specific placeholders
            template = template
                .Replace("{{DAYS_UNTIL_EXPIRATION}}", daysUntilExpiration.ToString())
                .Replace("{{CONTRACT_TITLE}}", contract.Title)
                .Replace("{{EXPIRATION_DATE}}", expirationDate)
                .Replace("{{MONTHLY_VALUE}}", contract.MonthlyValue?.ToString("C", new CultureInfo("pt-BR")) ?? "N/A")
                .Replace("{{CLIENT_COMPANY}}", clientCompany?.Name ?? "N/A")
                .Replace("{{PROVIDER_COMPANY}}", contract.Provider?.Name ?? "N/A")
                .Replace("{{RENEWAL_URL}}", $"{_baseUrl}/contratos/{contract.Id}/renovar");

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar template de vencimento de contrato para usuário {UserId}", recipient.Id);
            return GenerateFallbackTemplate("Contrato Vencendo", $"O contrato {contract.Title} vence em {daysUntilExpiration} dias.");
        }
    }

    public async Task<string> GenerateNewEmployeeTemplateAsync(User newEmployee, User recipient)
    {
        try
        {
            var baseTemplate = await LoadTemplateAsync("base-template.html");
            var iconSection = await LoadTemplateAsync("contract-created-icon.html"); // Reusing info icon

            var template = baseTemplate
                .Replace("{{TITLE}}", "Novo Funcionário Cadastrado")
                .Replace("{{COMPANY_LOGO}}", GetCompanyLogoUrl(null))
                .Replace("{{COMPANY_NAME}}", "Aure")
                .Replace("{{HEADER_TITLE}}", "Novo Funcionário Cadastrado")
                .Replace("{{ICON_SECTION}}", iconSection)
                .Replace("{{GREETING}}", $"Olá, {recipient.Name}!")
                .Replace("{{MAIN_MESSAGE}}", $"<strong>{newEmployee.Name}</strong> foi adicionado como {newEmployee.Role}.")
                .Replace("{{HIGHLIGHT_SECTION}}", "")
                .Replace("{{DETAILS_SECTION}}", $@"
                    <table class=""details-table"">
                        <tr>
                            <td>
                                <div class=""detail-label"">Nome:</div>
                                <div class=""detail-value"">{newEmployee.Name}</div>
                            </td>
                            <td class=""detail-right"">
                                <div class=""detail-label"">Email:</div>
                                <div class=""detail-value"">{newEmployee.Email}</div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div class=""detail-label"">Tipo:</div>
                                <div class=""detail-value"">{newEmployee.Role}</div>
                            </td>
                            <td class=""detail-right"">
                                <div class=""detail-label"">Data de Cadastro:</div>
                                <div class=""detail-value"">{newEmployee.CreatedAt:dd/MM/yyyy}</div>
                            </td>
                        </tr>
                    </table>")
                .Replace("{{BUTTON_SECTION}}", $@"
                    <div class=""btn-center"">
                        <a href=""{_baseUrl}/funcionarios"" class=""btn"">Ver Funcionários</a>
                    </div>")
                .Replace("{{INFO_BOX_SECTION}}", "")
                .Replace("{{PORTAL_URL}}", $"{_baseUrl}")
                .Replace("{{SUPPORT_URL}}", $"{_baseUrl}/suporte")
                .Replace("{{PRIVACY_URL}}", $"{_baseUrl}/privacidade");

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar template de novo funcionário para usuário {UserId}", recipient.Id);
            return GenerateFallbackTemplate("Novo Funcionário", $"{newEmployee.Name} foi cadastrado como {newEmployee.Role}.");
        }
    }

    public async Task<string> GeneratePaymentProcessedTemplateAsync(Payment payment, User recipient)
    {
        try
        {
            var baseTemplate = await LoadTemplateAsync("base-template.html");
            var iconSection = await LoadTemplateAsync("payment-received-icon.html");

            var clientCompany = payment.Contract?.Client;
            var providerCompany = payment.Contract?.Provider;

            var template = baseTemplate
                .Replace("{{TITLE}}", "Pagamento Processado")
                .Replace("{{COMPANY_LOGO}}", GetCompanyLogoUrl(clientCompany))
                .Replace("{{COMPANY_NAME}}", clientCompany?.Name ?? "Aure")
                .Replace("{{HEADER_TITLE}}", "Pagamento Processado")
                .Replace("{{ICON_SECTION}}", iconSection)
                .Replace("{{GREETING}}", $"Olá, {recipient.Name}!")
                .Replace("{{MAIN_MESSAGE}}", $"Pagamento de <strong>{payment.Amount:C}</strong> foi processado com sucesso para {providerCompany?.Name}.")
                .Replace("{{HIGHLIGHT_SECTION}}", $@"
                    <div class=""highlight-card"">
                        <p class=""highlight-label"">Valor Processado</p>
                        <p class=""highlight-value"">{payment.Amount:C}</p>
                    </div>")
                .Replace("{{DETAILS_SECTION}}", $@"
                    <table class=""details-table"">
                        <tr>
                            <td>
                                <div class=""detail-label"">Data:</div>
                                <div class=""detail-value"">{payment.CreatedAt:dd/MM/yyyy}</div>
                            </td>
                            <td class=""detail-right"">
                                <div class=""detail-label"">Contrato:</div>
                                <div class=""detail-value"">#{payment.Contract?.Id.ToString().Substring(0, 8).ToUpper()}</div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""2"">
                                <div class=""detail-label"">Fornecedor:</div>
                                <div class=""detail-value"">{providerCompany?.Name ?? "N/A"}</div>
                            </td>
                        </tr>
                    </table>")
                .Replace("{{BUTTON_SECTION}}", $@"
                    <div class=""btn-center"">
                        <a href=""{_baseUrl}/dashboard/pagamentos"" class=""btn"">Ver Relatório de Pagamentos</a>
                    </div>")
                .Replace("{{INFO_BOX_SECTION}}", "")
                .Replace("{{PORTAL_URL}}", $"{_baseUrl}")
                .Replace("{{SUPPORT_URL}}", $"{_baseUrl}/suporte")
                .Replace("{{PRIVACY_URL}}", $"{_baseUrl}/privacidade");

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar template de pagamento processado para usuário {UserId}", recipient.Id);
            return GenerateFallbackTemplate("Pagamento Processado", $"Pagamento de {payment.Amount:C} foi processado com sucesso.");
        }
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        try
        {
            var filePath = Path.Combine(_templatesPath, templateName);
            if (File.Exists(filePath))
            {
                return await File.ReadAllTextAsync(filePath);
            }
            
            _logger.LogWarning("Template {TemplateName} não encontrado em {Path}", templateName, filePath);
            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar template {TemplateName}", templateName);
            return "";
        }
    }

    private static string GetCompanyLogoUrl(Company? company)
    {
        // TODO: Implementar lógica para obter logo da empresa
        // Por enquanto, retorna um placeholder
        return "https://via.placeholder.com/120x60/667eea/ffffff?text=AURE";
    }

    private static string GenerateFallbackTemplate(string title, string message)
    {
        return $@"
        <html>
        <body style='font-family: Arial, sans-serif; padding: 20px;'>
            <h2>{title}</h2>
            <p>{message}</p>
            <p>Este é um email automático do sistema Aure.</p>
        </body>
        </html>";
    }
}