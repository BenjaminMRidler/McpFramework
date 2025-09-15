# McpFramework Attributes Reference

## Purpose
This document provides a canonical reference for all attributes available in the McpFramework.

## Overview
This catalog lists the currently implemented MCP attributes and their purpose. Collections of values are OR'd; separate parameters are AND'd.

## Attribute Reference

> **Attribute Scope Conventions**
> * Controller class: Glossary attributes provide domain context
> * Controller method (endpoint): Tool definition attributes expose actions and response types
> * Domain types: Metadata & validation attributes describe and enforce parameters

## Validation Attributes

**Applied to:** Request/response model properties and domain MCP types  
**Purpose:** Enforce data integrity and provide runtime validation for tool parameters and responses.

| Attribute | Parameters | Purpose |
|-----------|------------|---------|
| `McpRequired` | `customMessage : string` (optional) | Marks property/parameter as required. Triggers ValidateRequired() method. |
| `McpRange` | `min : object` – lower bound<br>`max : object` – upper bound<br>`inclusive : bool` (default true) – include bounds<br>`customMessage : string` (optional) | Constrains numeric value or collection size to min/max. Inclusive determines ≤ / < logic. Triggers ValidateRange() method. |
| `McpExists` | `entityType : string` (e.g., "Entity", "Topic", "Expression")<br>`customMessage : string` (optional) | Triggers ValidateExistenceAsync() on the value. Used for database existence validation. |

### Usage Examples

```csharp
public class SearchRequest : IMcpRequest
{
    [McpRequired]
    public UserIdValue UserId { get; init; } = new();

    [McpRange(1, 100)]
    public MaxResultsValue MaxResults { get; init; } = new(10);

    [McpRange(0.0, 1.0)]
    public ConfidenceThresholdValue Threshold { get; init; } = new(0.7);

    [McpExists("Entity")]
    public EntityIdValue? EntityId { get; init; }

    [McpRange(5, 50, true, "Query must be between 5 and 50 characters")]
    public QueryStringValue Query { get; init; } = new();
}
```

## Metadata Attributes

**Applied to:** Types and properties  
**Purpose:** Provide rich metadata for tool discovery and documentation.

| Attribute | Parameters | Purpose |
|-----------|------------|---------|
| `McpTypeDescription` | `description : string` | Human-readable description of the type's purpose and usage. |
| `McpTypePurpose` | `general : string` **or** `Input : string`, `Output : string` via named params | Describes how the type is used in different contexts (input vs output). |
| `McpTypeUsage` | `general : string` **or** `Input : string`, `Output : string` via named params | Provides guidance on how to use the type effectively. |

### Type Metadata Examples

```csharp
[McpTypeDescription("Unique identifier for entities in the system")]
[McpTypePurpose(
    Input = "Specifies which entity to operate on",
    Output = "Identifies the entity in the response"
)]
[McpTypeUsage(
    Input = "Use existing entity IDs from previous searches or creations",
    Output = "Store returned IDs for use in subsequent operations"
)]
public class EntityIdValue : McpGuidValue
{
    public EntityIdValue() : base() { }
    public EntityIdValue(Guid value) : base(value) { }
}
```

## Property-Level Metadata Attributes

**Applied to:** Properties only  
**Purpose:** Override or augment type-level metadata for specific property contexts.

| Attribute | Parameters | Purpose |
|-----------|------------|---------|
| `McpPropertyDescription` | `description : string`<br>`mode : MetadataMode` (optional) | Override, prepend, or append property-specific description. |
| `McpPropertyUsage` | `usage : string`<br>`mode : MetadataMode` (optional) | Override, prepend, or append property-specific usage guidance. |
| `McpPropertyPurpose` | `general : string` **or** `Input/Output : string`<br>`mode : MetadataMode` (optional) | Override, prepend, or append property-specific purpose. |

### Metadata Composition Modes

```csharp
public enum MetadataMode
{
    Override,  // Replace type metadata entirely (default)
    Prepend,   // Add before type metadata
    Append     // Add after type metadata
}
```

### Property Metadata Examples

```csharp
public class RelationshipSearchRequest : IMcpRequest
{
    // Disambiguate same type used in different contexts
    [McpPropertyDescription("Starting entity for relationship traversal")]
    public EntityIdValue FromEntityId { get; init; } = new();

    [McpPropertyDescription("Target entity for relationship traversal")]
    public EntityIdValue ToEntityId { get; init; } = new();

    // Augment base type description
    [McpPropertyDescription(" (must be active and verified)", MetadataMode.Append)]
    public UserIdValue UserId { get; init; } = new();

    // Add context while preserving base description
    [McpPropertyUsage("Specify the maximum depth to traverse when finding paths", MetadataMode.Prepend)]
    public MaxDepthValue MaxDepth { get; init; } = new(3);
}
```

## Tool Definition Attributes

**Applied to:** Controller methods  
**Purpose:** Expose endpoints as MCP tools with rich metadata.

| Attribute | Parameters | Purpose |
|-----------|------------|---------|
| `McpTool` | `name : string` – tool name (snake_case)<br>`description : string` – tool purpose<br>`Category : string` (optional) – tool category<br>`OperationType : string` (optional) – operation type<br>`Examples : string[]` (optional) – usage examples<br>`ValidateRequest : bool` (default true) – enable validation | Exposes method as MCP tool with comprehensive metadata. |
| `McpResponseType` | `responseType : Type` | Declares concrete response type for IActionResult methods. Required for proper schema generation. |

### Tool Definition Examples

