using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class McpGlossaryTermAttribute : Attribute
    {
        public string Term { get; }
        public string Definition { get; }
        
        public McpGlossaryTermAttribute(string term, string definition)
        {
            Term = term;
            Definition = definition;
        }
    }
}

