using System;

namespace McpFramework.Attributes
{
    /// <summary>
    /// Property-level attribute to provide or modify the usage guidance for a specific property
    /// Allows override, prepend, or append to the base Altu McpType usage
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class McpPropertyUsageAttribute : Attribute
    {
        /// <summary>
        /// The property-specific usage guidance text
        /// </summary>
        public string Usage { get; }
        
        /// <summary>
        /// How to combine this usage guidance with the Altu McpType usage
        /// </summary>
        public MetadataMode Mode { get; set; }
        
        /// <summary>
        /// Create a property usage attribute
        /// </summary>
        /// <param name="usage">Property-specific usage guidance</param>
        /// <param name="mode">How to combine with type-level usage (default: Override)</param>
        public McpPropertyUsageAttribute(string usage, MetadataMode mode = MetadataMode.Override)
        {
            Usage = usage ?? throw new ArgumentNullException(nameof(usage));
            Mode = mode;
        }
    }
}
