using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McpFramework.Attributes;

namespace McpFramework.McpTypes
{
    public abstract class McpValue : IEquatable<McpValue>
    {
        /// <summary>
        /// Virtual method for existence validation that can be overridden by derived classes.
        /// Default implementation always returns valid (no existence checking).
        /// </summary>
        public virtual async Task<McpValidationResult> ValidateExistenceAsync(string parameterName, string toolName)
        {
            // Default implementation - no existence checking
            // Derived classes can override this to provide specific existence validation
            return await Task.FromResult(new McpValidationResult { IsValid = true });
        }

        // Abstract methods that all MCP values must implement  
        public abstract McpValidationResult ValidateFormat(string parameterName, string toolName);
        public abstract McpValidationResult ValidateRequired(string parameterName, string toolName);
        public abstract McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName);
        
        // Common equality interface
        public abstract bool Equals(McpValue? other);
    }
}