using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid? ContractId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public NotificationType Type { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime? SentAt { get; private set; }
    public NotificationStatus Status { get; private set; }

    public Contract? Contract { get; private set; }
    public Payment? Payment { get; private set; }

    private Notification() { }

    public Notification(NotificationType type, string recipientEmail, string subject, string content, Guid? contractId = null, Guid? paymentId = null)
    {
        Type = type;
        RecipientEmail = recipientEmail;
        Subject = subject;
        Content = content;
        ContractId = contractId;
        PaymentId = paymentId;
        Status = NotificationStatus.Pending;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void MarkAsFailed()
    {
        Status = NotificationStatus.Failed;
        UpdateTimestamp();
    }

    public void UpdateContent(string subject, string content)
    {
        Subject = subject;
        Content = content;
        UpdateTimestamp();
    }
}