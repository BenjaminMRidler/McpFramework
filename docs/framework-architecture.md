# McpFramework Architecture

## Purpose
This document provides the architectural reference for the McpFramework - a reusable .NET framework for building Model Context Protocol (MCP) tools. It establishes the core principles, patterns, and conventions for framework usage.

## Overview

The **McpFramework** is an attribute-driven system that transforms .NET API endpoints into self-describing, metadata-rich MCP tools through declarative attributes and purpose-specific type systems.

### Key Value Propositions
- **Self-Describing APIs**: Attributes generate dynamic tool instructions and schemas
- **Type Safety**: Purpose-specific types replace generic primitives with meaningful domain types  
- **Validation Framework**: Declarative validation rules enforce data integrity
- **Agent Ergonomics**: Rich metadata helps AI agents understand tool purpose and usage patterns

### The Problem: Generic Types Lack Meaning
Traditional APIs use generic types (`string`, `Guid`, `int`) that don't convey their purpose or validation rules:

```csharp
// ❌ UNCLEAR PURPOSE - What do these strings represent?
public class BadSearchRequest 
{
    public string UserId { get; set; }        // Is this validated? Required? A GUID?
    public string EntityType { get; set; }   // What values are valid?
    public IEnumerable<string> Names { get; set; }  // Names of what? How are they used?
}
```

The MCP framework solves this by using purpose-specific types that clearly express their intent:

```csharp
// ✅ SEMANTIC CLARITY - Purpose and validation are explicit
public class SearchRequest : IMcpRequest
{
    [McpRequired]
    public UserIdValue UserId { get; init; } = new();          // Required, validated user ID
    
    public EntityTypeValue? EntityType { get; init; }         // Optional, controlled vocabulary
    
    public NamesCollection Names { get; init; } = new();      // OR'd collection with clear semantics
}

// Domain-specific type with validation
public class UserIdValue : McpGuidValue
{
    public UserIdValue() : base(Guid.Empty) { }
    public UserIdValue(Guid userId) : base(userId) { }

    public override McpValidationResult ValidateFormat(string parameterName, string toolName)
    {
        if (Value == Guid.Empty)
            return McpValidationResult.Error("UserId cannot be empty");
        return McpValidationResult.Success();
    }

    // Implicit conversions for service layer
    public static implicit operator UserIdValue(Guid userId) => new(userId);
    public static implicit operator Guid(UserIdValue userId) => userId.Value;
}
```

**What this gives us over `string UserId`:**
- **Self-documenting**: Clear description and purpose in attributes
- **Type safety**: Can't accidentally pass a string that's not a GUID
- **Validation**: Automatic format and existence checking
- **Tool discovery**: Rich metadata for AI agents
- **Consistency**: Same validation logic everywhere the type is used

---

## Core Principles

### 1. Attribute-Driven Metadata
All tool instructions, validation rules, and schema generation derive from .NET attributes applied to classes, methods, and types. This ensures metadata stays current with code changes.

```csharp
[McpTool("search_expressions_comprehensive", 
    description: "Find expressions using flexible criteria",
    category: "Memory Search")]
public async Task<ActionResult<ExpressionSearchResponse>> SearchExpressionsAsync(
    [FromBody] ExpressionSearchRequest request)
```

### 2. Type Hierarchy and Semantic Clarity
The framework uses a **strict 4-tier inheritance hierarchy** that should be followed for all MCP types:

```
McpValue (abstract base class)
    ↓ inherits from
McpTypedValue<T> (generic typed base: McpTypedValue<int>, McpTypedValue<DateTime>)
    ↓ inherits from  
Framework Primitives (McpInt, McpFloat, McpDateTime, McpStringValue, McpGuidValue)
    ↓ inherits from
Domain Types (UserIdValue, SearchTermValue, EntityIdValue)
    ↓ used in
API Request/Response Models (ExpressionSearchRequest)
```

**Critical Architectural Rule:**
- **Domain Types** inherit from **Framework Primitives** 
- **Framework Primitives** inherit from **McpTypedValue<T>**
- **McpTypedValue<T>** inherits from **McpValue**
- **System primitives** (string, int, Guid) are never used directly in MCP models

### 3. Critical Modeling Rule
> **Request/Response types MUST contain ONLY domain MCP types**  
> Never use primitives (`string`, `int`, `Guid`) or framework primitives (`McpStringValue`, `McpInt`) directly in API models. Only purpose-specific domain types carry the metadata necessary for tool discovery.

### 4. Validation Through Inheritance
All MCP types inherit validation capabilities from base classes. Each tier implements specific abstract methods:

