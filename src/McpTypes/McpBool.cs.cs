using McpFramework.Attributes;
using System;

namespace McpFramework.McpTypes
{
    [McpTypeDescription("Boolean value with MCP validation and metadata")]
    [McpTypePurpose(
        Input = "Used for flags and toggle parameters requiring true/false values",
        Output = "Returns validated boolean values with context"
    )]
    [McpTypeUsage(
        Input = "Use for on/off switches, binary states, or feature flags",
        Output = "Provides validated boolean results with proper typing"
    )]
    public class McpBool : McpTypedValue<bool>
    {
        public McpBool(bool value) : base(value) { }
        public McpBool() : base(false) { }

        // Implicit conversion from bool for convenience
        public static implicit operator McpBool(bool value) => new McpBool(value);

        // Implicit conversion to bool for service calls
        public static implicit operator bool(McpBool mcpBool) => mcpBool.Value;

        public override McpValidationResult ValidateFormat(string parameterName, string toolName)
        {
            // Booleans are always valid if deserialized correctly
            return new McpValidationResult { IsValid = true };
        }

        public override McpValidationResult ValidateRequired(string parameterName, string toolName)
        {
            // Both true and false are considered valid; "missing" would be handled by nullability
            return new McpValidationResult { IsValid = true };
        }

        public override McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName)
        {
            // Ranges don’t apply to bools, so we’ll treat this as always valid
            return new McpValidationResult { IsValid = true };
        }
    }
}
