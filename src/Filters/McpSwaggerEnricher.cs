using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McpFramework.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Interfaces;

namespace McpFramework;

/// <summary>
/// Central enrichment class for MCP -> OpenAPI/Swagger metadata.
/// Collects controller, endpoint, and DTO attributes and emits them as x-mcp-* extensions.
/// </summary>
public class McpSwaggerEnricher : IDocumentFilter, IOperationFilter, ISchemaFilter
{
    // ------------------------------
    // DOCUMENT FILTER
    // ------------------------------
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var controllerType in context.ApiDescriptions
                 .Select(d => d.ActionDescriptor?.EndpointMetadata?.OfType<ControllerActionDescriptor>()?.FirstOrDefault()?.ControllerTypeInfo?.AsType())
                 .Where(t => t != null)
                 .Distinct()!)
        {
            ApplyDocumentAttributes(controllerType, swaggerDoc);
        }
    }

    private void ApplyDocumentAttributes(Type controllerType, OpenApiDocument doc)
    {
        if (controllerType == null) return;

        foreach (var attr in controllerType.GetCustomAttributes())
        {
            switch (attr)
            {
                case McpGlossaryHeaderAttribute header:
                    doc.Extensions["x-mcp-glossary-header"] = new OpenApiObject
                    {
                        ["title"] = new OpenApiString(header.Title),
                        ["description"] = new OpenApiString(header.Description)
                    };
                    break;

                case McpGlossaryTermAttribute term:
                    AddToExtensionArray(doc.Extensions, "x-mcp-glossary-terms",
                        new OpenApiObject
                        {
                            ["term"] = new OpenApiString(term.Term ?? string.Empty),
                            ["definition"] = new OpenApiString(term.Definition ?? string.Empty)
                        });
                    break;

                case McpGlossaryRelationshipAttribute rel:
                    AddToExtensionArray(doc.Extensions, "x-mcp-glossary-relationships",
                        new OpenApiObject
                        {
                            ["from"] = new OpenApiString(rel.FromConcept ?? string.Empty),
                            ["to"] = new OpenApiString(rel.ToConcept ?? string.Empty),
                            ["description"] = new OpenApiString(rel.Description ?? string.Empty)
                        });
                    break;

                case McpGlossaryNoteAttribute note:
                    AddToExtensionArray(doc.Extensions, "x-mcp-glossary-notes", new OpenApiString(note.Note));
                    break;
            }
        }
    }

    // ------------------------------
    // OPERATION FILTER
    // ------------------------------
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        if (methodInfo == null) return;

        foreach (var attr in methodInfo.GetCustomAttributes())
        {
            switch (attr)
            {
                case McpToolAttribute tool:
                    operation.Extensions["x-mcp-tool"] = new OpenApiString(tool.Name);
                    operation.Extensions["x-mcp-description"] = new OpenApiString(tool.Description);
                    operation.Extensions["x-mcp-category"] = new OpenApiString(tool.Category?.ToString() ?? "");
                    operation.Extensions["x-mcp-operationType"] = new OpenApiString(tool.OperationType?.ToString() ?? "");
                    if (tool.Examples?.Any() == true)
                    {
                        var arr = new OpenApiArray();
                        foreach (var ex in tool.Examples)
                            arr.Add(new OpenApiString(ex));
                        operation.Extensions["x-mcp-examples"] = arr;
                    }
                    break;

                case McpResponseTypeAttribute resp:
                    operation.Extensions["x-mcp-response-type"] = new OpenApiObject
                    {
                        ["type"] = new OpenApiString(resp.ResponseType.Name),
                        ["description"] = new OpenApiString(resp.Description ?? "")
                    };
                    break;

                case McpResponseGuidanceAttribute guidance:
                    var examplesArray = new OpenApiArray();
                    foreach (var ex in guidance.Examples ?? Array.Empty<string>())
                        examplesArray.Add(new OpenApiString(ex));

                    AddToExtensionArray(operation.Extensions, "x-mcp-response-guidance",
                        new OpenApiObject
                        {
                            ["category"] = new OpenApiString(guidance.Category ?? ""),
                            ["condition"] = new OpenApiString(guidance.Condition ?? ""),
                            ["examples"] = examplesArray,
                            ["message"] = new OpenApiString(guidance.Message ?? ""),
                            ["priority"] = new OpenApiString(guidance.Priority.ToString()),
                        });
                    break;
            }
        }
    }

    // ------------------------------
    // SCHEMA FILTER
    // ------------------------------
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        foreach (var attr in type.GetCustomAttributes())
        {
            switch (attr)
            {
                case McpTypeDescriptionAttribute desc:
                    schema.Extensions["x-mcp-type-description"] = new OpenApiString(desc.Description);
                    break;

                case McpTypePurposeAttribute purpose:
                    schema.Extensions["x-mcp-type-purpose"] = new OpenApiObject
                    {
                        ["input"] = new OpenApiString(purpose.Input ?? ""),
                        ["output"] = new OpenApiString(purpose.Output ?? "")
                    };
                    break;

                case McpTypeUsageAttribute usage:
                    schema.Extensions["x-mcp-type-usage"] = new OpenApiObject
                    {
                        ["input"] = new OpenApiString(usage.Input ?? ""),
                        ["output"] = new OpenApiString(usage.Output ?? "")
                    };
                    break;
            }
        }

        // Property-level attributes (McpRange, McpRequired, etc.)
        if (context.MemberInfo != null)
        {
            foreach (var attr in context.MemberInfo.GetCustomAttributes())
            {
                switch (attr)
                {
                    case McpRangeAttribute range:
                        schema.Extensions["x-mcp-range"] = new OpenApiObject
                        {
                            ["min"] = new OpenApiDouble(Convert.ToDouble(range.Min)),
                            ["max"] = new OpenApiDouble(Convert.ToDouble(range.Max)),
                            ["inclusive"] = new OpenApiBoolean(range.Inclusive)
                        };
                        break;

                    case McpRequiredAttribute:
                        schema.Extensions["x-mcp-required"] = new OpenApiBoolean(true);
                        break;

                    case McpTypeUsageAttribute usage:
                        schema.Extensions["x-mcp-type-usage"] = new OpenApiObject
                        {
                            ["input"] = new OpenApiString(usage.Input ?? ""),
                            ["output"] = new OpenApiString(usage.Output ?? "")
                        };
                        break;

                }
            }
        }
    }

    // ------------------------------
    // Helper
    // ------------------------------
    private static void AddToExtensionArray(IDictionary<string, IOpenApiExtension> dict, string key, IOpenApiAny value)
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
