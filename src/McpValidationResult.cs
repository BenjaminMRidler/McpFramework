namespace McpFramework
{
    public class McpValidationResult
    {
        public bool IsValid { get; set; }
        public List<McpValidationError> Errors { get; set; } = new();
        public List<McpValidationSuggestion> Suggestions { get; set; } = new();
    }
    
    public class McpValidationError
    {
        public string ParameterName { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public object Value { get; set; }
        public string ToolName { get; set; }
        public string? Suggestion { get; set; }
    }
    
    public class McpValidationSuggestion
    {
        public string ParameterName { get; set; }
        public string Suggestion { get; set; }
        public string Example { get; set; }
    }
}

