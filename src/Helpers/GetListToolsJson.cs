using McpFramework;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace McpFramework;

public class McpToolsListBuilder
{
    public static List<object> GetMcpToolsListAsJson(OpenApiDocument doc)
    {
        List<object> tools = [];

        foreach (var path in doc.Paths)
            foreach (var op in path.Value.Operations)
            {
                var opAttrs = op.Value.Extensions;

                if (opAttrs.ContainsKey("x-mcp-tool"))
                {
                    // Try to pull request schema
                    if (!(op.Value.RequestBody?.Content?.TryGetValue("application/json", out var requestContent) ?? false))
                        continue;

                    // Try to pull response schema
                    if (!(op.Value.Responses?.TryGetValue("200", out var response) ?? false))
                        continue;

                    if (!(response?.Content?.TryGetValue("application/json", out var responseContent) ?? false))
                        continue;

                    tools.Add(new
                    {
                        name = op.Value.OperationId ?? path.Key,
                        description = GetExt(op.Value, "x-mcp-description"),
                        category = GetExt(op.Value, "x-mcp-category"),
                        operationType = GetExt(op.Value, "x-mcp-operationType"),
                        examples = GetExt(op.Value, "x-mcp-examples"),
                        responseType = GetExt(op.Value, "x-mcp-response-type"),
                        responseGuidance = GetExt(op.Value, "x-mcp-response-guidance"),
                        inputSchema = NormalizeSchema(requestContent?.Schema),
                        responseSchema = NormalizeSchema(responseContent?.Schema),
                    });
                }
            }

        return tools;
    }

    // ------------------------------
    // Normalizers
    // ------------------------------

    private static object? NormalizeExtension(IOpenApiExtension? ext)
    {
        return ext switch
        {
            OpenApiString s => s.Value,
            OpenApiInteger i => i.Value,
            OpenApiDouble d => d.Value,
            OpenApiBoolean b => b.Value,
            OpenApiArray arr => arr.Select(NormalizeExtension).ToList(),
            OpenApiObject obj => obj.ToDictionary(kv => kv.Key, kv => NormalizeExtension(kv.Value)),
            _ => null
        };
    }

    private static object? GetExt(OpenApiOperation op, string key) =>
        NormalizeExtension(op.Extensions.TryGetValue(key, out var ext) ? ext : null);

    /// <summary>
    /// Normalize an OpenApiSchema into a plain CLR object (with MCP extensions preserved).
    /// </summary>
    private static object? NormalizeSchema(OpenApiSchema? schema)
    {
        if (schema == null) return null;

        var normalized = new Dictionary<string, object?>()
        {
            ["type"] = schema.Type,
            ["format"] = schema.Format,
            ["description"] = schema.Description,
            ["nullable"] = schema.Nullable,
        };

        // Collect MCP extensions on the schema
        foreach (var ext in schema.Extensions)
        {
            normalized[ext.Key] = NormalizeExtension(ext.Value);
        }

        return normalized;
    }
}
