using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class McpToolAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string[] Examples { get; set; } = Array.Empty<string>();
        public bool ValidateRequest { get; set; } = true;
        
        public McpToolAttribute(string name)
        {
            Name = name;
        }
        
        public McpToolAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
        
        public McpToolAttribute(string name, string description, bool validateRequest)
        {
            Name = name;
            Description = description;
            ValidateRequest = validateRequest;
        }
    }
}

