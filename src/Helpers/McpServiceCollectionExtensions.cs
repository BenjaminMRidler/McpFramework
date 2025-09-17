
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
namespace McpFramework;

public static class McpServiceCollectionExtensions
{
    public static IServiceCollection AddMcpFramework(
        this IServiceCollection services,
        Action<McpMetaOptions>? configureMeta = null)
    {
        // Runtime serialization
        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new McpJsonConverterFactory());
        });

        // Domain meta options (consumer can configure these)
        if (configureMeta != null)
            services.Configure(configureMeta);

        // Swagger filters
        services.AddSwaggerGen(c =>
        {
            c.DocumentFilter<McpDocumentFilter>();
            c.OperationFilter<McpOperationFilter>();
            c.SchemaFilter<McpSchemaFilter>();
        });

        return services;
    }

    public static object AsListToolsJson(this OpenApiDocument doc)
    {
        return McpToolsListBuilder.GetMcpToolsListAsJson(doc);
    }

    public static void AddToExtensionArray(this IDictionary<string, IOpenApiExtension> dict, string key, IOpenApiAny value)
    {
        if (!dict.TryGetValue(key, out var existing))
        {
            existing = new OpenApiArray();
            dict[key] = existing;
        }

        var arr = existing as OpenApiArray;
        arr?.Add(value);
    }
}
