using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McpFramework.Attributes;
using McpFramework.McpTypes;

namespace McpFramework
{
    public class McpToolDiscoveryProcessor
    {
        private readonly McpMetadataProcessor _metadataProcessor;

        public McpToolDiscoveryProcessor()
        {
            _metadataProcessor = new McpMetadataProcessor();
        }

        public McpToolListResponse DiscoverTools(object controllerInstance)
        {
            var controllerType = controllerInstance.GetType();
            var response = new McpToolListResponse();

            // Extract domain metadata from controller
            response.Meta.Domain = ExtractDomainMetadata(controllerType, controllerInstance);

            // Find all methods marked with [McpTool]
            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<McpToolAttribute>() != null)
                .ToList();

            // Convert each method to a tool definition
            response.Tools = methods.Select(method => CreateToolDefinition(method)).ToList();

            return response;
        }

        private McpDomainMetadata ExtractDomainMetadata(Type controllerType, object controllerInstance)
        {
            var domainMeta = new McpDomainMetadata();

            // Extract header
            var header = controllerType.GetCustomAttribute<McpGlossaryHeaderAttribute>();
            if (header != null)
            {
                domainMeta.Title = header.Title;
                domainMeta.Description = header.Description;
            }

            // Extract terms
            var terms = controllerType.GetCustomAttributes<McpGlossaryTermAttribute>();
            domainMeta.Terms = terms.ToDictionary(t => t.Term, t => t.Definition);

            // Extract relationships
            var relationships = controllerType.GetCustomAttributes<McpGlossaryRelationshipAttribute>();
            domainMeta.Relationships = relationships.Select(r => new McpGlossaryRelationship
            {
                From = r.FromConcept,
                To = r.ToConcept,
                Description = r.Description
            }).ToList();

            // Extract notes
            var notes = controllerType.GetCustomAttributes<McpGlossaryNoteAttribute>();
            domainMeta.Notes = notes.Select(n => n.Note).ToList();

            // Extract domain data from marked methods
            var domainDataMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<McpDomainDataAttribute>() != null)
                .ToList();

            foreach (var method in domainDataMethods)
            {
                var attribute = method.GetCustomAttribute<McpDomainDataAttribute>()!;
                var dataType = string.IsNullOrEmpty(attribute.DataType) ? method.Name : attribute.DataType;

                try
                {
                    // Call the instance method with the controller instance
                    var data = method.Invoke(controllerInstance, null);
                    domainMeta.AdditionalData[dataType] = data;
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing
                    // For now, just skip this domain data method
                    domainMeta.AdditionalData[dataType] = new { error = $"Failed to extract domain data: {ex.Message}" };
                }
            }

