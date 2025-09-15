using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class McpGlossaryRelationshipAttribute : Attribute
    {
        public string FromConcept { get; }
        public string ToConcept { get; }
        public string Description { get; }
        
        public McpGlossaryRelationshipAttribute(string fromConcept, string toConcept, string description)
        {
            FromConcept = fromConcept;
            ToConcept = toConcept;
            Description = description;
        }
    }
}

