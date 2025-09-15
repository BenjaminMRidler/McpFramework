namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class McpExistsAttribute : Attribute
    {
        public string EntityType { get; set; } = string.Empty;
        public string CustomMessage { get; set; } = string.Empty;
        
        public McpExistsAttribute(string entityType = "", string customMessage = "")
        {
            EntityType = entityType;
            CustomMessage = customMessage;
        }
    }
}
