using System;
using System.Threading.Tasks;

namespace McpFramework.McpTypes
{
    public abstract class McpTypedValue<T> : McpValue, IEquatable<McpTypedValue<T>>
    {
        public T Value { get; protected set; }
        
        // Parameterless constructor for JSON deserialization
        protected McpTypedValue()
        {
            Value = default(T)!;
        }
        
        protected McpTypedValue(T value)
        {
            Value = value;
        }
        
        // Abstract methods that derived types must implement
        public abstract override McpValidationResult ValidateFormat(string parameterName, string toolName);
        public abstract override McpValidationResult ValidateRequired(string parameterName, string toolName);
        
        // Abstract method for existence validation - derived types must implement
        public abstract override Task<McpValidationResult> ValidateExistenceAsync(string parameterName, string toolName);
        
        // Common equality
        public override bool Equals(McpValue? other) => other is McpTypedValue<T> typed && Equals(typed);
        public bool Equals(McpTypedValue<T>? other) => other != null && (Value?.Equals(other.Value) ?? false);
        
        // Implicit conversion for API serialization
        public static implicit operator T(McpTypedValue<T> typedValue) => typedValue.Value;
        
        public override string ToString() => Value?.ToString() ?? string.Empty;
    }
}
