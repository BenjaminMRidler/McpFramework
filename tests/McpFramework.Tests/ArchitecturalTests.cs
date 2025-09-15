using System;
using System.Linq;
using System.Reflection;
using Xunit;
using McpFramework.McpTypes;

namespace McpFramework.Tests
{
    /// <summary>
    /// Architectural tests to verify the MCP Framework inheritance hierarchy is correctly implemented.
    /// Ensures the critical rule: McpTypes → McpPrimitives → McpTypedValue<T> → McpValue
    /// </summary>
    public class ArchitecturalTests
    {
        #region Inheritance Hierarchy Tests

        [Fact]
        public void McpPrimitives_ShouldInheritFromMcpTypedValue()
        {
            // Arrange - Get all primitive types
            var primitiveTypes = new[]
            {
                typeof(McpInt),
                typeof(McpFloat),
                typeof(McpDouble),
                typeof(McpDecimal),
                typeof(McpDateTime),
                typeof(McpStringValue),
                typeof(McpGuidValue)
            };

            // Act & Assert
            foreach (var primitiveType in primitiveTypes)
            {
                Assert.True(IsSubclassOfGeneric(primitiveType, typeof(McpTypedValue<>)), 
                    $"{primitiveType.Name} must inherit from McpTypedValue<T>");
            }
        }

        [Fact]
        public void McpTypedValue_ShouldInheritFromMcpValue()
        {
            // Arrange
            var mcpTypedValueType = typeof(McpTypedValue<>);

            // Act & Assert
            // McpTypedValue<T> is generic, so we check if its base type is McpValue
            Assert.Equal(typeof(McpValue), mcpTypedValueType.BaseType);
        }

        [Fact]
        public void McpValue_ShouldBeAbstractBaseClass()
        {
            // Arrange
            var mcpValueType = typeof(McpValue);

            // Act & Assert
            Assert.True(mcpValueType.IsAbstract);
            Assert.Null(mcpValueType.BaseType?.Name == "Object" ? null : mcpValueType.BaseType);
        }

        #endregion

        #region Validation Method Tests

        [Fact]
        public void McpValue_ShouldHaveAbstractValidateRangeMethod()
        {
            // Arrange
            var mcpValueType = typeof(McpValue);

            // Act
            var validateRangeMethod = mcpValueType.GetMethod("ValidateRange");

            // Assert
            Assert.NotNull(validateRangeMethod);
            Assert.True(validateRangeMethod.IsAbstract, "ValidateRange should be abstract in McpValue");
            
            // Check method signature
            var parameters = validateRangeMethod.GetParameters();
            Assert.Equal(3, parameters.Length);
            Assert.Equal("rangeAttr", parameters[0].Name);
            Assert.Equal("parameterName", parameters[1].Name);
            Assert.Equal("toolName", parameters[2].Name);
        }

        [Fact]
        public void AllMcpPrimitives_ShouldImplementValidateRange()
        {
            // Arrange
            var primitiveTypes = new[]
            {
                typeof(McpInt),
                typeof(McpFloat),
                typeof(McpDouble),
                typeof(McpDecimal),
                typeof(McpDateTime),
                typeof(McpStringValue),
                typeof(McpGuidValue)
            };

            // Act & Assert
            foreach (var primitiveType in primitiveTypes)
            {
                var validateRangeMethod = primitiveType.GetMethod("ValidateRange");
                Assert.NotNull(validateRangeMethod);
                Assert.False(validateRangeMethod.IsAbstract, 
                    $"{primitiveType.Name} must provide concrete implementation of ValidateRange");
            }
        }

        #endregion

        #region Type System Integrity Tests

        [Fact]
        public void AllMcpPrimitives_ShouldBeInMcpTypesNamespace()
        {
            // Arrange
            var expectedNamespace = "McpFramework.McpTypes";
            var primitiveTypes = new[]
            {
                typeof(McpInt),
                typeof(McpFloat),
                typeof(McpDouble),
                typeof(McpDecimal),
                typeof(McpDateTime),
                typeof(McpStringValue),
                typeof(McpGuidValue)
            };

            // Act & Assert
            foreach (var primitiveType in primitiveTypes)
            {
                Assert.Equal(expectedNamespace, primitiveType.Namespace);
            }
        }

        #endregion

        #region Helper Methods

        private static bool IsSubclassOfGeneric(Type type, Type genericBaseType)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericBaseType == cur)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        #endregion
    }
}