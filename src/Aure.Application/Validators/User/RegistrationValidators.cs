using Aure.Application.DTOs.User;
using Aure.Domain.Common;
using FluentValidation;

namespace Aure.Application.Validators.User;

public class RegisterCompanyAdminRequestValidator : AbstractValidator<RegisterCompanyAdminRequest>
{
    public RegisterCompanyAdminRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .Must(ValidationHelpers.IsValidCompanyName)
            .WithMessage("Company name must be between 2 and 200 characters with valid characters");

        RuleFor(x => x.CompanyCnpj)
            .NotEmpty().WithMessage("CNPJ is required")
            .Must(ValidationHelpers.IsValidCnpj)
            .WithMessage("Invalid CNPJ format or check digits");

        RuleFor(x => x.CompanyType)
            .IsInEnum().WithMessage("Invalid company type specified");

        RuleFor(x => x.BusinessModel)
            .IsInEnum().WithMessage("Invalid business model specified");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Must(ValidationHelpers.IsValidPersonName)
            .WithMessage("Name must contain only letters and spaces, between 2 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Must(ValidationHelpers.IsValidEmail)
            .WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Must(ValidationHelpers.IsValidPassword)
            .WithMessage("Password must be at least 8 characters with uppercase, lowercase, number and special character");
    }
}

public class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Must(ValidationHelpers.IsValidPersonName)
            .WithMessage("Name must contain only letters and spaces, between 2 and 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Must(ValidationHelpers.IsValidEmail)
            .WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified")
            .When(x => x.Role.HasValue);
    }
}

public class AcceptInviteRequestValidator : AbstractValidator<AcceptInviteRequest>
{
    public AcceptInviteRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Must(ValidationHelpers.IsValidPassword)
            .WithMessage("Password must be at least 8 characters with uppercase, lowercase, number and special character");
    }
}