using System.ComponentModel;

namespace Aure.Domain.Enums;

/// <summary>
/// Hierarquia de permissões no sistema
/// </summary>
public enum UserRole
{
    [Description("Dono da Empresa Pai - Todos os privilégios (pagamentos + operacional)")]
    DonoEmpresaPai = 1,
    
    [Description("Financeiro - Gestão financeira e operacional (sem autorizar pagamentos)")]
    Financeiro = 2,
    
    [Description("Jurídico - Contratos e documentação legal (sem dados financeiros sensíveis)")]
    Juridico = 3,
    
    [Description("Funcionário CLT - Funcionário com carteira assinada")]
    FuncionarioCLT = 4,
    
    [Description("Funcionário PJ - Prestador de serviço (Pessoa Jurídica)")]
    FuncionarioPJ = 5
}

/// <summary>
/// Tipo de empresa no sistema
/// </summary>
public enum CompanyType
{
    [Description("Cliente - Empresa que contrata serviços")]
    Client = 1,
    
    [Description("Fornecedor/Prestador - Empresa que presta serviços")]
    Provider = 2,
    
    [Description("Ambos - Empresa que atua como cliente e fornecedor")]
    Both = 3
}

/// <summary>
/// Modelo de negócio da empresa
/// </summary>
public enum BusinessModel
{
    [Description("Padrão - Empresa comum")]
    Standard = 1,
    
    [Description("Empresa Principal - Empresa que contrata PJs")]
    MainCompany = 2,
    
    [Description("PJ Contratado - Pessoa Jurídica contratada por outra empresa")]
    ContractedPJ = 3,
    
    [Description("Freelancer - Profissional autônomo individual")]
    Freelancer = 4
}

/// <summary>
/// Tipo de convite para novos usuários
/// </summary>
public enum InviteType
{
    [Description("Funcionário Interno - Usuário interno da empresa (Financeiro/Jurídico)")]
    Internal = 0,
    
    [Description("PJ Contratado - Pessoa Jurídica que terá empresa criada automaticamente")]
    ContractedPJ = 1,
    
    [Description("Usuário Externo - Acesso específico para usuário externo")]
    ExternalUser = 2
}

/// <summary>
/// Status de verificação KYC (Know Your Customer)
/// </summary>
public enum KycStatus
{
    [Description("Pendente - Aguardando verificação")]
    Pending = 1,
    
    [Description("Aprovado - Verificação concluída com sucesso")]
    Approved = 2,
    
    [Description("Rejeitado - Verificação falhou")]
    Rejected = 3
}

/// <summary>
/// Status do contrato
/// </summary>
public enum ContractStatus
{
    [Description("Rascunho - Contrato em elaboração")]
    Draft = 1,
    
    [Description("Ativo - Contrato em vigência")]
    Active = 2,
    
    [Description("Concluído - Contrato finalizado")]
    Completed = 3,
    
    [Description("Cancelado - Contrato cancelado antes da conclusão")]
    Cancelled = 4
}

/// <summary>
/// Status do pagamento
/// </summary>
public enum PaymentStatus
{
    [Description("Pendente - Aguardando processamento")]
    Pending = 1,
    
    [Description("Completado - Pagamento processado com sucesso")]
    Completed = 2,
    
    [Description("Falhou - Erro no processamento")]
    Failed = 3,
    
    [Description("Cancelado - Pagamento cancelado")]
    Cancelled = 4
}

/// <summary>
/// Método de pagamento
/// </summary>
public enum PaymentMethod
{
    [Description("PIX - Pagamento instantâneo")]
    PIX = 1,
    
    [Description("TED - Transferência Eletrônica Disponível")]
    TED = 2,
    
    [Description("Cartão de Crédito")]
    CreditCard = 3,
    
    [Description("Boleto Bancário")]
    Boleto = 4
}

/// <summary>
/// Método de assinatura de contrato
/// </summary>
public enum SignatureMethod
{
    [Description("Digital - Assinatura digital com certificado")]
    Digital = 1,
    
    [Description("Eletrônica - Assinatura eletrônica simples")]
    Electronic = 2,
    
    [Description("Manual - Assinatura física em papel")]
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