using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using McpFramework.Attributes;
using McpFramework.McpTypes;

namespace McpFramework
{
    public class McpValidationProcessor
    {
        public async Task<McpValidationResult> ValidateObjectAsync(object obj, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            var type = obj.GetType();

            // Get all properties of the object
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                await ValidatePropertyAsync(property, value, toolName, result);
            }

            return result;
        }

        private async Task ValidatePropertyAsync(PropertyInfo property, object? value, string toolName, McpValidationResult result)
        {
            var parameterName = property.Name;

            // Check if property is required
            var requiredAttr = property.GetCustomAttribute<McpRequiredAttribute>();
            if (requiredAttr != null)
            {
                if (value == null)
                {
                    result.IsValid = false;
                    result.Errors.Add(new McpValidationError
                    {
                        ParameterName = parameterName,
                        ErrorCode = "REQUIRED",
                        Message = !string.IsNullOrEmpty(requiredAttr.CustomMessage) 
                            ? requiredAttr.CustomMessage 
                            : $"{parameterName} is required",
                        Value = value,
                        ToolName = toolName
                    });
                    return; // Skip further validation if value is null and required
                }
            }

            // If value is null and not required, skip validation
            if (value == null) return;

            // If value is an McpValue, use its validation methods
            if (value is McpValue mcpValue)
            {
                // Validate format (always run)
                var formatResult = mcpValue.ValidateFormat(parameterName, toolName);
                if (!formatResult.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(formatResult.Errors);
                    result.Suggestions.AddRange(formatResult.Suggestions);
                }

                // Validate required (only if [McpRequired] attribute is present)
                if (requiredAttr != null)
                {
                    var requiredResult = mcpValue.ValidateRequired(parameterName, toolName);
                    if (!requiredResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(requiredResult.Errors);
                        result.Suggestions.AddRange(requiredResult.Suggestions);
                    }
                }

                // Check for existence validation attribute
                var existsAttr = property.GetCustomAttribute<McpExistsAttribute>();
                if (existsAttr != null)
                {
                    var existenceResult = await mcpValue.ValidateExistenceAsync(parameterName, toolName);
                    if (!existenceResult.IsValid)
                    {
                        result.IsValid = false;
                        result.Errors.AddRange(existenceResult.Errors);
                        result.Suggestions.AddRange(existenceResult.Suggestions);
                    }
                }
            }

            // Check range validation
            var rangeAttr = property.GetCustomAttribute<McpRangeAttribute>();
            if (rangeAttr != null && value is McpValue mcpRangeValue)
            {
                var rangeResult = mcpRangeValue.ValidateRange(rangeAttr, parameterName, toolName);
                if (!rangeResult.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(rangeResult.Errors);
                    result.Suggestions.AddRange(rangeResult.Suggestions);
                }
            }
        }
    }
}
