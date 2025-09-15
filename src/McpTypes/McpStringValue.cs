using System;
using System.Threading.Tasks;
using McpFramework.Attributes;

namespace McpFramework.McpTypes
{
    public abstract class McpStringValue : McpTypedValue<string>
    {
        // Parameterless constructor for JSON deserialization
        protected McpStringValue() : base() { }
        
        protected McpStringValue(string value) : base(value) { }

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            // Most string values should not be null/empty, but allow override for specific cases
            if (string.IsNullOrWhiteSpace(Value))
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "INVALID_STRING_FORMAT",
                    Message = $"{parameterName} cannot be null or whitespace",
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
            
            if (string.IsNullOrEmpty(Value))
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "REQUIRED",
                    Message = $"{parameterName} is required",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }
            
            return result;
        }

        public override Task<McpValidationResult> ValidateExistenceAsync(string parameterName, string toolName)
        {
            // Most string values don't need existence validation
            // Override in derived classes if needed
            return Task.FromResult(new McpValidationResult { IsValid = true });
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };

            try
            {
                var length = Value?.Length ?? 0;
                var min = Convert.ToInt32(rangeAttr.Min);
                var max = Convert.ToInt32(rangeAttr.Max);

                bool isInRange;
                if (rangeAttr.Inclusive)
                {
                    isInRange = length >= min && length <= max;
                }
                else
                {
                    isInRange = length > min && length < max;
                }

                if (!isInRange)
                {
                    var inclusiveText = rangeAttr.Inclusive ? "inclusive" : "exclusive";
                    var defaultMessage = $"{parameterName} length must be between {min} and {max} characters ({inclusiveText}). Current length: {length}";
                    
                    result.Errors.Add(new McpValidationError
                    {
                        ParameterName = parameterName,
                        ErrorCode = "STRING_LENGTH_OUT_OF_RANGE",
                        Message = !string.IsNullOrEmpty(rangeAttr.CustomMessage) ? rangeAttr.CustomMessage : defaultMessage,
                        Value = Value,
                        ToolName = toolName
                    });
                    result.IsValid = false;

                    // Add suggestion for corrected value
                    if (length < min)
                    {
                        result.Suggestions.Add(new McpValidationSuggestion
                        {
                            ParameterName = parameterName,
                            Suggestion = $"String is too short. Minimum length is {min} characters",
                            Example = Value?.PadRight(min, '_') ?? new string('_', min)
                        });
                    }
                    else if (length > max)
                    {
                        result.Suggestions.Add(new McpValidationSuggestion
                        {
                            ParameterName = parameterName,
                            Suggestion = $"String is too long. Maximum length is {max} characters",
                            Example = Value?.Substring(0, max) ?? ""
                        });
                    }
                    else if (!rangeAttr.Inclusive && min == max)
                    {
                        // Special case: exclusive range where min == max means no valid length exists
                        result.Suggestions.Add(new McpValidationSuggestion
                        {
                            ParameterName = parameterName,
                            Suggestion = $"Exclusive range [{min},{max}] allows no valid length. Consider using inclusive range or different bounds.",
                            Example = min > 0 ? new string('_', min - 1) : new string('_', min + 1)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "STRING_LENGTH_VALIDATION_ERROR",
                    Message = $"Error validating string length for {parameterName}: {ex.Message}",
                    Value = Value,
                    ToolName = toolName
                });
                result.IsValid = false;
            }

            return result;
        }

        public override bool Equals(McpValue? other) => other is McpStringValue typed && Value.Equals(typed.Value);
    }
}
