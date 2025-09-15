namespace McpFramework.Attributes
{
    /// <summary>
    /// Defines how property-level attribute metadata should be combined with type-level metadata
    /// </summary>
    public enum MetadataMode
    {
        /// <summary>
        /// Replace the Altu McpType metadata entirely with property-specific metadata
        /// </summary>
        Override,
        
        /// <summary>
        /// Add property-specific metadata before the Altu McpType metadata
        /// </summary>
        Prepend,
        
        /// <summary>
        /// Add property-specific metadata after the Altu McpType metadata
        /// </summary>
        Append
    }
}
