namespace Aure.Domain.Enums;

public enum UserRole
{
    DonoEmpresaPai = 1,    // Dono com todos os privilégios (pagamentos + operacional)
    Financeiro = 2,        // Gestão financeira e operacional (sem autorizar pagamentos)
    Juridico = 3,          // Contratos e documentação legal (sem dados financeiros sensíveis)
    FuncionarioCLT = 4,    // Funcionário com carteira assinada
    FuncionarioPJ = 5      // Prestador de serviço (Pessoa Jurídica)
}

public enum CompanyType
{
    Client = 1,
    Provider = 2,
    Both = 3
}

public enum BusinessModel
{
    Standard = 1,         // Empresa padrão
    MainCompany = 2,      // Empresa que contrata PJs
    ContractedPJ = 3,     // PJ contratado
    Freelancer = 4        // Freelancer individual
}

public enum InviteType
{
    Employee = 0,          // Funcionário interno da empresa
    ContractedPJ = 1,      // PJ contratado que terá sua própria empresa criada  
    ExternalUser = 2       // Usuário externo para acesso específico
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

public enum RelationshipType
{
    ContractedPJ = 1,      // Empresa contrata PJ
    Partnership = 2,       // Parceria entre empresas
    Supplier = 3,          // Fornecedor
    Client = 4             // Cliente
}

public enum RelationshipStatus
{
    Active = 1,
    Inactive = 2,
    Terminated = 3,
    Suspended = 4
}