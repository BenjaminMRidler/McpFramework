# MCP Tool Naming Standards

## Purpose
This document establishes standardized naming conventions for MCP tools to ensure consistency, clarity, and maintainability across implementations using the McpFramework.

## Core Principles

### 1. **Consistency First**
- All tools must follow the same naming pattern
- Use consistent action verbs across similar operations
- Maintain predictable naming structure

### 2. **Self-Describing Names**
- Tool names should clearly indicate their purpose
- Include relevant qualifiers for specificity
- Avoid ambiguous or generic terms

### 3. **MCP Protocol Compliance**
- Use snake_case for all tool names
- Follow MCP naming conventions
- Ensure compatibility with external MCP clients

## Standardized Naming Pattern

### **Primary Pattern**: `{action}_{entity}_{qualifier}`

**Components:**
- **Action**: The operation being performed (search, identify, navigate, create, update, delete)
- **Entity**: The primary data type being operated on (entity, expression, communication, relationship, session, topic)
- **Qualifier**: Additional specificity (comprehensive, by_threshold, by_guarantee, from_prior_sessions)

### **Action Verb Standards**

| Action | Purpose | Usage | Examples |
|--------|---------|-------|----------|
| `search` | Find/retrieve data with filtering | Primary action for data retrieval | `search_expressions_comprehensive` |
| `identify` | Find specific entities with confidence | Entity identification with scoring | `identify_entity_by_threshold` |
| `navigate` | Traverse relationships or hierarchies | Graph traversal and exploration | `navigate_memory_hierarchy` |
| `create` | Add new data/entities | Data creation operations | `create_entity` |
| `update` | Modify existing data | Data modification operations | `update_entity_attributes` |
| `delete` | Remove data/entities | Data deletion operations | `delete_entity` |
| `analyze` | Process and compute insights | Analysis and computation | `analyze_sentiment` |

### **Entity Type Standards**

| Entity | Description | Usage |
|--------|-------------|-------|
| `entity` | Core domain entities | Entity management operations |
| `expression` | Atomic content units | Expression search and retrieval |
| `communication` | Message exchanges | Communication search and retrieval |
| `relationship` | Entity-to-entity connections | Relationship traversal and discovery |
| `session` | Time-bounded containers | Session-based operations |
| `topic` | Discussion themes | Topic-based operations |
| `document` | File-based content | Document management |
| `user` | User-specific operations | User management |

### **Qualifier Standards**

| Qualifier | Purpose | Usage |
|-----------|---------|-------|
| `comprehensive` | Multi-criteria search with extensive filtering | `search_expressions_comprehensive` |
| `by_threshold` | Filter by confidence/relevance threshold | `identify_entity_by_threshold` |
| `by_guarantee` | Return best matches regardless of confidence | `identify_entity_by_guarantee` |
| `by_entity` | Filter by specific entity | `search_relationships_by_entity` |
| `by_type` | Filter by specific type | `search_relationships_by_type` |
| `from_prior_sessions` | Temporal context from previous sessions | `search_expressions_from_prior_sessions` |
| `by_relationship` | Filter by relationship criteria | `search_entities_by_relationship` |
| `paths` | Find connection paths | `search_relationship_paths` |
| `bulk` | Batch operations | `create_entities_bulk` |
| `advanced` | Complex operations with many options | `search_entities_advanced` |

## Example Tool Categories

### **Entity Management Tools**
- `identify_entity_by_threshold`
- `identify_entity_by_guarantee`
- `create_entity`
- `update_entity_attributes`
- `delete_entity`
- `search_entities_comprehensive`

### **Content Search Tools**
- `search_expressions_comprehensive`
- `search_expressions_from_prior_sessions`
- `search_expressions_by_session`
- `search_expressions_by_topic`

### **Communication Tools**
- `search_communications_comprehensive`
- `search_communications_by_session`
- `search_communications_by_expression`

### **Relationship Management Tools**
- `search_relationships_by_entity`
- `search_relationships_by_type`
- `search_entities_by_relationship`
- `search_relationship_paths`

### **Navigation Tools**
- `navigate_memory_hierarchy`
- `navigate_entity_relationships`
- `navigate_topic_sessions`

## Implementation Guidelines

### **For New Tools**
- Always follow the `{action}_{entity}_{qualifier}` pattern
- Use `search` as the primary action for data retrieval
- Use `identify` only for entity identification with confidence scoring
- Use `navigate` for graph traversal and hierarchy exploration
- Include appropriate qualifiers for specificity

### **Tool Definition Example**
```csharp
[McpTool("search_entities_comprehensive", 
    "Find entities using flexible search criteria",
    Category = McpToolCategories.EntityManagement,
    OperationType = McpOperationTypes.Search,
    Examples = new[] { 
        "Find all organizations in San Francisco",
        "Search for people associated with Project Alpha"
    })]
public async Task<ActionResult<EntitySearchResponse>> SearchEntitiesAsync(
    [FromBody] EntitySearchRequest request)
```

### **Validation Rules**
- Tool names must be snake_case
- Tool names must follow the established pattern
- Tool names must be unique across the system
- Tool names must be self-describing and unambiguous
- Tool names should not exceed 50 characters

### **Special Cases**

#### **Single-Word Actions**
For simple operations, the qualifier may be omitted:
- `create_entity` (instead of `create_entity_single`)
- `delete_entity` (instead of `delete_entity_single`)

#### **Tool Families**
Related tools should share the same base pattern:
- `search_entities_basic`
- `search_entities_comprehensive`
- `search_entities_advanced`

#### **Domain-Specific Actions**
Some domains may require specific action verbs:
- `authenticate_user`
- `authorize_access`
- `validate_input`

## Framework Integration

### **Tool Categories**
Use the framework's standard categories:

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

### **Operation Types**
Use the framework's standard operation types:

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

## Benefits of Standardization

1. **Consistency**: All tools follow the same naming pattern
2. **Predictability**: Users can guess tool names based on functionality
3. **Maintainability**: Easier to manage and update tool names
4. **Discoverability**: Better tool discovery and documentation
5. **Professional**: More polished and professional API surface
6. **AI-Friendly**: Easier for AI agents to understand and categorize tools

## Best Practices

### **Do's**
- Use descriptive, specific names
- Follow the established pattern consistently
- Include appropriate qualifiers for disambiguation
- Use standard action verbs when possible
- Keep names concise but clear

### **Don'ts**
- Don't use abbreviations or acronyms
- Don't include version numbers in tool names
- Don't use camelCase or PascalCase
- Don't create overly long tool names
- Don't use ambiguous or generic qualifiers

## Tool Discovery Integration

The framework's tool discovery system will automatically extract and validate tool names:

```csharp
var discoveryProcessor = new McpToolDiscoveryProcessor();
var toolsResult = discoveryProcessor.DiscoverTools(controller);

// Validates tool naming conventions
foreach (var tool in toolsResult.Tools)
{
    ValidateToolName(tool.Name); // Framework validation
}
```

## Changelog

### [1.0.0] - 2025-09-15
- **Created**: Framework-specific tool naming standards
- **Established**: `{action}_{entity}_{qualifier}` naming pattern
- **Defined**: Action verb standards and entity type standards
- **Integrated**: Framework categories and operation types
- **Added**: Validation rules and best practices