```csharp
[McpTool("search_entities_comprehensive", 
    "Find entities using flexible search criteria with confidence scoring",
    Category = McpToolCategories.EntityManagement,
    OperationType = McpOperationTypes.Search,
    Examples = new[] { 
        "Find all organizations in San Francisco",
        "Search for people associated with Project Alpha",
        "Locate entities with high confidence scores"
    })]
[McpResponseType(typeof(EntitySearchResponse))]
public async Task<IActionResult> SearchEntitiesAsync([FromBody] EntitySearchRequest request)
{
    // Implementation
}

// Disable validation for special cases
[McpTool("legacy_import", "Import legacy data without validation", ValidateRequest = false)]
public async Task<IActionResult> LegacyImportAsync([FromBody] LegacyRequest request)
{
    // Implementation
}
```

## Response Guidance Attributes

**Applied to:** Controller methods  
**Purpose:** Provide AI agents with guidance on how to interpret and use tool responses.

| Attribute | Parameters | Purpose |
|-----------|------------|---------|
| `McpResponseGuidance.Usage` | `guidance : string`<br>`condition : string` (optional)<br>`priority : int` (optional, default 3) | How to use the tool response effectively. |
| `McpResponseGuidance.Interpretation` | `guidance : string`<br>`condition : string` (optional)<br>`priority : int` (optional, default 3) | How to understand response data and values. |
| `McpResponseGuidance.BestPractice` | `guidance : string`<br>`condition : string` (optional)<br>`priority : int` (optional, default 4) | Recommended approaches and patterns. |
| `McpResponseGuidance.Warning` | `guidance : string`<br>`condition : string` (optional)<br>`priority : int` (optional, default 5) | Important cautions and limitations. |
| `McpResponseGuidance.Tip` | `guidance : string`<br>`condition : string` (optional)<br>`priority : int` (optional, default 2) | Helpful hints and optimization suggestions. |

### Response Guidance Examples

```csharp
[McpTool("identify_entity_by_threshold", "Find entities with confidence scoring")]
[McpResponseGuidance.Usage("Use returned EntityIds in subsequent searches for better accuracy than text-based searches")]
[McpResponseGuidance.Interpretation("Confidence scores indicate match quality - higher values mean more reliable matches", "When confidence > 0.8")]
[McpResponseGuidance.BestPractice("Store EntityIds from high-confidence matches for future use in other tools")]
[McpResponseGuidance.Warning("Lower confidence matches may be unreliable - verify before using in critical operations", "When confidence < 0.5")]
[McpResponseGuidance.Tip("Check Reasoning field to understand why entities matched your criteria")]
public async Task<ActionResult<EntityIdentificationResponse>> IdentifyEntityAsync(...)
```

### Priority Levels

- **Priority 5 (Critical)**: Warnings and critical information
- **Priority 4 (High)**: Best practices and important guidance  
- **Priority 3 (Normal)**: Standard usage guidance (default)
- **Priority 2 (Low)**: Tips and helpful suggestions
- **Priority 1 (Informational)**: Additional context and details

## Glossary Attributes

**Applied to:** Controller classes  
**Purpose:** Provide domain context and terminology for tool discovery.

| Attribute | Parameters | Purpose |
|-----------|------------|---------|
| `McpGlossaryHeader` | `title : string`<br>`overview : string` | Provides domain overview and context for all tools in the controller. |
| `McpGlossaryTerm` | `term : string`<br>`definition : string` | Defines important domain terminology. |
| `McpGlossaryRelationship` | `concept1 : string`<br>`relationship : string`<br>`concept2 : string` | Describes relationships between domain concepts. |
| `McpGlossaryNote` | `note : string` | Adds contextual notes and clarifications. |

### Glossary Examples

```csharp
[McpGlossaryHeader("Entity Management", 
    "Tools for identifying, searching, and managing entities within the system")]
[McpGlossaryTerm("Entity", "A distinct object or concept that can be identified and tracked")]
[McpGlossaryTerm("Confidence Score", "Numerical value (0.0-1.0) indicating match reliability")]
[McpGlossaryRelationship("Entity", "can have multiple", "Relationships")]
[McpGlossaryRelationship("Confidence Score", "determines", "Match Quality")]
[McpGlossaryNote("All entity operations require valid user authentication")]
public class EntityController : ControllerBase
{
    // Tool methods
}
```

## Framework Categories and Operation Types

### Standard Categories

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

### Standard Operation Types

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

## Validation Flow

When a request is processed, validation attributes trigger specific validation methods:

```
1. [McpRequired] → McpValue.ValidateRequired()
2. [McpRange] → McpValue.ValidateRange()
3. [McpExists] → McpValue.ValidateExistenceAsync()
4. Custom validation → McpValue.ValidateFormat()
```

## Best Practices

### Validation Attributes
- Use `[McpRequired]` for mandatory fields
- Apply `[McpRange]` to constrain numeric values and collection sizes
- Use `[McpExists]` for database reference validation
- Provide custom messages for business-specific validation rules

### Metadata Attributes
- Use `[McpTypeDescription]` to clearly explain the type's purpose
- Apply `[McpTypePurpose]` to distinguish input vs output usage
- Use property-level attributes to disambiguate same types in different contexts
- Choose appropriate MetadataMode for property-level overrides

### Tool Definition
- Follow snake_case naming convention for tool names
- Provide clear, actionable descriptions
- Use standard categories and operation types
- Include realistic usage examples
- Set ValidateRequest = false only when necessary

### Response Guidance
- Provide Usage guidance for all tools
- Add Warnings for important limitations
- Use appropriate priority levels
- Include conditions for contextual guidance

## Changelog

### [1.0.0] - 2025-09-15
- **Created**: Comprehensive framework attributes reference
- **Documented**: All validation, metadata, and tool definition attributes
- **Added**: Response guidance attribute system
- **Included**: Glossary attributes for domain context
- **Provided**: Examples and best practices for all attribute types