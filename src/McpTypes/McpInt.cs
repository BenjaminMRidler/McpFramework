using McpFramework.Attributes;
using System;
using System.Threading.Tasks;

namespace McpFramework.McpTypes
{
    [McpTypeDescription("Integer value with MCP validation and metadata")]
    [McpTypePurpose(
        Input = "Used for integer parameters that need validation and rich metadata",
        Output = "Returns validated integer values with context"
    )]
    [McpTypeUsage(
        Input = "Use for numeric parameters like counts, depths, limits, and thresholds",
        Output = "Provides validated integer results with proper typing"
    )]
    public class McpInt : McpTypedValue<int>
    {
        public McpInt(int value) : base(value) { }
        public McpInt() : base(0) { }

        // Implicit conversion from int for convenience
        public static implicit operator McpInt(int value) => new McpInt(value);

        // Implicit conversion to int for service calls
        public static implicit operator int(McpInt mcpInt) => mcpInt.Value;

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // Integers are always in valid format if they're valid integers
            // No format validation needed for int
            return result;
        }

        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // For integers, we consider 0 as a valid value (not missing)
            // If you want to require non-zero values, that should be handled by McpRange
            return result;
        }

        public override Task<McpValidationResult> ValidateExistenceAsync(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            // Basic validation - integers are always valid if they're in range
            // Range validation is handled by McpRange attribute
            return Task.FromResult(result);
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            try
            {
                // Convert range boundaries to int
                var min = Convert.ToInt32(rangeAttr.Min);
                var max = Convert.ToInt32(rangeAttr.Max);

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
                    var defaultMessage = $"{parameterName} must be between {min} and {max} ({inclusiveText}). Current value: {Value}";
                    
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
                        Suggestion = $"Consider using a value between {min} and {max} ({inclusiveText})",
                        Example = (Value < min ? min : max).ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "RANGE_CONVERSION_ERROR",
                    Message = $"Could not convert range boundaries to integer: {ex.Message}",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }
    }
}
