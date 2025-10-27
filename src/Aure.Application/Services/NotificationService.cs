using Aure.Application.Interfaces;
using Aure.Domain.Interfaces;
using Aure.Domain.Entities;
using Aure.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Aure.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly INotificationTemplateService _templateService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        INotificationTemplateService templateService,
        IBackgroundJobClient backgroundJobClient,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _templateService = templateService;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task SendPaymentNotificationToPJAsync(Guid paymentId)
    {
        try
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment?.Contract?.Provider == null)
            {
                _logger.LogWarning("Pagamento {PaymentId} não possui funcionário PJ associado", paymentId);
                return;
            }

            var providerUsers = await _unitOfWork.Users.GetByCompanyIdAsync(payment.Contract.ProviderId);
            var providerUser = providerUsers.FirstOrDefault(u => u.Role == UserRole.FuncionarioPJ);
            
            if (providerUser == null)
            {
                _logger.LogWarning("Nenhum usuário PJ encontrado para a empresa fornecedora do contrato {ContractId}", payment.Contract.Id);
                return;
            }
            
            var clientCompany = payment.Contract.Client;

            var htmlContent = await _templateService.GeneratePaymentReceivedTemplateAsync(payment, providerUser);
            
            var notification = new Notification(
                NotificationType.Email,
                providerUser.Email,
                "Pagamento Recebido",
                htmlContent,
                paymentId: paymentId
            );

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _backgroundJobClient.Enqueue<IEmailService>("notificacoes",
                x => x.SendNotificationEmailAsync(notification.Id)
            );

            _logger.LogInformation("Notificação de pagamento enfileirada para PJ {UserId} - Pagamento {PaymentId}", providerUser.Id, paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pagamento para PJ - PaymentId: {PaymentId}", paymentId);
        }
    }

    public async Task SendPaymentProcessedToManagersAsync(Guid paymentId)
    {
        try
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment?.Contract?.Client == null)
            {
                _logger.LogWarning("Pagamento {PaymentId} não possui empresa cliente associada", paymentId);
                return;
            }

            var companyId = payment.Contract.ClientId;
            var managers = await _unitOfWork.Users.GetByCompanyIdAsync(companyId);
            var financialManagers = managers.Where(u => u.Role == UserRole.Financeiro).ToList();

            var notifications = new List<Notification>();
            
            foreach (var manager in financialManagers)
            {
                var htmlContent = await _templateService.GeneratePaymentProcessedTemplateAsync(payment, manager);
                
                var notification = new Notification(
                    NotificationType.Email,
                    manager.Email,
                    "Pagamento Processado",
                    htmlContent,
                    paymentId: paymentId
                );

                await _unitOfWork.Notifications.AddAsync(notification);
                notifications.Add(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                _backgroundJobClient.Enqueue<IEmailService>("notificacoes",
                    x => x.SendNotificationEmailAsync(notification.Id)
                );
            }

            _logger.LogInformation("Notificações de pagamento processado enviadas para {Count} gestores - PaymentId: {PaymentId}", 
                financialManagers.Count, paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificações de pagamento processado - PaymentId: {PaymentId}", paymentId);
        }
    }

    public async Task SendContractCreatedToPJAsync(Guid contractId)
    {
        try
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            if (contract?.Provider == null)
            {
                _logger.LogWarning("Contrato {ContractId} não possui empresa fornecedora associada", contractId);
                return;
            }

            var providerUsers = await _unitOfWork.Users.GetByCompanyIdAsync(contract.ProviderId);
            var providerUser = providerUsers.FirstOrDefault(u => u.Role == UserRole.FuncionarioPJ);
            
            if (providerUser == null)
            {
                _logger.LogWarning("Nenhum usuário PJ encontrado para a empresa fornecedora do contrato {ContractId}", contractId);
                return;
            }

            var htmlContent = await _templateService.GenerateContractCreatedTemplateAsync(contract, providerUser);
            
            var notification = new Notification(
                NotificationType.Email,
                providerUser.Email,
                "Novo Contrato Disponível",
                htmlContent,
                contractId: contractId
            );

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _backgroundJobClient.Enqueue<IEmailService>("contratos",
                x => x.SendNotificationEmailAsync(notification.Id)
            );

            _logger.LogInformation("Notificação de contrato criado enviada para PJ {UserId} - ContractId: {ContractId}", 
                providerUser.Id, contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de contrato criado - ContractId: {ContractId}", contractId);
        }
    }

    public async Task SendContractSignedToManagersAsync(Guid contractId)
    {
        try
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            if (contract == null)
            {
                _logger.LogWarning("Contrato {ContractId} não encontrado", contractId);
                return;
            }

            var clientCompanyUsers = await _unitOfWork.Users.GetByCompanyIdAsync(contract.ClientId);
            var recipients = clientCompanyUsers.Where(u => 
                u.Role == UserRole.DonoEmpresaPai || 
                u.Role == UserRole.Financeiro || 
                u.Role == UserRole.Juridico).ToList();

            var notifications = new List<Notification>();
            
            foreach (var recipient in recipients)
            {
                var htmlContent = await _templateService.GenerateContractSignedTemplateAsync(contract, recipient);
                
                var notification = new Notification(
                    NotificationType.Email,
                    recipient.Email,
                    "Contrato Assinado",
                    htmlContent,
                    contractId: contractId
                );

                await _unitOfWork.Notifications.AddAsync(notification);
                notifications.Add(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                _backgroundJobClient.Enqueue<IEmailService>("contratos",
                    x => x.SendNotificationEmailAsync(notification.Id)
                );
            }

            _logger.LogInformation("Notificações de contrato assinado enviadas para {Count} gestores - ContractId: {ContractId}", 
                recipients.Count, contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificações de contrato assinado - ContractId: {ContractId}", contractId);
        }
    }

    public async Task SendContractExpirationAlertsAsync()
    {
        try
        {
            var currentDate = DateTime.UtcNow.Date;
            var contracts = await _unitOfWork.Contracts.GetActiveContractsAsync();
            
            var contractsNearExpiration = contracts.Where(c => 
                c.ExpirationDate.HasValue && 
                c.Status == ContractStatus.Active)
                .ToList();

            foreach (var contract in contractsNearExpiration)
            {
                var daysUntilExpiration = (contract.ExpirationDate!.Value.Date - currentDate).Days;
                
                if (daysUntilExpiration == 30 || daysUntilExpiration == 15 || daysUntilExpiration == 7)
                {
                    await SendExpirationAlertForContract(contract, daysUntilExpiration);
                }
            }

            _logger.LogInformation("Processados alertas de vencimento para {Count} contratos", contractsNearExpiration.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar alertas de vencimento de contratos");
        }
    }

    private async Task SendExpirationAlertForContract(Contract contract, int daysUntilExpiration)
    {
        try
        {
            var clientCompanyUsers = await _unitOfWork.Users.GetByCompanyIdAsync(contract.ClientId);
            var providerCompanyUsers = await _unitOfWork.Users.GetByCompanyIdAsync(contract.ProviderId);

            var recipients = new List<User>();
            
            recipients.AddRange(clientCompanyUsers.Where(u => 
                u.Role == UserRole.DonoEmpresaPai || u.Role == UserRole.Juridico));
            
            recipients.AddRange(providerCompanyUsers.Where(u => 
                u.Role == UserRole.FuncionarioPJ));

            var notifications = new List<Notification>();
            
            foreach (var recipient in recipients)
            {
                var htmlContent = await _templateService.GenerateContractExpirationTemplateAsync(contract, recipient, daysUntilExpiration);
                
                var notification = new Notification(
                    NotificationType.Email,
                    recipient.Email,
                    "Contrato Próximo ao Vencimento",
                    htmlContent,
                    contractId: contract.Id
                );

                await _unitOfWork.Notifications.AddAsync(notification);
                notifications.Add(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                _backgroundJobClient.Enqueue<IEmailService>("contratos",
                    x => x.SendNotificationEmailAsync(notification.Id)
                );
            }

            _logger.LogInformation("Alertas de vencimento enviados para contrato {ContractId} - {Days} dias", 
                contract.Id, daysUntilExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar alerta de vencimento para contrato {ContractId}", contract.Id);
        }
    }

    public async Task SendNewEmployeeNotificationAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user?.CompanyId == null)
            {
                _logger.LogWarning("Usuário {UserId} não possui empresa associada", userId);
                return;
            }

            var managers = await _unitOfWork.Users.GetByCompanyIdAsync(user.CompanyId.Value);
            var targetManagers = managers.Where(u => 
                u.Role == UserRole.Financeiro || 
                u.Role == UserRole.Juridico).ToList();

            var notifications = new List<Notification>();
            
            foreach (var manager in targetManagers)
            {
                var htmlContent = await _templateService.GenerateNewEmployeeTemplateAsync(user, manager);
                
                var notification = new Notification(
                    NotificationType.Email,
                    manager.Email,
                    "Novo Funcionário Cadastrado",
                    htmlContent
                );

                await _unitOfWork.Notifications.AddAsync(notification);
                notifications.Add(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                _backgroundJobClient.Enqueue<IEmailService>("notificacoes",
                    x => x.SendNotificationEmailAsync(notification.Id)
                );
            }

            _logger.LogInformation("Notificações de novo funcionário enviadas para {Count} gestores - UserId: {UserId}", 
                targetManagers.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificações de novo funcionário - UserId: {UserId}", userId);
        }
    }

    public async Task<bool> ProcessPendingNotificationsAsync()
    {
        try
        {
            var pendingNotifications = await _unitOfWork.Notifications.GetPendingNotificationsAsync();
            
            foreach (var notification in pendingNotifications)
            {
                _backgroundJobClient.Enqueue<IEmailService>(
                    x => x.SendNotificationEmailAsync(notification.Id)
                );
            }

            _logger.LogInformation("Processadas {Count} notificações pendentes", pendingNotifications.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar notificações pendentes");
            return false;
        }
    }

    public async Task ScheduleRecurringNotificationsAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                RecurringJob.AddOrUpdate(
                    "contract-expiration-alerts",
                    () => SendContractExpirationAlertsAsync(),
                    Cron.Daily(9)
                );
            });

            _logger.LogInformation("Jobs recorrentes de notificação agendados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao agendar jobs recorrentes de notificação");
        }
    }




}