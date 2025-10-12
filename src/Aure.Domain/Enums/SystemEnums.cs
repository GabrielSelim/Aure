namespace Aure.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    Company = 2,
    Provider = 3
}

public enum CompanyType
{
    Client = 1,
    Provider = 2,
    Both = 3
}

public enum KycStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum ContractStatus
{
    Draft = 1,
    Active = 2,
    Completed = 3,
    Cancelled = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

public enum PaymentMethod
{
    PIX = 1,
    TED = 2,
    CreditCard = 3,
    Boleto = 4
}

public enum SignatureMethod
{
    Digital = 1,
    Electronic = 2,
    Manual = 3
}

public enum NotificationType
{
    Email = 1,
    SMS = 2,
    Push = 3,
    InApp = 4
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Sent = 3,
    Cancelled = 4,
    Error = 5
}

public enum TaxType
{
    ICMS = 1,
    ISS = 2,
    PIS = 3,
    COFINS = 4,
    IPI = 5,
    II = 6
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Read = 4
}

public enum DocumentType
{
    CNPJ = 1,
    CPF = 2,
    RG = 3,
    Passport = 4
}

public enum SplitExecutionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}