using System;
using System.Threading.Tasks;

namespace McpFramework.McpTypes
{
    public abstract class McpTypedValue<T> : McpValue, IEquatable<McpTypedValue<T>>
    {
        public T Value { get; internal set; }

        // For JSON deserialization
        protected McpTypedValue()
        {
            Value = default!;
        }

        protected McpTypedValue(T value)
        {
            Value = value;
        }

        // Derived types must provide validation
        public abstract override McpValidationResult ValidateFormat(string parameterName, string toolName);
        public abstract override McpValidationResult ValidateRequired(string parameterName, string toolName);

        // Equality
        public override bool Equals(McpValue? other) =>
            other is McpTypedValue<T> typed && Equals(typed);

        public bool Equals(McpTypedValue<T>? other) =>
            other != null && EqualityComparer<T>.Default.Equals(Value, other.Value);

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        // Implicit conversion (to T only here)
        public static implicit operator T(McpTypedValue<T> typedValue) => typedValue.Value;

        public override string ToString() => Value?.ToString() ?? string.Empty;
    }
}