**Validation Methods (Abstract in McpValue):**
- `ValidateFormat()` - Format-specific validation (implemented in framework primitives)
- `ValidateRequired()` - Required field validation (implemented in framework primitives)  
- `ValidateRange()` - Range/boundary validation (implemented in framework primitives)
- `ValidateExistenceAsync()` - Database existence validation (implemented per domain type)

**Range Validation Implementation:**
- **McpInt, McpFloat, McpDouble, McpDecimal**: Numeric range validation
- **McpDateTime**: Date/time range validation  
- **McpStringValue**: String length range validation
- **McpGuidValue**: Reports range validation as not applicable

```csharp
// Validation triggered by attributes
[McpRequired]           // Calls ValidateRequired() 
[McpExists("Entity")]   // Calls ValidateExistenceAsync()
[McpRange(1, 100)]     // Calls ValidateRange() with min=1, max=100
public EntityIdValue EntityId { get; init; } = new();
```

---

## Framework Components

### Base Class Hierarchy
```csharp
McpValue (abstract base)
├── McpTypedValue<T> (generic abstract base)
│   ├── McpStringValue (abstract string wrapper)
│   ├── McpGuidValue (abstract GUID wrapper)  
│   ├── McpInt, McpDouble, McpFloat, McpDecimal (concrete primitives)
│   ├── McpDateTime (concrete date/time primitive)
│   └── McpCollection<T> (abstract collection base)
```

### Processors
- **McpToolDiscoveryProcessor**: Generates tool schemas from attributes
- **McpMetadataProcessor**: Extracts type and property metadata  
- **McpValidationProcessor**: Enforces attribute-driven validation rules

### Conversion Patterns
MCP types provide implicit conversions for service layer integration:

```csharp
// API Model → Service Layer
EntityIdValue entityId = request.EntityId;
Guid serviceGuid = entityId.Value;              // Extract primitive
string[] serviceStrings = names.ToStringList(); // Extract collection

// Service Layer → API Model  
var response = new EntitySearchResponse
{
    EntityId = new EntityIdValue(serviceResult.Id),
    Names = new NamesCollection(serviceResult.Names)
};
```

---

## Attribute Framework

### Validation Attributes (Applied to properties/types)
- `[McpRequired]` - Enforce required values
- `[McpRange(min, max)]` - Constrain numeric/collection bounds
- `[McpExists("EntityType")]` - Validate database existence
- `[McpTypeUsage]` - Provide input/output usage guidance

### Metadata Attributes (Applied to types/properties)  
- `[McpTypeDescription]` - Human-readable type purpose
- `[McpTypePurpose]` - Input vs output context clarification
- `[McpResponseType]` - Declare concrete response types for IActionResult

### Property-Level Metadata Attributes (Applied to properties only)
- `[McpPropertyDescription]` - Override, prepend, or append property-specific descriptions
- `[McpPropertyUsage]` - Override, prepend, or append property-specific usage guidance
- `[McpPropertyPurpose]` - Override, prepend, or append property-specific input/output purpose

**Property-Level Composition Modes:**
- `MetadataMode.Override` - Replace the base type metadata entirely (default)
- `MetadataMode.Prepend` - Add property metadata before the base type metadata  
- `MetadataMode.Append` - Add property metadata after the base type metadata

### Tool Definition Attributes (Applied to controller methods)
- `[McpTool]` - Expose endpoint as MCP tool with name, description, category, and validation control

```csharp
// Default behavior - validation enabled
[McpTool("search_expressions", "Search expressions with validation")]
public async Task<ActionResult> SearchExpressionsAsync(SearchRequest request)

// Disable validation for special cases
[McpTool("legacy_search", "Legacy search without validation", validateRequest: false)]
public async Task<ActionResult> LegacySearchAsync(LegacyRequest request)
```

### Tool Categorization
The framework provides standard categories and operation types:

```csharp
public static class McpToolCategories
{
    public const string EntityManagement = "Entity Management";
    public const string MemorySearch = "Memory Search";
    public const string RelationshipManagement = "Relationship Management";
    public const string General = "General";
}

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

---

## Naming Conventions

### Type Naming
- **Framework Primitives**: `Mcp` + primitive type (e.g., `McpStringValue`, `McpInt`)
- **Domain Types**: descriptive domain concept + `Value` (e.g., `EntityIdValue`, `SearchTermValue`)
- **Collections**: Base name + `Collection` (e.g., `NamesCollection`, `EntityIdCollection`)
- **API Models**: operation + `Request`/`Response` (e.g., `ExpressionSearchRequest`)

### Tool Naming
- **Pattern**: `{action}_{entity}_{qualifier}` (e.g., `search_expressions_comprehensive`)
- **Consistency**: Use snake_case for tool names to match MCP protocol conventions
- **Clarity**: Tool names should be self-describing and unique

---

## Nullability Patterns

### Required vs Optional Properties
```csharp
// Required - use non-nullable MCP type
[McpRequired]
public UserIdValue UserId { get; init; } = new();

