using Aure.Application.DTOs.Common;

namespace Aure.Application.Interfaces;

public interface ICnpjValidationService
{
    Task<CnpjValidationResult> ValidateAsync(string cnpj);
}