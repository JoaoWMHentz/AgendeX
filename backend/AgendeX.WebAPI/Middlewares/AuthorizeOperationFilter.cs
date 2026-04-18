using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgendeX.WebAPI.Middlewares;

public sealed class AllowAnonymousOperationFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (KeyValuePair<string, IOpenApiPathItem> path in swaggerDoc.Paths)
        {
            IOpenApiPathItem pathItem = path.Value ?? throw new InvalidOperationException("OpenAPI path item missing.");
            IReadOnlyDictionary<System.Net.Http.HttpMethod, OpenApiOperation> operations = pathItem.Operations ?? throw new InvalidOperationException("OpenAPI operations missing.");
            bool anonymousPath = path.Key.StartsWith("/api/Auth", StringComparison.OrdinalIgnoreCase);

            foreach (KeyValuePair<System.Net.Http.HttpMethod, OpenApiOperation> operationEntry in operations)
            {
                if (anonymousPath)
                {
                    operationEntry.Value.Security = new List<OpenApiSecurityRequirement>();
                    continue;
                }

                operationEntry.Value.Security ??= new List<OpenApiSecurityRequirement>();
                operationEntry.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference(
                            JwtBearerDefaults.AuthenticationScheme,
                            swaggerDoc,
                            null),
                        new List<string>()
                    }
                });
            }
        }
    }
}
