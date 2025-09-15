using System;
using System.Collections.Generic;
using System.Reflection;
using McpFramework.Attributes;
using McpFramework.McpTypes;

namespace McpFramework
{
    public class McpMetadataProcessor
    {
        public McpTypeMetadata ExtractTypeMetadata(Type type, bool isInputContext = true)
        {
            var metadata = new McpTypeMetadata
            {
                TypeName = type.Name,
                FullTypeName = type.FullName ?? type.Name
            };

            // Extract class-level metadata attributes
            var descriptionAttr = type.GetCustomAttribute<McpTypeDescriptionAttribute>();
            if (descriptionAttr != null)
            {
                metadata.Description = descriptionAttr.Description;
            }

            var purposeAttr = type.GetCustomAttribute<McpTypePurposeAttribute>();
            if (purposeAttr != null)
            {
                metadata.Purpose = purposeAttr.GetPurpose(isInputContext);
            }

            var usageAttr = type.GetCustomAttribute<McpTypeUsageAttribute>();
            if (usageAttr != null)
            {
                metadata.Usage = usageAttr.GetUsage(isInputContext);
            }

            return metadata;
        }

        public McpPropertyMetadata ExtractPropertyMetadata(PropertyInfo property)
        {
            var metadata = new McpPropertyMetadata
            {
                PropertyName = property.Name,
                PropertyType = property.PropertyType.Name,
                IsRequired = property.GetCustomAttribute<McpRequiredAttribute>() != null,
                HasExistenceValidation = property.GetCustomAttribute<McpExistsAttribute>() != null
            };

            // Check if the property type has its own metadata
            if (typeof(McpValue).IsAssignableFrom(property.PropertyType))
            {
                metadata.TypeMetadata = ExtractTypeMetadata(property.PropertyType);
            }

            return metadata;
        }
    }

    public class McpTypeMetadata
    {
        public string TypeName { get; set; } = string.Empty;
        public string FullTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
    }

    public class McpPropertyMetadata
    {
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool HasExistenceValidation { get; set; }
        public McpTypeMetadata? TypeMetadata { get; set; }
    }
}
