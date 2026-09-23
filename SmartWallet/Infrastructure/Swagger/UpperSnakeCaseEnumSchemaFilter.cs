using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Nasurino.SmartWallet.Infrastructure.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nasurino.SmartWallet.Infrastructure.Swagger;

public class UpperSnakeCaseEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = Enum.GetNames(context.Type)
            .Select(name => new OpenApiString(new UpperSnakeCaseNamingPolicy().ConvertName(name)))
            .ToList<IOpenApiAny>();
    }
}
