using System;
using Xunit;
using McpFramework;
using McpFramework.Attributes;
using McpFramework.McpTypes;

namespace McpFramework.Tests
{
    /// <summary>
    /// Comprehensive tests for MCP Framework range validation functionality.
    /// Tests the architectural pattern where primitives implement ValidateRange()
    /// and domain types inherit this capability automatically.
    /// </summary>
    public class RangeValidationTests
    {
        #region McpInt Range Validation Tests

        [Fact]
        public void McpInt_ValidateRange_WithinInclusiveRange_ShouldPass()
        {
            // Arrange
            var mcpInt = new McpInt(5);
            var rangeAttr = new McpRangeAttribute(1, 10) { Inclusive = true };

            // Act
            var result = mcpInt.ValidateRange(rangeAttr, "testValue", "testTool");

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void McpInt_ValidateRange_OnInclusiveBoundary_ShouldPass()
        {
            // Arrange
            var mcpInt = new McpInt(10);
            var rangeAttr = new McpRangeAttribute(1, 10) { Inclusive = true };

            // Act
            var result = mcpInt.ValidateRange(rangeAttr, "testValue", "testTool");

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void McpInt_ValidateRange_OnExclusiveBoundary_ShouldFail()
        {
            // Arrange
            var mcpInt = new McpInt(10);
            var rangeAttr = new McpRangeAttribute(1, 10) { Inclusive = false };

            // Act
            var result = mcpInt.ValidateRange(rangeAttr, "testValue", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("exclusive", result.Errors[0].Message);
            Assert.Equal("OUT_OF_RANGE", result.Errors[0].ErrorCode);
            Assert.Single(result.Suggestions);
        }

        [Fact]
        public void McpInt_ValidateRange_BelowRange_ShouldFailWithCorrectSuggestion()
        {
            // Arrange
            var mcpInt = new McpInt(-5);
            var rangeAttr = new McpRangeAttribute(1, 10) { Inclusive = true };

            // Act
            var result = mcpInt.ValidateRange(rangeAttr, "testValue", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("must be between 1 and 10", result.Errors[0].Message);
            Assert.Single(result.Suggestions);
            Assert.Equal("1", result.Suggestions[0].Example); // Should suggest minimum value
        }

        [Fact]
        public void McpInt_ValidateRange_WithCustomMessage_ShouldUseCustomMessage()
        {
            // Arrange
            var mcpInt = new McpInt(15);
            var rangeAttr = new McpRangeAttribute(1, 10) 
            { 
                Inclusive = true, 
                CustomMessage = "Score must be between 1 and 10" 
            };

            // Act
            var result = mcpInt.ValidateRange(rangeAttr, "score", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Score must be between 1 and 10", result.Errors[0].Message);
            Assert.Equal("10", result.Suggestions[0].Example); // Should suggest maximum value
        }

        #endregion

        #region McpFloat Range Validation Tests

        [Fact]
        public void McpFloat_ValidateRange_WithinRange_ShouldPass()
        {
            // Arrange
            var mcpFloat = new McpFloat(0.5f);
            var rangeAttr = new McpRangeAttribute(0.0, 1.0) { Inclusive = true };

            // Act
            var result = mcpFloat.ValidateRange(rangeAttr, "threshold", "testTool");

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void McpFloat_ValidateRange_OutOfRange_ShouldFailWithPrecision()
        {
            // Arrange
            var mcpFloat = new McpFloat(1.5f);
            var rangeAttr = new McpRangeAttribute(0.0, 1.0) { Inclusive = true };

            // Act
            var result = mcpFloat.ValidateRange(rangeAttr, "threshold", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("0.00", result.Errors[0].Message);
            Assert.Contains("1.00", result.Errors[0].Message);
            Assert.Contains("1.50", result.Errors[0].Message); // Current value with precision
        }

        #endregion

        #region McpDateTime Range Validation Tests

        [Fact]
        public void McpDateTime_ValidateRange_WithinDateRange_ShouldPass()
        {
            // Arrange
            var testDate = new DateTime(2025, 6, 15);
            var mcpDateTime = new McpDateTime(testDate);
            var rangeAttr = new McpRangeAttribute(
                new DateTime(2025, 1, 1), 
                new DateTime(2025, 12, 31)
            ) { Inclusive = true };

            // Act
            var result = mcpDateTime.ValidateRange(rangeAttr, "eventDate", "testTool");

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void McpDateTime_ValidateRange_OutOfDateRange_ShouldFailWithDateFormat()
        {
            // Arrange
            var testDate = new DateTime(2024, 6, 15);
            var mcpDateTime = new McpDateTime(testDate);
            var rangeAttr = new McpRangeAttribute(
                new DateTime(2025, 1, 1), 
                new DateTime(2025, 12, 31)
            ) { Inclusive = true };

            // Act
            var result = mcpDateTime.ValidateRange(rangeAttr, "eventDate", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("DATETIME_OUT_OF_RANGE", result.Errors[0].ErrorCode);
            Assert.Contains("2025-01-01", result.Errors[0].Message);
            Assert.Contains("2025-12-31", result.Errors[0].Message);
            Assert.Contains("2024-06-15", result.Errors[0].Message);
        }

        #endregion

        #region McpStringValue Range Validation Tests

        [Theory]
        [InlineData("Hello", 3, 10, true, true)]      // Within range
        [InlineData("Hi", 3, 10, true, false)]        // Too short
        [InlineData("This is too long", 3, 10, true, false)] // Too long
        [InlineData("Exact", 5, 5, true, true)]       // Exact length inclusive
        [InlineData("Exact", 5, 5, false, false)]     // Exact length exclusive
        public void McpStringValue_ValidateRange_VariousLengths_ShouldValidateCorrectly(
            string testValue, int min, int max, bool inclusive, bool shouldPass)
        {
            // Arrange
            var mcpString = new TestMcpStringValue(testValue);
            var rangeAttr = new McpRangeAttribute(min, max) { Inclusive = inclusive };

            // Act
            var result = mcpString.ValidateRange(rangeAttr, "testString", "testTool");

            // Assert
            Assert.Equal(shouldPass, result.IsValid);
            if (!shouldPass)
            {
                Assert.Single(result.Errors);
                Assert.Equal("STRING_LENGTH_OUT_OF_RANGE", result.Errors[0].ErrorCode);
                Assert.Single(result.Suggestions);
            }
        }

        [Fact]
        public void McpStringValue_ValidateRange_TooLong_ShouldSuggestTruncation()
        {
            // Arrange
            var longString = "This string is way too long for the range";
            var mcpString = new TestMcpStringValue(longString);
            var rangeAttr = new McpRangeAttribute(5, 20) { Inclusive = true };

            // Act
            var result = mcpString.ValidateRange(rangeAttr, "description", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Suggestions);
            Assert.Contains("too long", result.Suggestions[0].Suggestion);
            Assert.Equal(20, result.Suggestions[0].Example.Length); // Should be truncated to max length
        }

        [Fact]
        public void McpStringValue_ValidateRange_TooShort_ShouldSuggestPadding()
        {
            // Arrange
            var shortString = "Hi";
            var mcpString = new TestMcpStringValue(shortString);
            var rangeAttr = new McpRangeAttribute(5, 20) { Inclusive = true };

            // Act
            var result = mcpString.ValidateRange(rangeAttr, "code", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Suggestions);
            Assert.Contains("too short", result.Suggestions[0].Suggestion);
            Assert.Equal(5, result.Suggestions[0].Example.Length); // Should be padded to min length
        }

        #endregion

        #region McpGuidValue Range Validation Tests

        [Fact]
        public void McpGuidValue_ValidateRange_ShouldReportNotApplicable()
        {
            // Arrange
            var mcpGuid = new TestMcpGuidValue(Guid.NewGuid());
            var rangeAttr = new McpRangeAttribute(1, 10);

            // Act
            var result = mcpGuid.ValidateRange(rangeAttr, "entityId", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("RANGE_NOT_APPLICABLE", result.Errors[0].ErrorCode);
            Assert.Contains("Range validation is not applicable for GUID values", result.Errors[0].Message);
            Assert.Contains("unique identifiers", result.Errors[0].Message);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void McpInt_ValidateRange_InvalidRangeConversion_ShouldFailGracefully()
        {
            // Arrange
            var mcpInt = new McpInt(5);
            var rangeAttr = new McpRangeAttribute("not-a-number", 10);

            // Act
            var result = mcpInt.ValidateRange(rangeAttr, "testValue", "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("RANGE_CONVERSION_ERROR", result.Errors[0].ErrorCode);
            Assert.Contains("Could not convert", result.Errors[0].Message);
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Concrete implementation of McpStringValue for testing
        /// </summary>
        public class TestMcpStringValue : McpStringValue
        {
            public TestMcpStringValue(string value) : base(value) { }
            public TestMcpStringValue() : base() { }

            public override McpValidationResult ValidateFormat(string parameterName, string toolName)
            {
                return new McpValidationResult { IsValid = true };
            }

            public override McpValidationResult ValidateRequired(string parameterName, string toolName)
            {
                return new McpValidationResult { IsValid = true };
            }
        }

        /// <summary>
        /// Concrete implementation of McpGuidValue for testing
        /// </summary>
        public class TestMcpGuidValue : McpGuidValue
        {
            public TestMcpGuidValue(Guid value) : base(value) { }
            public TestMcpGuidValue() : base(Guid.NewGuid()) { }

            public override McpValidationResult ValidateFormat(string parameterName, string toolName)
            {
                return new McpValidationResult { IsValid = true };
            }

            public override McpValidationResult ValidateRequired(string parameterName, string toolName)
            {
                return new McpValidationResult { IsValid = true };
            }
        }

        #endregion
    }
}