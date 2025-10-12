using Aure.Application.DTOs.Company;
using Aure.Domain.Common;
using FluentValidation;

namespace Aure.Application.Validators.Company;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .Must(ValidationHelpers.IsValidCompanyName)
            .WithMessage("Company name must be between 2 and 200 characters with valid characters");

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("CNPJ is required")
            .Must(ValidationHelpers.IsValidCnpj)
            .WithMessage("Invalid CNPJ format or check digits");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid company type specified");
    }

    private static bool BeValidCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
        
        if (cnpj.Length != 14)
            return false;

        if (!cnpj.All(char.IsDigit))
            return false;

        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCnpj = cnpj[..12];
        var soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        var digito = resto.ToString();
        tempCnpj = tempCnpj + digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        digito = digito + resto.ToString();

        return cnpj.EndsWith(digito);
    }
}

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required")
            .Length(2, 200).WithMessage("Company name must be between 2 and 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid company type specified");
    }
}