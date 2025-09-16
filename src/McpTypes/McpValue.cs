using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McpFramework.Attributes;

namespace McpFramework.McpTypes
{
    public abstract class McpValue : IEquatable<McpValue>
    {
        // Abstract methods that all MCP values must implement  
        public abstract McpValidationResult ValidateFormat(string parameterName, string toolName);
        public abstract McpValidationResult ValidateRequired(string parameterName, string toolName);
        public abstract McpValidationResult ValidateRange(McpRangeAttribute rangeAttr, string parameterName, string toolName);
        
        // Common equality interface
        public abstract bool Equals(McpValue? other);
    }
}