            return domainMeta;
        }

        private McpToolDefinition CreateToolDefinition(MethodInfo method)
        {
            var toolAttr = method.GetCustomAttribute<McpToolAttribute>()!;
            var tool = new McpToolDefinition
            {
                Name = toolAttr.Name,
                Description = !string.IsNullOrEmpty(toolAttr.Description) 
                    ? toolAttr.Description 
                    : $"Execute {method.Name} operation",
                Category = !string.IsNullOrEmpty(toolAttr.Category) 
                    ? toolAttr.Category 
                    : "General",
                OperationType = !string.IsNullOrEmpty(toolAttr.OperationType) 
                    ? toolAttr.OperationType 
                    : "Read",
                Examples = toolAttr.Examples?.ToList() ?? new List<string>()
            };

            // Extract input schema from method parameters
            tool.InputSchema = CreateInputSchema(method);
            
            // Extract response guidance from McpResponseGuidance attributes
            tool.ResponseGuidance = ExtractResponseGuidance(method);
            
            // Enhance description with response schema information
            var responseSchema = GenerateResponseSchemaDocumentation(method);
            if (!string.IsNullOrEmpty(responseSchema))
            {
                tool.Description += $"\n\nRESPONSE SCHEMA:\n{responseSchema}";
            }

            return tool;
        }

        private List<McpResponseGuidance> ExtractResponseGuidance(MethodInfo method)
        {
            var guidanceList = new List<McpResponseGuidance>();
            
            // Extract all McpResponseGuidance attributes from the method
            var guidanceAttributes = method.GetCustomAttributes<McpResponseGuidanceAttribute>();
            
            foreach (var attr in guidanceAttributes)
            {
                var guidance = new McpResponseGuidance
                {
                    Category = attr.Category,
                    Message = attr.Message,
                    Priority = attr.Priority,
                    Condition = attr.Condition,
                    Examples = attr.Examples?.ToList() ?? new List<string>()
                };
                
                guidanceList.Add(guidance);
            }
            
            // Sort by priority (highest first)
            return guidanceList.OrderByDescending(g => g.Priority).ToList();
        }

        private string GenerateResponseSchemaDocumentation(MethodInfo method)
        {
            // Check for McpResponseType attribute first
            var responseTypeAttr = method.GetCustomAttribute<McpResponseTypeAttribute>();
            if (responseTypeAttr != null)
            {
                var documentation = GenerateTypeDocumentation(responseTypeAttr.ResponseType, false); // false = output context
                
                // Add custom description if provided
                if (!string.IsNullOrEmpty(responseTypeAttr.Description))
                {
                    documentation = $"{responseTypeAttr.Description}\n\n{documentation}";
                }
                
                return documentation;
            }
            
            // Fallback: Try to extract from return type (for methods that return concrete types)
            var returnType = method.ReturnType;
            
            // Handle Task<T>
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                returnType = returnType.GetGenericArguments()[0];
            }
            
            // Handle ActionResult<T> and IActionResult
            if (returnType.IsGenericType && returnType.Name.StartsWith("ActionResult"))
            {
                returnType = returnType.GetGenericArguments()[0];
            }
            else if (returnType.Name == "IActionResult")
            {
                // Can't determine response type from IActionResult - skip
                return string.Empty;
            }
            
            // Skip primitive types and common framework types for cleaner output
            if (returnType.IsPrimitive || returnType == typeof(string) || 
                returnType.Name.Contains("ActionResult") || returnType.Name.Contains("IActionResult"))
            {
                return string.Empty;
            }
            
            // Generate documentation for the response type
            return GenerateTypeDocumentation(returnType, false); // false = output context
        }

        private string GenerateTypeDocumentation(Type type, bool isInputContext)
        {
            var result = new System.Text.StringBuilder();
            
            // Handle primitive types
            if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(Guid))
            {
                return $"Returns: {GetFriendlyTypeName(type)}";
            }
            
            // Handle collections
            if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                var elementType = type.GetGenericArguments()[0];
                result.AppendLine($"Returns: Array of {GetFriendlyTypeName(elementType)}");
                
                // If the element type has MCP attributes, document it
                if (HasMcpAttributes(elementType))
                {
                    var elementDoc = GenerateTypeDocumentation(elementType, isInputContext);
                    if (!string.IsNullOrEmpty(elementDoc))
                    {
                        result.AppendLine($"  Element Type: {elementDoc}");
                    }
                }
                return result.ToString();
            }
            
            // Handle complex objects with properties
            var processor = new McpMetadataProcessor();
            var typeMetadata = processor.ExtractTypeMetadata(type, isInputContext);
            
            if (!string.IsNullOrEmpty(typeMetadata.Description))
            {
                result.AppendLine($"• {type.Name}: {typeMetadata.Description}");
                if (!string.IsNullOrEmpty(typeMetadata.Usage))
                {
                    result.AppendLine($"  Usage: {typeMetadata.Usage}");
                }
            }
            
            // Document properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .OrderBy(p => p.Name);
                
            foreach (var prop in properties)
            {
                var propDoc = GeneratePropertyDocumentation(prop, isInputContext);
                if (!string.IsNullOrEmpty(propDoc))
                {
                    result.AppendLine($"  ◦ {propDoc}");
                }
            }
            
            return result.ToString();
        }

        private string GeneratePropertyDocumentation(PropertyInfo property, bool isInputContext)
        {
            var propType = GetFriendlyTypeName(property.PropertyType);
            var result = $"{property.Name} ({propType})";
            
            // Check if property type has MCP metadata
            if (HasMcpAttributes(property.PropertyType))
            {
                var processor = new McpMetadataProcessor();
                var metadata = processor.ExtractTypeMetadata(property.PropertyType, isInputContext);
                
                if (!string.IsNullOrEmpty(metadata.Description))
                {
                    result += $": {metadata.Description}";
                }
                if (!string.IsNullOrEmpty(metadata.Usage))
                {
                    result += $" | {metadata.Usage}";
                }
            }
            
            return result;
        }

        private bool HasMcpAttributes(Type type)
        {
            return type.GetCustomAttribute<McpTypeDescriptionAttribute>() != null ||
                   type.GetCustomAttribute<McpTypePurposeAttribute>() != null ||
                   type.GetCustomAttribute<McpTypeUsageAttribute>() != null;
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int)) return "integer";
            if (type == typeof(double) || type == typeof(float)) return "number";
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(DateTime)) return "datetime";
            if (type == typeof(Guid)) return "uuid";
            
            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return GetFriendlyTypeName(type.GetGenericArguments()[0]) + "?";
            }
            
            // Handle generic collections
            if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                var elementType = type.GetGenericArguments()[0];
                return $"{GetFriendlyTypeName(elementType)}[]";
            }
            
            return type.Name;
        }

        private McpInputSchema CreateInputSchema(MethodInfo method)
        {
            var schema = new McpInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, object>(),
                Required = new List<string>()
            };

            var parameters = method.GetParameters();

            foreach (var param in parameters)
            {
                // Skip framework parameters (CancellationToken, etc.)
                if (ShouldSkipParameter(param))
                    continue;

                // Handle [FromBody] parameters by expanding their properties
                if (HasFromBodyAttribute(param))
                {
                    ExpandFromBodyParameter(param, schema);
                }
                else
                {
                    var paramSchema = CreateParameterSchema(param);
                    schema.Properties[param.Name!] = paramSchema;

                    // Check if parameter is required
                    if (IsParameterRequired(param))
                    {
                        schema.Required.Add(param.Name!);
                    }
                }
            }

            return schema;
        }

        private bool HasFromBodyAttribute(ParameterInfo parameter)
        {
            return parameter.GetCustomAttributes().Any(attr => 
                attr.GetType().Name == "FromBodyAttribute");
        }

        private void ExpandFromBodyParameter(ParameterInfo parameter, McpInputSchema schema)
        {
            var bodyType = parameter.ParameterType;
            var properties = bodyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var propSchema = CreatePropertySchema(prop);
                schema.Properties[prop.Name] = propSchema;

                // Check if property is required (for now, assume non-nullable value types are required)
                if (IsPropertyRequired(prop))
                {
                    schema.Required.Add(prop.Name);
                }
            }
        }

        private object CreatePropertySchema(PropertyInfo property)
        {
            var schema = new Dictionary<string, object>();

            // Handle different property types
            if (typeof(McpValue).IsAssignableFrom(property.PropertyType))
            {
                // It's one of our typed values - extract metadata
                var typeMetadata = _metadataProcessor.ExtractTypeMetadata(property.PropertyType);
                
                schema["type"] = "string"; // Most of our typed values are GUIDs (strings)
                schema["description"] = BuildPropertyDescription(typeMetadata, property);
                
                // Add format hint for GUIDs
                if (typeof(McpGuidValue).IsAssignableFrom(property.PropertyType))
                {
                    schema["format"] = "uuid";
                }
                
                // Add range constraints from McpRange attribute
                var rangeAttr = property.PropertyType.GetCustomAttribute<McpRangeAttribute>();
                if (rangeAttr != null)
                {
                    if (rangeAttr.Min != null) schema["minimum"] = rangeAttr.Min;
                    if (rangeAttr.Max != null) schema["maximum"] = rangeAttr.Max;
                }
                
                // Add validation constraints from other attributes
                var requiredAttr = property.GetCustomAttribute<McpRequiredAttribute>();
                if (requiredAttr != null)
                {
                    schema["required"] = true;
                }
                
                // Add exists constraint information
                var existsAttr = property.PropertyType.GetCustomAttribute<McpExistsAttribute>();
                if (existsAttr != null)
                {
                    schema["validation"] = new Dictionary<string, object>
                    {
                        ["exists"] = true,
                        ["entityType"] = existsAttr.EntityType ?? "Unknown",
                        ["message"] = existsAttr.CustomMessage ?? "Entity must exist"
                    };
                }
            }
            else if (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?))
            {
                schema["type"] = "string";
                schema["format"] = "uuid";
                schema["description"] = $"{property.Name} identifier";
            }
            else if (property.PropertyType == typeof(string))
            {
                schema["type"] = "string";
                schema["description"] = $"{property.Name} parameter";
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                schema["type"] = "integer";
                schema["description"] = $"{property.Name} parameter";
            }
            else if (property.PropertyType == typeof(double) || property.PropertyType == typeof(float) ||
                     property.PropertyType == typeof(double?) || property.PropertyType == typeof(float?))
            {
                schema["type"] = "number";
                schema["description"] = $"{property.Name} parameter";
            }
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                schema["type"] = "boolean";
                schema["description"] = $"{property.Name} parameter";
            }
            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                schema["type"] = "string";
                schema["format"] = "date-time";
                schema["description"] = $"{property.Name} date/time parameter";
            }
            else if (property.PropertyType.IsGenericType && 
                     property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = property.PropertyType.GetGenericArguments()[0];
                schema["type"] = "array";
                schema["items"] = CreateArrayItemSchema(itemType);
                schema["description"] = $"Array of {property.Name} items";
            }
            else
            {
                // Complex object - would need more sophisticated handling
                schema["type"] = "object";
                schema["description"] = $"{property.Name} parameter";
            }

            return schema;
        }

        private object CreateArrayItemSchema(Type itemType)
        {
            var itemSchema = new Dictionary<string, object>();

            if (itemType == typeof(string))
            {
                itemSchema["type"] = "string";
            }
            else if (itemType == typeof(Guid))
            {
                itemSchema["type"] = "string";
                itemSchema["format"] = "uuid";
            }
            else if (itemType == typeof(int))
            {
                itemSchema["type"] = "integer";
            }
            else if (itemType == typeof(double) || itemType == typeof(float))
            {
                itemSchema["type"] = "number";
            }
            else
            {
                itemSchema["type"] = "object";
            }

            return itemSchema;
        }

        private string BuildPropertyDescription(McpTypeMetadata typeMetadata, PropertyInfo property)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(typeMetadata.Description))
                parts.Add(typeMetadata.Description);

            if (!string.IsNullOrEmpty(typeMetadata.Purpose))
                parts.Add($"Purpose: {typeMetadata.Purpose}");

            if (!string.IsNullOrEmpty(typeMetadata.Usage))
                parts.Add($"Usage: {typeMetadata.Usage}");

            return parts.Count > 0 ? string.Join(". ", parts) : $"{property.Name} parameter";
        }

        private bool IsPropertyRequired(PropertyInfo property)
        {
            // Check for [McpRequired] attribute
            if (property.GetCustomAttribute<McpRequiredAttribute>() != null)
                return true;

            // Check if property is nullable
            if (property.PropertyType.IsGenericType && 
                property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return false;
            }

            // Check if it's a reference type that can be null
            if (!property.PropertyType.IsValueType)
            {
                return false; // Most reference types are optional by default
            }

            return true; // Non-nullable value types are required
        }

        private object CreateParameterSchema(ParameterInfo parameter)
        {
            var schema = new Dictionary<string, object>();

            // Handle different parameter types
            if (typeof(McpValue).IsAssignableFrom(parameter.ParameterType))
            {
                // It's one of our typed values - extract metadata
                var typeMetadata = _metadataProcessor.ExtractTypeMetadata(parameter.ParameterType);
                
                schema["type"] = "string"; // Most of our typed values are GUIDs (strings)
                schema["description"] = BuildParameterDescription(typeMetadata, parameter);
                
                // Add format hint for GUIDs
                if (typeof(McpGuidValue).IsAssignableFrom(parameter.ParameterType))
                {
                    schema["format"] = "uuid";
                }
                
                // Add range constraints from McpRange attribute
                var rangeAttr = parameter.ParameterType.GetCustomAttribute<McpRangeAttribute>();
                if (rangeAttr != null)
                {
                    if (rangeAttr.Min != null) schema["minimum"] = rangeAttr.Min;
                    if (rangeAttr.Max != null) schema["maximum"] = rangeAttr.Max;
                }
                
                // Add validation constraints from other attributes
                var requiredAttr = parameter.GetCustomAttribute<McpRequiredAttribute>();
                if (requiredAttr != null)
                {
                    schema["required"] = true;
                }
                
                // Add exists constraint information
                var existsAttr = parameter.ParameterType.GetCustomAttribute<McpExistsAttribute>();
                if (existsAttr != null)
                {
                    schema["validation"] = new Dictionary<string, object>
                    {
                        ["exists"] = true,
                        ["entityType"] = existsAttr.EntityType ?? "Unknown",
                        ["message"] = existsAttr.CustomMessage ?? "Entity must exist"
                    };
                }
            }
            else if (parameter.ParameterType == typeof(string))
            {
                schema["type"] = "string";
                schema["description"] = $"{parameter.Name} parameter";
            }
            else if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(int?))
            {
                schema["type"] = "integer";
                schema["description"] = $"{parameter.Name} parameter";
            }
            else if (parameter.ParameterType == typeof(double) || parameter.ParameterType == typeof(float) ||
                     parameter.ParameterType == typeof(double?) || parameter.ParameterType == typeof(float?))
            {
                schema["type"] = "number";
                schema["description"] = $"{parameter.Name} parameter";
            }
            else if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(bool?))
            {
                schema["type"] = "boolean";
                schema["description"] = $"{parameter.Name} parameter";
            }
            else
            {
                // Complex object - would need more sophisticated handling
                schema["type"] = "object";
                schema["description"] = $"{parameter.Name} parameter";
            }

            return schema;
        }

        private string BuildParameterDescription(McpTypeMetadata typeMetadata, ParameterInfo parameter)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(typeMetadata.Description))
                parts.Add(typeMetadata.Description);

            if (!string.IsNullOrEmpty(typeMetadata.Purpose))
                parts.Add($"Purpose: {typeMetadata.Purpose}");

            if (!string.IsNullOrEmpty(typeMetadata.Usage))
                parts.Add($"Usage: {typeMetadata.Usage}");

            return parts.Count > 0 ? string.Join(". ", parts) : $"{parameter.Name} parameter";
        }

        private bool ShouldSkipParameter(ParameterInfo parameter)
        {
            // Skip common framework types
            var typeName = parameter.ParameterType.FullName ?? parameter.ParameterType.Name;
            
            var skipTypeNames = new[]
            {
                "System.Threading.CancellationToken",
                "Microsoft.AspNetCore.Http.HttpContext", 
                "Microsoft.AspNetCore.Mvc.ControllerContext"
            };

            return skipTypeNames.Contains(typeName);
        }

        private bool IsParameterRequired(ParameterInfo parameter)
        {
            // Check for [McpRequired] attribute
            if (parameter.GetCustomAttribute<McpRequiredAttribute>() != null)
                return true;

            // Check if parameter is nullable or has default value
            if (parameter.HasDefaultValue || 
                parameter.ParameterType.IsGenericType && 
                parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return false;
            }

            return true; // Non-nullable, no default = required
        }

        /// <summary>
        /// Generate MCP-compliant tools/list response for a controller
        /// </summary>
        public async Task<McpToolsListResponse> GenerateToolsListResponseAsync(object controllerInstance)
        {
            var response = new McpToolsListResponse();
            
            // Use the existing DiscoverTools method (sync) - we can make this async later if needed
            await Task.CompletedTask; // For now, just to maintain async signature
            var toolList = DiscoverTools(controllerInstance);
            response.Tools = toolList.Tools;
            
            // Include metadata (glossary + domain data) 
            response._meta = new
            {
                glossary = new
                {
                    title = toolList.Meta.Domain.Title,
                    description = toolList.Meta.Domain.Description,
                    terms = toolList.Meta.Domain.Terms,
                    relationships = toolList.Meta.Domain.Relationships,
                    notes = toolList.Meta.Domain.Notes
                },
                domainData = toolList.Meta.Domain.AdditionalData,
                generatedAt = DateTime.UtcNow,
                version = "1.0",
                description = "Altu MCP tools for agentic memory management"
            };
            
            return response;
        }
    }

    // Response models for MCP tool discovery
    public class McpToolListResponse
    {
        public List<McpToolDefinition> Tools { get; set; } = new();
        public McpResponseMeta Meta { get; set; } = new();
    }

    public class McpResponseMeta
    {
        public McpDomainMetadata Domain { get; set; } = new();
    }

    public class McpDomainMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> Terms { get; set; } = new();
        public List<McpGlossaryRelationship> Relationships { get; set; } = new();
        public List<string> Notes { get; set; } = new();
        public Dictionary<string, object?> AdditionalData { get; set; } = new();
    }

    public class McpGlossaryRelationship
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class McpToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public List<string> Examples { get; set; } = new();
        public McpInputSchema InputSchema { get; set; } = new();
        public List<McpResponseGuidance> ResponseGuidance { get; set; } = new();
    }

    public class McpResponseGuidance
    {
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Priority { get; set; } = 3;
        public string? Condition { get; set; }
        public List<string> Examples { get; set; } = new();
    }

    public class McpInputSchema
    {
        public string Type { get; set; } = "object";
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> Required { get; set; } = new();
    }

    /// <summary>
    /// MCP standard tools/list response format
    /// </summary>
    public class McpToolsListResponse
    {
        public List<McpToolDefinition> Tools { get; set; } = new();
        public object? _meta { get; set; }
    }
}
