namespace McpFramework.Attributes
{
    /// <summary>
    /// Standard domain-based categories for MCP tools organized by business functionality
    /// </summary>
    public static class McpToolCategories
    {
        /// <summary>
        /// Tools for finding, identifying, and managing entities
        /// </summary>
        public const string EntityManagement = "Entity Management";
        
        /// <summary>
        /// Tools for searching expressions, topics, and memory content
        /// </summary>
        public const string MemorySearch = "Memory Search";
        
        /// <summary>
        /// Tools for browsing hierarchies, relationships, and memory navigation
        /// </summary>
        public const string MemoryNavigation = "Memory Navigation";
        
        /// <summary>
        /// Tools for analyzing patterns, metrics, and data insights
        /// </summary>
        public const string DataAnalysis = "Data Analysis";
        
        /// <summary>
        /// Tools for managing relationships between entities
        /// </summary>
        public const string RelationshipManagement = "Relationship Management";
        
        /// <summary>
        /// Tools for communication and session management
        /// </summary>
        public const string CommunicationManagement = "Communication Management";
        
        /// <summary>
        /// Tools for testing and framework validation
        /// </summary>
        public const string Testing = "Testing";
    }
    
    /// <summary>
    /// Standard CRUD-based operation types for MCP tools organized by action performed
    /// </summary>
    public static class McpOperationTypes
    {
        /// <summary>
        /// Operations that create new data or entities
        /// </summary>
        public const string Create = "Create";
        
        /// <summary>
        /// Operations that read, retrieve, or search existing data
        /// </summary>
        public const string Read = "Read";
        
        /// <summary>
        /// Operations that update or modify existing data
        /// </summary>
        public const string Update = "Update";
        
        /// <summary>
        /// Operations that delete or remove data
        /// </summary>
        public const string Delete = "Delete";
        
        /// <summary>
        /// Operations that search, query, or find data with filters
        /// </summary>
        public const string Search = "Search";
        
        /// <summary>
        /// Operations that analyze, process, or compute insights
        /// </summary>
        public const string Analyze = "Analyze";
        
        /// <summary>
        /// Operations that navigate, browse, or explore relationships
        /// </summary>
        public const string Navigate = "Navigate";
        
        /// <summary>
        /// Operations that identify, match, or resolve entities
        /// </summary>
        public const string Identify = "Identify";
    }
}
