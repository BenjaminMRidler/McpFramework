# McpFramework

A powerful, extensible framework for building Model Context Protocol (MCP) tools in .NET applications. McpFramework provides a comprehensive set of primitives, attributes, and validation processors to create robust, well-documented MCP tools with minimal boilerplate code.

## 🚀 Features

- **Rich Type System**: Built-in MCP primitive types with validation (McpInt, McpFloat, McpString, McpGuid, etc.)
- **Declarative Attributes**: Simple attribute-based tool and validation configuration
- **Automatic Tool Discovery**: Reflection-based tool discovery and metadata generation
- **Comprehensive Validation**: Range validation, format validation, required field validation
- **Extensible Architecture**: Easy to extend with custom types and validation logic
- **Production Ready**: 41 comprehensive tests with 100% pass rate

## 📦 Installation

### As Git Submodule (Recommended for Development)
```bash
git submodule add <mcpframework-repo-url> McpFramework
git submodule update --init --recursive
```

### As NuGet Package (Future)
```bash
dotnet add package McpFramework
```

## 🏗️ Architecture

```
McpFramework/
├── McpTypes/           # Primitive types (McpInt, McpString, etc.)
├── Attributes/         # MCP attributes for tools and validation
├── Processors/         # Tool discovery and validation processors
└── Core/              # Base interfaces and result types
```

## 🛠️ Quick Start

### 1. Define Your MCP Tool

```csharp
using McpFramework;
using McpFramework.Attributes;
using McpFramework.McpTypes;

public class SearchController
{
    [McpTool("memory-search", "Search through entity memories and expressions",
        Category = McpToolCategories.MemorySearch,
        OperationType = McpOperationTypes.Search,
        Examples = new[] { 
            "Search for memories about John Doe",
            "Find expressions containing 'project meeting'"
        })]
    public async Task<SearchResponse> SearchMemories(SearchRequest request)
    {
        // Your search implementation
        return new SearchResponse();
    }
}

public class SearchRequest : IMcpRequest
{
    [McpRequired]
    [McpRange(1, 100)]
    public McpStringValue Query { get; set; } = new();

    [McpRange(1, 50)]
    public McpInt MaxResults { get; set; } = new(10);

    [McpRange(0.0, 1.0)]
    public McpFloat ConfidenceThreshold { get; set; } = new(0.7f);
}
```

### 2. Discover and Validate Tools

```csharp
using McpFramework;

// Tool Discovery
var discoveryProcessor = new McpToolDiscoveryProcessor();
var toolsResult = discoveryProcessor.DiscoverTools(new SearchController());

// Request Validation
var validationProcessor = new McpValidationProcessor();
var searchRequest = new SearchRequest 
{ 
    Query = new McpStringValue("memories about John"),
    MaxResults = new McpInt(25)
};

var validationResult = await validationProcessor.ValidateObjectAsync(searchRequest, "memory-search");
if (!validationResult.IsValid)
{
    // Handle validation errors
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"{error.ParameterName}: {error.Message}");
    }
}
```

## 📊 MCP Types

### Primitive Types
| Type | Purpose | Validation Features |
|------|---------|-------------------|
| `McpInt` | Integer values | Range validation, boundary checks |
| `McpFloat` | Single-precision floats | Range validation with precision |
| `McpDouble` | Double-precision floats | Range validation with precision |
| `McpDecimal` | High-precision decimals | Range validation for financial data |
| `McpDateTime` | Date and time values | Date range validation |
| `McpStringValue` | Text content (abstract) | Length validation, format checks |
| `McpGuidValue` | Unique identifiers (abstract) | Format validation, existence checks |

### Creating Custom Types

```csharp
public class EmailAddress : McpStringValue
{
    public EmailAddress(string email) : base(email) { }
    public EmailAddress() : base() { }

    public override McpValidationResult ValidateFormat(string parameterName, string toolName)
    {
        var result = new McpValidationResult { IsValid = true };
        
        if (!IsValidEmail(Value))
        {
            result.Errors.Add(new McpValidationError
            {
                ParameterName = parameterName,
                ErrorCode = "INVALID_EMAIL_FORMAT",
                Message = $"{parameterName} must be a valid email address",
                Value = Value,
                ToolName = toolName
            });
            result.IsValid = false;
        }
        
        return result;
    }

    private bool IsValidEmail(string email)
    {
        // Email validation logic
        return email?.Contains("@") == true;
    }
}
```

## 🏷️ Attributes

### Tool Definition
```csharp
[McpTool("tool-name", "Tool description",
    Category = McpToolCategories.MemorySearch,    // Categorize your tool
    OperationType = McpOperationTypes.Search,     // Define operation type
    Examples = new[] { "Usage example 1", "Usage example 2" })]
```

### Validation Attributes
```csharp
[McpRequired]                                     // Required field
[McpRange(1, 100)]                               // Inclusive range
[McpRange(1, 100, false)]                        // Exclusive range
[McpRange(1, 100, true, "Custom error message")] // With custom message
```

### Documentation Attributes
```csharp
[McpTypeDescription("Describes what this type represents")]
[McpTypePurpose(Input = "Input usage", Output = "Output usage")]
[McpPropertyDescription("Describes this property")]
```

