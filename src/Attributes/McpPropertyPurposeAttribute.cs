using System;

namespace McpFramework.Attributes
{
    /// <summary>
    /// Property-level attribute to provide or modify the purpose for a specific property
    /// Allows override, prepend, or append to the base Altu McpType purpose
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class McpPropertyPurposeAttribute : Attribute
    {
        /// <summary>
        /// The property-specific input purpose
        /// </summary>
        public string? Input { get; set; }
        
        /// <summary>
        /// The property-specific output purpose
        /// </summary>
        public string? Output { get; set; }
        
        /// <summary>
        /// How to combine input purpose with the Altu McpType input purpose
        /// </summary>
        public MetadataMode InputMode { get; set; }
        
        /// <summary>
        /// How to combine output purpose with the Altu McpType output purpose
        /// </summary>
        public MetadataMode OutputMode { get; set; }
        
        /// <summary>
        /// Create a property purpose attribute
        /// </summary>
        /// <param name="input">Property-specific input purpose</param>
        /// <param name="output">Property-specific output purpose</param>
        /// <param name="inputMode">How to combine with type-level input purpose (default: Override)</param>
        /// <param name="outputMode">How to combine with type-level output purpose (default: Override)</param>
        public McpPropertyPurposeAttribute(string? input = null, string? output = null, 
            MetadataMode inputMode = MetadataMode.Override, MetadataMode outputMode = MetadataMode.Override)
        {
            Input = input;
            Output = output;
            InputMode = inputMode;
            OutputMode = outputMode;
        }
    }
}
