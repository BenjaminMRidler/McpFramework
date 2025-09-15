using McpFramework.Attributes;
using System;
using System.Threading.Tasks;

namespace McpFramework.McpTypes
{
    [McpTypeDescription("Decimal value with MCP validation and metadata for precise numeric calculations")]
    [McpTypePurpose(
        Input = "Used for decimal parameters that require high precision (e.g., relationship strengths, confidence scores)",
        Output = "Returns validated decimal values with context for precise calculations"
    )]
    [McpTypeUsage(
        Input = "Use for precise numeric parameters like scores, percentages, and financial values",
        Output = "Provides validated decimal results with proper precision"
    )]
    public class McpDecimal : McpTypedValue<decimal>
    {
        public McpDecimal(decimal value) : base(value) { }
        public McpDecimal() : base(0m) { }

        // Implicit conversion from decimal for convenience
        public static implicit operator McpDecimal(decimal value) => new McpDecimal(value);

        // Implicit conversion to decimal for service calls
        public static implicit operator decimal(McpDecimal mcpDecimal) => mcpDecimal.Value;

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // Decimals are always in valid format if they're valid decimals
            // No format validation needed for decimal
            return result;
        }

        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // For decimals, we consider 0 as a valid value (not missing)
            // If you want to require non-zero values, that should be handled by McpRange
            return result;
        }

        public override Task<McpValidationResult> ValidateExistenceAsync(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            // Basic validation - decimals are always valid if they're in range
            // Range validation is handled by McpRange attribute
            return Task.FromResult(result);
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            try
            {
                // Convert range boundaries to decimal
                var min = Convert.ToDecimal(rangeAttr.Min);
                var max = Convert.ToDecimal(rangeAttr.Max);

                bool isInRange;
                if (rangeAttr.Inclusive)
                {
                    isInRange = Value >= min && Value <= max;
                }
                else
                {
                    isInRange = Value > min && Value < max;
                }

                if (!isInRange)
                {
                    var inclusiveText = rangeAttr.Inclusive ? "inclusive" : "exclusive";
                    var defaultMessage = $"{parameterName} must be between {min:F2} and {max:F2} ({inclusiveText}). Current value: {Value:F2}";
                    
                    result.Errors.Add(new McpValidationError
                    {
                        ParameterName = parameterName,
                        ErrorCode = "OUT_OF_RANGE",
                        Message = !string.IsNullOrEmpty(rangeAttr.CustomMessage) ? rangeAttr.CustomMessage : defaultMessage,
                        Value = Value,
                        ToolName = toolName
                    });
                    result.IsValid = false;

                    // Add suggestion for corrected value
                    result.Suggestions.Add(new McpValidationSuggestion
                    {
                        ParameterName = parameterName,
                        Suggestion = $"Consider using a value between {min:F2} and {max:F2} ({inclusiveText})",
                        Example = (Value < min ? min : max).ToString("F2")
                    });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "RANGE_CONVERSION_ERROR",
                    Message = $"Could not convert range boundaries to decimal: {ex.Message}",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }
    }
}