## 🔧 Tool Categories

The framework provides standard categories for organizing MCP tools:

```csharp
public static class McpToolCategories
{
    public const string EntityManagement = "Entity Management";
    public const string MemorySearch = "Memory Search";
    public const string RelationshipManagement = "Relationship Management";
    public const string SessionManagement = "Session Management";
    public const string TopicManagement = "Topic Management";
    public const string ExpressionManagement = "Expression Management";
    public const string Testing = "Testing";
    public const string General = "General";
}
```

## ⚡ Operation Types

Standard operation types for MCP tools:

```csharp
public static class McpOperationTypes
{
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Search = "Search";
    public const string Navigate = "Navigate";
    public const string Identify = "Identify";
}
```

## 🧪 Testing

The framework includes comprehensive test support:

```csharp
[Fact]
public async Task ValidateRequest_WithValidData_ShouldPass()
{
    // Arrange
    var processor = new McpValidationProcessor();
    var request = new SearchRequest
    {
        Query = new TestStringValue("valid query"),
        MaxResults = new McpInt(25)
    };

    // Act
    var result = await processor.ValidateObjectAsync(request, "test-tool");

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}
```

### Test Helpers

Create concrete implementations for testing:

```csharp
public class TestStringValue : McpStringValue
{
    public TestStringValue(string value) : base(value) { }
    public TestStringValue() : base() { }

    public override McpValidationResult ValidateFormat(string parameterName, string toolName)
    {
        return new McpValidationResult { IsValid = true };
    }

    public override McpValidationResult ValidateRequired(string parameterName, string toolName)
    {
        return new McpValidationResult { IsValid = true };
    }
}
```

## 📈 Validation Results

The framework provides detailed validation feedback:

```csharp
public class McpValidationResult
{
    public bool IsValid { get; set; }
    public List<McpValidationError> Errors { get; set; } = new();
    public List<McpValidationSuggestion> Suggestions { get; set; } = new();
}
```

### Error Codes
- `OUT_OF_RANGE` - Value outside allowed range
- `STRING_LENGTH_OUT_OF_RANGE` - String length validation failure
- `DATETIME_OUT_OF_RANGE` - DateTime outside allowed range
- `INVALID_GUID` - Invalid GUID format
- `REQUIRED` - Required field missing
- `RANGE_NOT_APPLICABLE` - Range validation not supported for type

## 🔍 Advanced Features

### Custom Validation Logic

```csharp
public class CustomProcessor : McpValidationProcessor
{
    public override async Task<McpValidationResult> ValidateObjectAsync(object obj, string toolName)
    {
        var result = await base.ValidateObjectAsync(obj, toolName);
        
        // Add custom validation logic
        if (obj is SearchRequest request && request.Query.Value.Contains("sensitive"))
        {
            result.Errors.Add(new McpValidationError
            {
                ErrorCode = "SENSITIVE_CONTENT",
                Message = "Query contains sensitive content",
                ParameterName = nameof(request.Query)
            });
            result.IsValid = false;
        }
        
        return result;
    }
}
```

### Metadata Processing

```csharp
var metadataProcessor = new McpMetadataProcessor();
var metadata = metadataProcessor.ExtractTypeMetadata(typeof(SearchRequest));

Console.WriteLine($"Type: {metadata.TypeName}");
Console.WriteLine($"Description: {metadata.Description}");
foreach (var property in metadata.Properties)
{
    Console.WriteLine($"  {property.Name}: {property.Description}");
}
```

## 🏛️ Framework Architecture

### Core Components

1. **McpValue**: Abstract base class for all MCP types
2. **McpTypedValue<T>**: Generic base for typed values
3. **McpValidationProcessor**: Handles request validation
4. **McpToolDiscoveryProcessor**: Discovers and catalogs MCP tools
5. **McpMetadataProcessor**: Extracts type and property metadata

### Design Principles

- **Type Safety**: Strong typing with compile-time checks
- **Extensibility**: Easy to extend with custom types and logic
- **Declarative**: Attribute-based configuration
- **Testability**: Comprehensive test support and helpers
- **Performance**: Efficient reflection-based processing
- **Documentation**: Self-documenting through attributes

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes with tests
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Development Requirements

- .NET 9.0 or later
- Visual Studio 2022 or JetBrains Rider
- Git

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Build framework
dotnet build src/McpFramework.csproj
```

## 📋 Changelog

### [1.0.0] - 2025-09-15
- **Added**: Complete MCP Framework extraction from Altu
- **Added**: Comprehensive type system with validation
- **Added**: Tool discovery and metadata processing
- **Added**: 41 comprehensive tests with 100% pass rate
- **Added**: Professional documentation and examples
- **Changed**: Namespace from `McpFramework` to `McpFramework`
- **Fixed**: Edge case handling in string range validation
- **Improved**: Error messaging and validation suggestions

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Check the [docs](docs/) folder for detailed guides
- **Issues**: Report bugs and request features via GitHub Issues
- **Discussions**: Join community discussions in GitHub Discussions

## 🔗 Related Projects

- **Altu**: AI Partner system that uses McpFramework for MCP tool implementation
- **Model Context Protocol**: The protocol specification this framework implements

---

Built with ❤️ for the MCP community