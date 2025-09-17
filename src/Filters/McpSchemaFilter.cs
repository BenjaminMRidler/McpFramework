using McpFramework.Attributes;
using McpFramework.McpTypes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class McpSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        // === Primitive wrappers ===
        if (typeof(McpGuid).IsAssignableFrom(type))
        {
            schema.Type = "string";
            schema.Format = "uuid";
            schema.Description = $"MCP GUID wrapper ({type.Name})";
            schema.Example = new OpenApiString(Guid.NewGuid().ToString());
            schema.Properties?.Clear();
        }
        else if (typeof(McpString).IsAssignableFrom(type))
        {
            schema.Type = "string";
            schema.Description = $"MCP string wrapper ({type.Name})";
            schema.Example = new OpenApiString("string");
            schema.Properties?.Clear();
        }
        else if (typeof(McpInt).IsAssignableFrom(type))
        {
            schema.Type = "integer";
            schema.Format = "int32";
            schema.Description = $"MCP int wrapper ({type.Name})";
            schema.Example = new OpenApiInteger(0);
            schema.Properties?.Clear();
        }
        else if (typeof(McpDouble).IsAssignableFrom(type) ||
                 typeof(McpFloat).IsAssignableFrom(type) ||
                 typeof(McpDecimal).IsAssignableFrom(type))
        {
            schema.Type = "number";
            schema.Format = "double";
            schema.Description = $"MCP numeric wrapper ({type.Name})";
            schema.Example = new OpenApiDouble(0.0);
            schema.Properties?.Clear();
        }
        else if (typeof(McpBool).IsAssignableFrom(type))
        {
            schema.Type = "boolean";
            schema.Description = $"MCP boolean wrapper ({type.Name})";
            schema.Example = new OpenApiBoolean(false);
            schema.Properties?.Clear();
        }
        else if (typeof(McpDateTime).IsAssignableFrom(type))
        {
            schema.Type = "string";
            schema.Format = "date-time";
            schema.Description = $"MCP DateTime wrapper ({type.Name})";
            schema.Example = new OpenApiString(DateTime.UtcNow.ToString("o"));
            schema.Properties?.Clear();
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(McpCollection<>))
        {
            var itemType = type.GetGenericArguments()[0];
            schema.Type = "array";
            schema.Items = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);
            schema.Description = $"MCP collection of {itemType.Name}";
            schema.Properties?.Clear();
        }

        // === Type-level attributes ===
        foreach (var attr in type.GetCustomAttributes(inherit: true))
        {
            switch (attr)
            {
                case McpTypeDescriptionAttribute desc:
                    schema.Extensions["x-mcp-description"] = new OpenApiString(desc.Description);
                    break;

                case McpTypePurposeAttribute purpose:
                    schema.Extensions["x-mcp-purpose"] = new OpenApiObject
                    {
                        ["input"] = new OpenApiString(purpose.Input),
                        ["output"] = new OpenApiString(purpose.Output)
                    };
                    break;

                case McpTypeUsageAttribute usage:
                    schema.Extensions["x-mcp-usage"] = new OpenApiObject
                    {
                        ["input"] = new OpenApiString(usage.Input ?? ""),
                        ["output"] = new OpenApiString(usage.Output ?? "")
                    };
                    break;
            }
        }

        // === Property-level attributes ===
        if (context.MemberInfo != null)
        {
            foreach (var attr in context.MemberInfo.GetCustomAttributes(inherit: true))
            {
                switch (attr)
                {
                    case McpRequiredAttribute:
                        schema.Extensions["x-mcp-required"] = new OpenApiBoolean(true);
                        schema.Nullable = false;
                        break;

                    case McpRangeAttribute range:
                        schema.Extensions["x-mcp-range"] = new OpenApiObject
                        {
                            ["min"] = new OpenApiDouble(Convert.ToDouble(range.Min)),
                            ["max"] = new OpenApiDouble(Convert.ToDouble(range.Max)),
                            ["inclusive"] = new OpenApiBoolean(range.Inclusive)
                        };
                        schema.Minimum = Convert.ToDecimal(range.Min);
                        schema.Maximum = Convert.ToDecimal(range.Max);
                        break;

                    case McpTypeUsageAttribute usage:
                        schema.Extensions["x-mcp-usage"] = new OpenApiObject
                        {
                            ["input"] = new OpenApiString(usage.Input ?? ""),
                            ["output"] = new OpenApiString(usage.Output ?? "")
                        };
                        break;
                }
            }
        }
    }
}