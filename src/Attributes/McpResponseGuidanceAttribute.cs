using System;

namespace McpFramework.Attributes;

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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BestPracticeAttribute : McpResponseGuidanceAttribute
    {
        public BestPracticeAttribute(string message, params string[] examples) : base("Best Practices", message)
        {
            Examples = examples;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CommonPatternAttribute : McpResponseGuidanceAttribute
    {
        public CommonPatternAttribute(string message, params string[] examples) : base("Common Patterns", message)
        {
            Examples = examples;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InterpretationAttribute : McpResponseGuidanceAttribute
    {
        public InterpretationAttribute(string message, params string[] examples) : base("Interpretation", message)
        {
            Examples = examples;
        }
    }

    public class TipAttribute : McpResponseGuidanceAttribute
    {
        public TipAttribute(string message, params string[] examples) : base("Tip", message)
        {
            Examples = examples;
            Priority = 2;
        }
    }


    public class UsageAttribute : McpResponseGuidanceAttribute
    {
        public UsageAttribute(string message, params string[] examples) : base("Usage", message)
        {
            Examples = examples;
        }
    }

    public class WarningAttribute : McpResponseGuidanceAttribute
    {
        public WarningAttribute(string message, params string[] examples) : base("Warning", message)
        {
            Examples = examples;
            Priority = 5;
        }
    }
}
