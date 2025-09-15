using McpFramework;
using McpFramework.Attributes;

namespace McpFramework.Tests;

public class McpToolDiscoveryProcessorTests
{
    [Fact]
    public void DiscoverTools_WithEnhancedAttributes_ShouldIncludeCategoryOperationTypeAndExamples()
    {
        // Arrange
        var processor = new McpToolDiscoveryProcessor();
        var testController = new TestControllerWithEnhancedAttributes();

        // Act
        var result = processor.DiscoverTools(testController);

        // Assert
        Assert.Single(result.Tools);
        
        var tool = result.Tools.First();
        Assert.Equal("enhanced-test-tool", tool.Name);
        Assert.Equal("A test tool with enhanced attributes", tool.Description);
        Assert.Equal("Testing", tool.Category);
        Assert.Equal("Read", tool.OperationType);
        Assert.Equal(2, tool.Examples.Count);
        Assert.Contains("Example with basic usage", tool.Examples);
        Assert.Contains("Example with advanced filtering", tool.Examples);
    }

    [Fact]
    public void DiscoverTools_WithoutEnhancedAttributes_ShouldUseDefaults()
    {
        // Arrange
        var processor = new McpToolDiscoveryProcessor();
        var testController = new TestControllerWithBasicAttributes();

        // Act
        var result = processor.DiscoverTools(testController);

        // Assert
        Assert.Single(result.Tools);
        
        var tool = result.Tools.First();
        Assert.Equal("basic-test-tool", tool.Name);
        Assert.Equal("A basic test tool", tool.Description);
        Assert.Equal("General", tool.Category); // Default category
        Assert.Equal("Read", tool.OperationType); // Default operation type
        Assert.Empty(tool.Examples); // No examples provided
    }

    [Fact]
    public void DiscoverTools_MultipleTools_ShouldDiscoverAllWithCorrectAttributes()
    {
        // Arrange
        var processor = new McpToolDiscoveryProcessor();
        var testController = new TestControllerWithMultipleTools();

        // Act
        var result = processor.DiscoverTools(testController);

        // Assert
        Assert.Equal(3, result.Tools.Count);
        
        // Test Entity Management tool
        var entityTool = result.Tools.First(t => t.Name == "entity-tool");
        Assert.Equal("Entity Management", entityTool.Category);
        Assert.Equal("Identify", entityTool.OperationType);
        Assert.Single(entityTool.Examples);

        // Test Memory Search tool
        var searchTool = result.Tools.First(t => t.Name == "search-tool");
        Assert.Equal("Memory Search", searchTool.Category);
        Assert.Equal("Search", searchTool.OperationType);
        Assert.Equal(2, searchTool.Examples.Count);

        // Test Relationship tool
        var relationTool = result.Tools.First(t => t.Name == "relation-tool");
        Assert.Equal("Relationship Management", relationTool.Category);
        Assert.Equal("Navigate", relationTool.OperationType);
        Assert.Empty(relationTool.Examples);
    }
}

// Test controller classes for testing
public class TestControllerWithEnhancedAttributes
{
    [McpTool("enhanced-test-tool", "A test tool with enhanced attributes",
        Category = McpToolCategories.Testing,
        OperationType = McpOperationTypes.Read,
        Examples = new[] { "Example with basic usage", "Example with advanced filtering" })]
    public string EnhancedTestMethod()
    {
        return "test";
    }
}

public class TestControllerWithBasicAttributes
{
    [McpTool("basic-test-tool", "A basic test tool")]
    public string BasicTestMethod()
    {
        return "test";
    }
}

public class TestControllerWithMultipleTools
{
    [McpTool("entity-tool", "Tool for entity management",
        Category = McpToolCategories.EntityManagement,
        OperationType = McpOperationTypes.Identify,
        Examples = new[] { "Find person by name" })]
    public string EntityMethod() => "entity";

    [McpTool("search-tool", "Tool for memory search",
        Category = McpToolCategories.MemorySearch,
        OperationType = McpOperationTypes.Search,
        Examples = new[] { "Search expressions by content", "Search by entity relationships" })]
    public string SearchMethod() => "search";

    [McpTool("relation-tool", "Tool for relationship management",
        Category = McpToolCategories.RelationshipManagement,
        OperationType = McpOperationTypes.Navigate)]
    public string RelationMethod() => "relation";
}