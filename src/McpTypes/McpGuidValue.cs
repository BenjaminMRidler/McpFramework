using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McpFramework.Attributes;

namespace McpFramework.McpTypes
{
    public abstract class McpGuidValue : McpTypedValue<Guid>
    {
        // Parameterless constructor for JSON deserialization
        protected McpGuidValue() : base() { }
        
        protected McpGuidValue(Guid value) : base(value)
        {
        }
        
        // GUID-specific format validation
        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            if (Value == Guid.Empty)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "INVALID_GUID",
                    Message = $"{parameterName} must be a valid GUID",
                    ToolName = toolName
                });
                result.IsValid = false;
            }
            
            return result;
        }
        
        // GUID-specific required validation
        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = true };
            
            if (Value == Guid.Empty)
            {
                result.Errors.Add(new McpValidationError
                {
                    ParameterName = parameterName,
                    ErrorCode = "REQUIRED",
                    Message = $"{parameterName} is required",
                    ToolName = toolName
                });
                result.IsValid = false;
            }
            
            return result;
        }

        // Range validation doesn't make sense for GUIDs
        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            var result = new McpValidationResult { IsValid = false };
            
            result.Errors.Add(new McpValidationError
            {
                ParameterName = parameterName,
                ErrorCode = "RANGE_NOT_APPLICABLE",
                Message = $"Range validation is not applicable for GUID values. GUIDs are unique identifiers and cannot be compared using ranges.",
                Value = Value,
                ToolName = toolName
            });
            
            return result;
        }
        
        // Common equality for GUID values
        public override bool Equals(McpValue? other) => other is McpGuidValue typed && Value.Equals(typed.Value);
        
        // Implicit conversion to Guid - inherited by all derived GUID types
        public static implicit operator Guid(McpGuidValue guidValue) => guidValue.Value;
    }
}
