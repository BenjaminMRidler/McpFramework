using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class McpGlossaryHeaderAttribute : Attribute
    {
        public string Title { get; }
        public string Description { get; }
        
        public McpGlossaryHeaderAttribute(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}

