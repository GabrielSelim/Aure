using System.Threading.Tasks;

namespace Aure.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendInviteEmailAsync(string recipientEmail, string recipientName, string inviteToken, string inviterName, string companyName);
}