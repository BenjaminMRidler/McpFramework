using McpFramework.Attributes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
namespace McpFramework;
public class McpOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        if (methodInfo == null) return;

        foreach (var attr in methodInfo.GetCustomAttributes(inherit: true))
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

                    operation.Extensions.AddToExtensionArray("x-mcp-response-guidance",
                        new OpenApiObject
                        {
                            ["category"] = new OpenApiString(guidance.Category ?? ""),
                            ["condition"] = new OpenApiString(guidance.Condition ?? ""),
                            ["examples"] = examplesArray,
                            ["message"] = new OpenApiString(guidance.Message ?? ""),
                            ["priority"] = new OpenApiInteger(guidance.Priority)
                        });
                    break;
            }
        }
    }
}