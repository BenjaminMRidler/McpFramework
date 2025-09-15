using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class McpGlossaryNoteAttribute : Attribute
    {
        public string Note { get; }
        
        public McpGlossaryNoteAttribute(string note)
        {
            Note = note;
        }
    }
}

