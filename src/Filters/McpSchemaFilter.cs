using McpFramework.McpTypes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace McpFramework;

/// <summary>
/// Ensures Swagger/OpenAPI shows MCP primitives as their underlying JSON types
/// instead of empty objects. Keeps Swagger in sync with System.Text.Json converters.
/// </summary>
public class McpSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        // === MCP GUID ===
        if (typeof(McpGuid).IsAssignableFrom(type))
        {
            schema.Type = "string";
            schema.Format = "uuid";
            schema.Description = $"MCP GUID wrapper ({type.Name})";
            schema.Example = new OpenApiString(Guid.NewGuid().ToString());
            schema.Properties?.Clear();
        }
        // === MCP STRING ===
        else if (typeof(McpString).IsAssignableFrom(type))
        {
            schema.Type = "string";
            schema.Description = $"MCP string wrapper ({type.Name})";
            schema.Example = new OpenApiString("example");
            schema.Properties?.Clear();
        }
        // === MCP INT ===
        else if (typeof(McpInt).IsAssignableFrom(type))
        {
            schema.Type = "integer";
            schema.Format = "int32";
            schema.Description = $"MCP int wrapper ({type.Name})";
            schema.Example = new OpenApiInteger(42);
            schema.Properties?.Clear();
        }
        // === MCP DOUBLE/FLOAT/DECIMAL ===
        else if (typeof(McpDouble).IsAssignableFrom(type) ||
                 typeof(McpFloat).IsAssignableFrom(type) ||
                 typeof(McpDecimal).IsAssignableFrom(type))
        {
            schema.Type = "number";
            schema.Format = "double"; // good enough for float/decimal too
            schema.Description = $"MCP numeric wrapper ({type.Name})";
            schema.Example = new OpenApiDouble(3.14);
            schema.Properties?.Clear();
        }
        // === MCP BOOL ===
        else if (typeof(McpBool).IsAssignableFrom(type))
        {
            schema.Type = "boolean";
            schema.Description = $"MCP boolean wrapper ({type.Name})";
            schema.Example = new OpenApiBoolean(true);
            schema.Properties?.Clear();
        }
        // === MCP DATETIME ===
        else if (typeof(McpDateTime).IsAssignableFrom(type))
        {
            schema.Type = "string";
            schema.Format = "date-time";
            schema.Description = $"MCP DateTime wrapper ({type.Name})";
            schema.Example = new OpenApiString(DateTime.UtcNow.ToString("o"));
            schema.Properties?.Clear();
        }
        // === MCP COLLECTION<T> ===
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(McpCollection<>))
        {
            var itemType = type.GetGenericArguments()[0];
            schema.Type = "array";
            schema.Items = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);
            schema.Description = $"MCP collection of {itemType.Name}";
            schema.Properties?.Clear();
        }

        // Keep any custom MCP attributes (type-level, property-level) attached as x-mcp-* extensions
        // (you’ve already got this enrichment logic in place in your filters/enricher).
    }
}
