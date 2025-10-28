using AutoMapper;
using Aure.Application.DTOs.User;
using Aure.Application.DTOs.Company;
using Aure.Application.DTOs.Contract;
using Aure.Application.DTOs.Payment;
using Aure.Domain.Entities;

namespace Aure.Application.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<User, UserResponse>();
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.PasswordHash, 
                opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

        CreateMap<Company, CompanyResponse>();
        CreateMap<CreateCompanyRequest, Company>();

        CreateMap<Contract, ContractResponse>();
        CreateMap<CreateContractRequest, Contract>()
            .ForMember(dest => dest.Sha256Hash,
                opt => opt.MapFrom(src => GenerateContractHash(src)));

        CreateMap<Payment, PaymentResponse>();
        CreateMap<CreatePaymentRequest, Payment>();

        CreateMap<UserInvite, InviteResponse>();
    }

    private static string GenerateContractHash(CreateContractRequest request)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var input = $"{request.ClientId}{request.ProviderId}{request.Title}{request.ValueTotal}{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}";
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}