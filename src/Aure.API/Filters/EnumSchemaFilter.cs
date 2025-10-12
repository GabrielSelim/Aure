using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace Aure.API.Filters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var enumName in Enum.GetNames(context.Type))
            {
                var memberInfo = context.Type.GetField(enumName);
                var descriptionAttribute = memberInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;
                
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
            }
        }
    }
}