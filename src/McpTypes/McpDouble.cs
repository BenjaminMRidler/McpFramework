using McpFramework.Attributes;
using System;
using System.Threading.Tasks;

namespace McpFramework.McpTypes
{
    [McpTypeDescription("Double precision floating-point number with validation")]
    [McpTypePurpose(
        Input = "Used for numeric parameters requiring double precision",
        Output = "Represents double precision numeric values in responses"
    )]
    [McpTypeUsage(
        Input = "Use for decimal numbers that require double precision (e.g., scores, thresholds, measurements)",
        Output = "Provides double precision numeric values with validation"
    )]
    public class McpDouble : McpTypedValue<double>
    {
        public McpDouble(double value) : base(value) { }
        public McpDouble() : base(0.0) { }

        // Implicit conversion from double for convenience
        public static implicit operator McpDouble(double value) => new McpDouble(value);

        // Implicit conversion to double for service calls
        public static implicit operator double(McpDouble mcpDouble) => mcpDouble.Value;

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            // Check for NaN or Infinity
            if (double.IsNaN(Value))
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "INVALID_DOUBLE_NAN",
                    Message = $"{parameterName} cannot be NaN",
                    Value = Value.ToString(),
                    ToolName = toolName
                });
                result.IsValid = false;
            }
            else if (double.IsInfinity(Value))
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "INVALID_DOUBLE_INFINITY",
                    Message = $"{parameterName} cannot be Infinity",
                    Value = Value.ToString(),
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }

        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // For doubles, we consider 0 as a valid value, so no required validation needed
            // unless there's a specific business rule that 0 is not allowed
            return result;
        }

        public override Task<McpValidationResult> ValidateExistenceAsync(string parameterName, string toolName)
        {
            // Doubles don't need existence validation - they're mathematical values
            return Task.FromResult(new McpValidationResult { IsValid = true });
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            try
            {
                // Convert range boundaries to double
                var min = Convert.ToDouble(rangeAttr.Min);
                var max = Convert.ToDouble(rangeAttr.Max);

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
                    var defaultMessage = $"{parameterName} must be between {min:F4} and {max:F4} ({inclusiveText}). Current value: {Value:F4}";
                    
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
                        Suggestion = $"Consider using a value between {min:F4} and {max:F4} ({inclusiveText})",
                        Example = (Value < min ? min : max).ToString("F4")
                    });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "RANGE_CONVERSION_ERROR",
                    Message = $"Could not convert range boundaries to double: {ex.Message}",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }
    }
}
