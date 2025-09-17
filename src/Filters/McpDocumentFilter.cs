using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using McpFramework.Attributes;
using Microsoft.OpenApi.Any;

namespace McpFramework;

public class McpDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var apiDesc in context.ApiDescriptions)
        {
            var controllerType = apiDesc.ActionDescriptor
                .EndpointMetadata
                .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                .FirstOrDefault()?
                .ControllerTypeInfo.AsType();

            if (controllerType == null) continue;

            foreach (var attr in controllerType.GetCustomAttributes(inherit: true))
            {
                switch (attr)
                {
                    case McpGlossaryHeaderAttribute header:
                        swaggerDoc.Extensions["x-mcp-glossary-header"] = new OpenApiObject
                        {
                            ["title"] = new OpenApiString(header.Title),
                            ["description"] = new OpenApiString(header.Description)
                        };
                        break;

                    case McpGlossaryTermAttribute term:
                        swaggerDoc.Extensions.AddToExtensionArray("x-mcp-glossary-terms",
                            new OpenApiObject
                            {
                                ["term"] = new OpenApiString(term.Term ?? ""),
                                ["definition"] = new OpenApiString(term.Definition ?? "")
                            });
                        swaggerDoc.Extensions.AddToExtensionArray("x-mcp-glossary-terms",
                            new OpenApiObject
                            {
                                ["term"] = new OpenApiString(term.Term ?? ""),
                                ["definition"] = new OpenApiString(term.Definition ?? "")
                            });
                        break;

                    case McpGlossaryRelationshipAttribute rel:
                        swaggerDoc.Extensions.AddToExtensionArray("x-mcp-glossary-relationships",
                            new OpenApiObject
                            {
                                ["from"] = new OpenApiString(rel.FromConcept ?? ""),
                                ["to"] = new OpenApiString(rel.ToConcept ?? ""),
                                ["description"] = new OpenApiString(rel.Description ?? "")
                            });
                        break;

                    case McpGlossaryNoteAttribute note:
                        swaggerDoc.Extensions.AddToExtensionArray("x-mcp-glossary-notes",
                            new OpenApiString(note.Note));
                        break;
                }
            }
        }
    }
}