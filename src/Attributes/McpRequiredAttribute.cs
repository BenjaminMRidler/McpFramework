namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class McpRequiredAttribute : Attribute
    {
        public string CustomMessage { get; set; } = string.Empty;
        
        public McpRequiredAttribute(string customMessage = "")
        {
            CustomMessage = customMessage;
        }
    }
}
