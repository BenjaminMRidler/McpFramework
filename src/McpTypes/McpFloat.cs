using McpFramework.Attributes;
using System;
using System.Threading.Tasks;

namespace McpFramework.McpTypes
{
    [McpTypeDescription("Float value with MCP validation and metadata for floating-point calculations")]
    [McpTypePurpose(
        Input = "Used for float parameters that need floating-point precision (e.g., similarity scores, thresholds)",
        Output = "Returns validated float values with context for floating-point calculations"
    )]
    [McpTypeUsage(
        Input = "Use for floating-point numeric parameters like scores, thresholds, and similarity values",
        Output = "Provides validated float results with proper floating-point precision"
    )]
    public class McpFloat : McpTypedValue<float>
    {
        public McpFloat(float value) : base(value) { }
        public McpFloat() : base(0f) { }

        // Implicit conversion from float for convenience
        public static implicit operator McpFloat(float value) => new McpFloat(value);

        // Implicit conversion to float for service calls
        public static implicit operator float(McpFloat mcpFloat) => mcpFloat.Value;

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // Floats are always in valid format if they're valid floats
            // No format validation needed for float
            return result;
        }

        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // For floats, we consider 0 as a valid value (not missing)
            // If you want to require non-zero values, that should be handled by McpRange
            return result;
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            try
            {
                // Convert range boundaries to float
                var min = Convert.ToSingle(rangeAttr.Min);
                var max = Convert.ToSingle(rangeAttr.Max);

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
                    Message = $"Could not convert range boundaries to float: {ex.Message}",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }
    }
}
