namespace McpFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class McpRangeAttribute : Attribute
    {
        public object Min { get; set; }
        public object Max { get; set; }
        public bool Inclusive { get; set; } = true;
        public string CustomMessage { get; set; } = string.Empty;
        
        public McpRangeAttribute(object min, object max, bool inclusive = true, string customMessage = "")
        {
            Min = min;
            Max = max;
            Inclusive = inclusive;
            CustomMessage = customMessage;
        }
    }
}
