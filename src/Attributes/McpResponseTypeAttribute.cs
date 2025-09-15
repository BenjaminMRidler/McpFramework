using System;

namespace McpFramework.Attributes
{
    /// <summary>
    /// Specifies the response type for MCP tool documentation.
    /// Use this attribute on methods to indicate what type of object will be returned,
    /// even when the method signature returns IActionResult or Task&lt;IActionResult&gt;.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class McpResponseTypeAttribute : Attribute
    {
        public Type ResponseType { get; }
        public string? Description { get; set; }

        public McpResponseTypeAttribute(Type responseType)
        {
            ResponseType = responseType ?? throw new ArgumentNullException(nameof(responseType));
        }

        public McpResponseTypeAttribute(Type responseType, string description)
        {
            ResponseType = responseType ?? throw new ArgumentNullException(nameof(responseType));
            Description = description;
        }
    }
}