// Optional - use nullable MCP type  
public StartDateValue? StartDate { get; init; }
```

### Collection Handling
```csharp
// Empty collections are valid (OR logic within collection)
public NamesCollection Names { get; init; } = new();

// Check for meaningful content
if (request.Names.Count > 0)
{
    var serviceNames = request.Names.ToStringList();
}
```

---

## Validation Flow

```
Request received by controller
    ↓
McpValidationFilter checks for [McpTool] attribute
    ↓
If [McpTool] attribute present and ValidateRequest = true:
  Model binding creates MCP-typed request
    ↓
  McpValidationProcessor.ValidateObjectAsync()
    ↓
  For each property with validation attributes:
    - McpValue.ValidateRequired()
    - McpValue.ValidateFormat()  
    - McpValue.ValidateExistenceAsync()
    - McpValue.ValidateRange()
    ↓
  Aggregate validation results
    ↓
  If validation fails: Return BadRequest with errors
  If validation passes: Continue to controller action
```

**Key Point**: Validation is **automatically triggered** by the `[McpTool]` attribute. Any endpoint decorated with `[McpTool]` will have its request automatically validated using the MCP validation framework.

---

## Collection vs Single-Value Rules

### OR Logic Within Collections
Properties that accept collections use **OR logic** - any matching item satisfies the criteria:

```csharp
// Searches for items containing ANY of these names
public NamesCollection Names { get; init; } = new();
```

### AND Logic Between Parameters
Separate parameters use **AND logic** - all specified parameters must be satisfied:

```csharp
// Must match entity type AND at least one name AND threshold
public EntityTypeValue? EntityType { get; init; }
public NamesCollection Names { get; init; } = new();  
public RelevanceThresholdValue RelevanceThreshold { get; init; } = new(0.7);
```

---

## Response Guidance System

The framework provides comprehensive response guidance to help AI agents understand and effectively use tool responses:

```csharp
[McpTool("identify_entity", "Find entities with high confidence")]
[McpResponseGuidance.Usage("Use returned EntityIds in subsequent searches for more accurate results")]
[McpResponseGuidance.Interpretation("Confidence scores indicate match quality - higher values mean more reliable matches")]
[McpResponseGuidance.BestPractice("Store EntityIds from high-confidence matches for future use")]
[McpResponseGuidance.Warning("Lower confidence matches may be less reliable", "When confidence < 0.5")]
public async Task<ActionResult<EntityResponse>> IdentifyEntityAsync(...)
```

### Response Guidance Categories
- **Usage**: How to use the tool response effectively
- **Interpretation**: How to understand response data and values
- **Best Practices**: Recommended approaches and patterns
- **Warning**: Important cautions and limitations
- **Tip**: Helpful hints and optimization suggestions

---

## Framework Extensions

### Creating Custom Domain Types

```csharp
[McpTypeDescription("Email address with validation")]
[McpTypePurpose(Input = "User email for notifications", Output = "Validated email address")]
public class EmailAddressValue : McpStringValue
{
    public EmailAddressValue(string email) : base(email) { }
    public EmailAddressValue() : base() { }

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
        return email?.Contains("@") == true && email.Contains(".");
    }
}
```

### Custom Validation Processors

```csharp
public class CustomValidationProcessor : McpValidationProcessor
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

---

## Best Practices

1. **Always inherit from framework primitives** - Never use system primitives directly
2. **Use descriptive type names** - Make the purpose clear from the type name
3. **Implement proper validation** - Override validation methods for domain-specific rules
4. **Provide rich metadata** - Use attributes to document purpose and usage
5. **Follow naming conventions** - Consistent naming improves maintainability
6. **Use collections for OR logic** - Collections represent alternative options
7. **Document response patterns** - Help agents understand how to use responses

---

## Framework Evolution

The McpFramework is designed to be:
- **Extensible**: Easy to add new types and validation rules
- **Maintainable**: Attribute-driven metadata stays in sync with code
- **Reusable**: Independent framework suitable for any MCP implementation
- **Type-safe**: Compile-time checks prevent common errors
- **Self-documenting**: Rich metadata helps developers and AI agents

---

## Changelog

### [1.0.0] - 2025-09-15
- **Initial Release**: Complete framework extraction with independent build capability
- **Type System**: Comprehensive hierarchy with validation and metadata support
- **Attribute Framework**: Full attribute system for tools, validation, and metadata
- **Processors**: Tool discovery, validation, and metadata processing
- **Response Guidance**: Comprehensive guidance system for AI agents
- **Documentation**: Complete architectural documentation and examples