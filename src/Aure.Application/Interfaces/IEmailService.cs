using System.Threading.Tasks;

namespace Aure.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendInviteEmailAsync(string recipientEmail, string recipientName, string inviteToken, string inviterName, string companyName);
    Task<bool> SendNotificationEmailAsync(Guid notificationId);
    Task<bool> SendPaymentNotificationAsync(string recipientEmail, string recipientName, decimal amount, DateTime paymentDate, string contractReference, string companyName);
    Task<bool> SendWelcomeEmailAsync(string recipientEmail, string recipientName, string companyName);
    Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetLink);
}