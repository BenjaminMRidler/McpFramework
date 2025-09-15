using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class McpDomainDataAttribute : Attribute
    {
        public string DataType { get; set; } = string.Empty;
        
        public McpDomainDataAttribute(string dataType = "")
        {
            DataType = dataType;
        }
    }
}

