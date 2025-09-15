using System;

namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class McpTypePurposeAttribute : Attribute
    {
        public string? Input { get; set; }
        public string? Output { get; set; }
        public string? General { get; set; }
        
        /// <summary>
        /// Constructor for general purpose (backwards compatibility)
        /// </summary>
        public McpTypePurposeAttribute(string general)
        {
            General = general;
        }
        
        /// <summary>
        /// Constructor for Input/Output specific purposes
        /// </summary>
        public McpTypePurposeAttribute()
        {
        }
        
        /// <summary>
        /// Gets the appropriate purpose for the given context
        /// </summary>
        public string GetPurpose(bool isInput)
        {
            if (isInput && !string.IsNullOrEmpty(Input))
                return Input;
            if (!isInput && !string.IsNullOrEmpty(Output))
                return Output;
            return General ?? "";
        }
    }
}

