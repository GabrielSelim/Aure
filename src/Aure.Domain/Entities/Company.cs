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

    // Contatos
    public string? PhoneMobile { get; private set; }
    public string? PhoneLandline { get; private set; }

    // Endereço
    public string? AddressStreet { get; private set; }
    public string? AddressNumber { get; private set; }
    public string? AddressComplement { get; private set; }
    public string? AddressNeighborhood { get; private set; }
    public string? AddressCity { get; private set; }
    public string? AddressState { get; private set; }
    public string? AddressCountry { get; private set; }
    public string? AddressZipCode { get; private set; }

    public string? Nire { get; private set; }
    public string? StateRegistration { get; private set; }

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

    public void UpdateCnpj(string cnpj)
    {
        Cnpj = cnpj;
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

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da empresa não pode ser vazio", nameof(name));
        
        Name = name;
        UpdateTimestamp();
    }

    public void UpdateContactInfo(string phoneMobile, string? phoneLandline)
    {
        if (string.IsNullOrWhiteSpace(phoneMobile))
            throw new ArgumentException("Telefone celular é obrigatório", nameof(phoneMobile));
        
        PhoneMobile = phoneMobile;
        PhoneLandline = phoneLandline;
        UpdateTimestamp();
    }

    public void UpdateAddress(string street, string number, string? complement, string neighborhood,
        string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Rua é obrigatória", nameof(street));
        
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Número é obrigatório", nameof(number));
        
        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("Bairro é obrigatório", nameof(neighborhood));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Cidade é obrigatória", nameof(city));
        
        if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
            throw new ArgumentException("Estado inválido (deve ter 2 caracteres)", nameof(state));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("País é obrigatório", nameof(country));
        
        if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 8)
            throw new ArgumentException("CEP inválido (deve ter 8 dígitos)", nameof(zipCode));
        
        AddressStreet = street;
        AddressNumber = number;
        AddressComplement = complement;
        AddressNeighborhood = neighborhood;
        AddressCity = city;
        AddressState = state;
        AddressCountry = country;
        AddressZipCode = zipCode;
        UpdateTimestamp();
    }

    public void UpdateRegistrationInfo(string? nire, string? stateRegistration)
    {
        Nire = nire;
        StateRegistration = stateRegistration;
        UpdateTimestamp();
    }

    public string GetFormattedCnpj()
    {
        if (string.IsNullOrEmpty(Cnpj) || Cnpj.Length != 14)
            return Cnpj ?? string.Empty;
        
        return $"{Cnpj.Substring(0, 2)}.{Cnpj.Substring(2, 3)}.{Cnpj.Substring(5, 3)}/{Cnpj.Substring(8, 4)}-{Cnpj.Substring(12, 2)}";
    }

    public string GetFullAddress()
    {
        if (string.IsNullOrWhiteSpace(AddressStreet))
            return string.Empty;
        
        var address = $"{AddressStreet}, {AddressNumber}";
        
        if (!string.IsNullOrWhiteSpace(AddressComplement))
            address += $", {AddressComplement}";
        
        address += $" - {AddressNeighborhood}, {AddressCity}/{AddressState}, {AddressCountry} - CEP: {AddressZipCode}";
        
        return address;
    }
}