using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class McpTypeUsageAttribute : Attribute
    {
        public string? Input { get; set; }
        public string? Output { get; set; }
        public string? General { get; set; }
        
        /// <summary>
        /// Constructor for general usage (backwards compatibility)
        /// </summary>
        public McpTypeUsageAttribute(string general)
        {
            General = general;
        }
        
        /// <summary>
        /// Constructor for Input/Output specific usage
        /// </summary>
        public McpTypeUsageAttribute()
        {
        }
        
        /// <summary>
        /// Gets the appropriate usage for the given context
        /// </summary>
        public string GetUsage(bool isInput)
        {
            if (isInput && !string.IsNullOrEmpty(Input))
                return Input;
            if (!isInput && !string.IsNullOrEmpty(Output))
                return Output;
            return General ?? "";
        }
    }
}
