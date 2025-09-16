using System;
using System.Threading.Tasks;
using McpFramework.Attributes;

namespace McpFramework.McpTypes
{
    [McpTypeDescription("DateTime value with MCP validation and metadata")]
    [McpTypePurpose(
        Input = "Used for date and time parameters requiring validation",
        Output = "Represents validated DateTime values in responses"
    )]
    [McpTypeUsage(
        Input = "Use UTC DateTime for consistency. Supports date range validation",
        Output = "Provides validated DateTime values with proper formatting"
    )]
    public class McpDateTime : McpTypedValue<DateTime?>
    {
        public McpDateTime(DateTime? value) : base(value) { }
        public McpDateTime() : base() { }

        // Implicit conversion from DateTime for convenience
        public static implicit operator McpDateTime(DateTime? value) => new McpDateTime(value);

        // Implicit conversion to DateTime for service calls
        public static implicit operator DateTime?(McpDateTime mcpDateTime) => mcpDateTime.Value ?? null;

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            // Check for default/uninitialized DateTime
            if (Value == default(DateTime))
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "INVALID_DATETIME",
                    Message = $"{parameterName} cannot be the default DateTime value",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }

        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            if (Value == default(DateTime))
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "DATETIME_REQUIRED",
                    Message = $"{parameterName} is required and cannot be the default DateTime value",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            try
            {
                // Convert range boundaries to DateTime
                var min = Convert.ToDateTime(rangeAttr.Min);
                var max = Convert.ToDateTime(rangeAttr.Max);

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
                    var defaultMessage = $"{parameterName} must be between {min:yyyy-MM-dd HH:mm:ss} and {max:yyyy-MM-dd HH:mm:ss} ({inclusiveText}). Current value: {Value:yyyy-MM-dd HH:mm:ss}";
                    
                    result.Errors.Add(new McpValidationError
                    {
                        ParameterName = parameterName,
                        ErrorCode = "DATETIME_OUT_OF_RANGE",
                        Message = !string.IsNullOrEmpty(rangeAttr.CustomMessage) ? rangeAttr.CustomMessage : defaultMessage,
                        Value = Value,
                        ToolName = toolName
                    });
                    result.IsValid = false;

                    // Add suggestion for corrected value
                    result.Suggestions.Add(new McpValidationSuggestion
                    {
                        ParameterName = parameterName,
                        Suggestion = $"Consider using a date between {min:yyyy-MM-dd} and {max:yyyy-MM-dd} ({inclusiveText})",
                        Example = (Value < min ? min : max).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "DATETIME_RANGE_CONVERSION_ERROR",
                    Message = $"Could not convert range boundaries to DateTime: {ex.Message}",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }
    }
}
