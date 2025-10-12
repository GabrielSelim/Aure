using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public CompanyType Type { get; private set; }
    public BusinessModel BusinessModel { get; private set; }
    public KycStatus KycStatus { get; private set; }

    private readonly List<Contract> _clientContracts = new();
    private readonly List<Contract> _providerContracts = new();
    private readonly List<KycRecord> _kycRecords = new();
    private readonly List<Payment> _payments = new();

    public IReadOnlyCollection<Contract> ClientContracts => _clientContracts.AsReadOnly();
    public IReadOnlyCollection<Contract> ProviderContracts => _providerContracts.AsReadOnly();
    public IReadOnlyCollection<KycRecord> KycRecords => _kycRecords.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Company() { }

    public Company(string name, string cnpj, CompanyType type, BusinessModel businessModel = BusinessModel.Standard)
    {
        Name = name;
        Cnpj = cnpj;
        Type = type;
        BusinessModel = businessModel;
        KycStatus = Enums.KycStatus.Pending;
    }

    public void UpdateCompanyInfo(string name, CompanyType type, BusinessModel businessModel)
    {
        Name = name;
        Type = type;
        BusinessModel = businessModel;
        UpdateTimestamp();
    }

    public void UpdateKycStatus(KycStatus status)
    {
        KycStatus = status;
        UpdateTimestamp();
    }

    public void AddClientContract(Contract contract)
    {
        _clientContracts.Add(contract);
    }

    public void AddProviderContract(Contract contract)
    {
        _providerContracts.Add(contract);
    }

    public void AddKycRecord(KycRecord kycRecord)
    {
        _kycRecords.Add(kycRecord);
    }

    public void AddPayment(Payment payment)
    {
        _payments.Add(payment);
    }
}