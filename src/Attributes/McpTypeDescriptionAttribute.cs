using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class McpTypeDescriptionAttribute : Attribute
    {
        public string Description { get; }
        
        public McpTypeDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
