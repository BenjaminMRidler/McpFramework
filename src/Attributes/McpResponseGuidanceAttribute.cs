using System;

namespace McpFramework.Attributes
{
    /// <summary>
    /// MCP002: Provides response guidance notes for AI agents using MCP tools
    /// Helps agents understand how to interpret and use tool responses effectively
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class McpResponseGuidanceAttribute : Attribute
    {
        /// <summary>
        /// The guidance category (e.g., "Usage", "Interpretation", "Best Practices", "Common Patterns")
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The guidance message providing context for AI agents
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Optional priority level (1-5, where 5 is highest priority)
        /// </summary>
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Optional condition when this guidance applies (e.g., "When confidence > 0.8", "For entity searches")
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Optional examples of when to apply this guidance
        /// </summary>
        public string[]? Examples { get; set; }

        public McpResponseGuidanceAttribute(string category, string message)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Creates a usage guidance note
        /// </summary>
        public static McpResponseGuidanceAttribute Usage(string message, string[]? examples = null) =>
            new("Usage", message) { Examples = examples };

        /// <summary>
        /// Creates an interpretation guidance note
        /// </summary>
        public static McpResponseGuidanceAttribute Interpretation(string message, string? condition = null) =>
            new("Interpretation", message) { Condition = condition };

        /// <summary>
        /// Creates a best practices guidance note
        /// </summary>
        public static McpResponseGuidanceAttribute BestPractice(string message, int priority = 4) =>
            new("Best Practices", message) { Priority = priority };

        /// <summary>
        /// Creates a common patterns guidance note
        /// </summary>
        public static McpResponseGuidanceAttribute CommonPattern(string message, string[]? examples = null) =>
            new("Common Patterns", message) { Examples = examples };

        /// <summary>
        /// Creates a warning guidance note
        /// </summary>
        public static McpResponseGuidanceAttribute Warning(string message, string? condition = null) =>
            new("Warning", message) { Condition = condition, Priority = 5 };

        /// <summary>
        /// Creates a tip guidance note
        /// </summary>
        public static McpResponseGuidanceAttribute Tip(string message, string[]? examples = null) =>
            new("Tip", message) { Examples = examples, Priority = 2 };
    }
}
