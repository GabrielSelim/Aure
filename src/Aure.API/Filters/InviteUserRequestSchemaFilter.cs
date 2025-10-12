using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Aure.Application.DTOs.User;
using Aure.Domain.Enums;

namespace Aure.API.Filters;

public class InviteUserRequestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(InviteUserRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("João Silva"),
                ["email"] = new OpenApiString("joao.silva@empresa.com"),
                ["role"] = new OpenApiString("Provider"),
                ["inviteType"] = new OpenApiString("ContractedPJ"),
                ["companyName"] = new OpenApiString("João Silva Consultoria ME"),
                ["cnpj"] = new OpenApiString("12345678000190"),
                ["companyType"] = new OpenApiString("Provider"),
                ["businessModel"] = new OpenApiString("ContractedPJ")
            };
        }
    }
}