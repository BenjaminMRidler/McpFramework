using System;

namespace McpFramework.Attributes
{
    /// <summary>
    /// Property-level attribute to provide or modify the description for a specific property
    /// Allows override, prepend, or append to the base Altu McpType description
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class McpPropertyDescriptionAttribute : Attribute
    {
        /// <summary>
        /// The property-specific description text
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// How to combine this description with the Altu McpType description
        /// </summary>
        public MetadataMode Mode { get; set; }
        
        /// <summary>
        /// Create a property description attribute
        /// </summary>
        /// <param name="description">Property-specific description</param>
        /// <param name="mode">How to combine with type-level description (default: Override)</param>
        public McpPropertyDescriptionAttribute(string description, MetadataMode mode = MetadataMode.Override)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Mode = mode;
        }
    }
}
