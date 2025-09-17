namespace McpFramework;
public class McpMetaOptions
{
    // Simple key → list of strings
    public Dictionary<string, IEnumerable<string>> DomainData { get; } = new();

    // Key → nested dictionary (e.g., attributes["person"], attributes["place"])
    public Dictionary<string, Dictionary<string, IEnumerable<string>>> ComplexDomainData { get; } = new();
}
