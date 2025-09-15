using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using McpFramework;
using McpFramework.Attributes;
using McpFramework.McpTypes;

namespace McpFramework.Tests
{
    /// <summary>
    /// Integration tests for McpValidationProcessor with range validation.
    /// Tests the complete validation pipeline from attributes to results.
    /// </summary>
    public class McpValidationProcessorTests
    {
        private readonly McpValidationProcessor _processor;

        public McpValidationProcessorTests()
        {
            _processor = new McpValidationProcessor();
        }

        #region Test Models

        /// <summary>
        /// Test request model with various range validations
        /// </summary>
        public class TestRangeValidationRequest
        {
            [McpRange(1, 100)]
            public McpInt Score { get; set; } = new McpInt();

            [McpRange(0.0, 1.0)]
            public McpFloat Confidence { get; set; } = new McpFloat();

            [McpRange(5, 50)]
            public TestMcpStringValue Description { get; set; } = new TestMcpStringValue();

            [McpRange("2025-01-01", "2025-12-31")]
            public McpDateTime EventDate { get; set; } = new McpDateTime();

            [McpRange(1, 5, false, "Priority must be between 1 and 5 (exclusive)")]
            public McpInt Priority { get; set; } = new McpInt();

            // Property without range validation
            public TestMcpGuidValue Id { get; set; } = new TestMcpGuidValue(Guid.NewGuid());
        }

        /// <summary>
        /// Test model with multiple validation attributes
        /// </summary>
        public class TestComplexValidationRequest  
        {
            [McpRequired]
            [McpRange(1, 10)]
            public McpInt RequiredRangedValue { get; set; } = new McpInt();

            [McpRange(3, 20)]
            public TestMcpStringValue OptionalRangedString { get; set; } = new TestMcpStringValue();
        }

        /// <summary>
        /// Test model to verify required validation is only called when [McpRequired] attribute is present
        /// </summary>
        public class TestRequiredGatedValidationRequest
        {
            [McpRequired]
            public TestMcpStringValue RequiredProperty { get; set; } = new TestMcpStringValue();

            // No [McpRequired] attribute - should skip required validation
            public TestMcpStringValue OptionalProperty { get; set; } = new TestMcpStringValue();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task ValidateObjectAsync_AllRangesValid_ShouldPass()
        {
            // Arrange
            var request = CreateValidTestRequest();

            // Act
            var result = await _processor.ValidateObjectAsync(request, "testTool");

            // Assert
            Assert.True(result.IsValid, $"Validation should pass but got errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
            Assert.Empty(result.Errors);
            Assert.Empty(result.Suggestions);
        }

        [Fact]
        public async Task ValidateObjectAsync_MultipleRangeFailures_ShouldReportAllErrors()
        {
            // Arrange
            var request = CreateValidTestRequest();
            // Set all properties to invalid values
            request.Score = new McpInt(150);           // Above range (1-100)
            request.Confidence = new McpFloat(1.5f);   // Above range (0.0-1.0)  
            request.Description = new TestMcpStringValue("Hi");  // Too short (5-50)
            request.EventDate = new McpDateTime(new DateTime(2024, 6, 15)); // Before range (2025)
            request.Priority = new McpInt(5);          // On exclusive boundary (1-5 exclusive)

            // Act
            var result = await _processor.ValidateObjectAsync(request, "testTool");

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(5, result.Errors.Count);
            Assert.Equal(5, result.Suggestions.Count);

            // Verify error codes
            Assert.Contains(result.Errors, e => e.ErrorCode == "OUT_OF_RANGE");
            Assert.Contains(result.Errors, e => e.ErrorCode == "STRING_LENGTH_OUT_OF_RANGE");
            Assert.Contains(result.Errors, e => e.ErrorCode == "DATETIME_OUT_OF_RANGE");
        }

        [Fact]
        public async Task ValidateObjectAsync_CustomRangeMessage_ShouldUseCustomMessage()
        {
            // Arrange
            var request = CreateValidTestRequest();
            request.Priority = new McpInt(1); // On exclusive boundary with custom message

            // Act
            var result = await _processor.ValidateObjectAsync(request, "testTool");

            // Assert
            Assert.False(result.IsValid);
            var priorityError = result.Errors.FirstOrDefault(e => e.ParameterName == "Priority");
            Assert.NotNull(priorityError);
            Assert.Equal("Priority must be between 1 and 5 (exclusive)", priorityError.Message);
        }

        [Fact]
        public async Task ValidateObjectAsync_RequiredAttributeGatedValidation_ShouldOnlyValidateRequiredWhenAttributePresent()
        {
            // Arrange
            var request = new TestRequiredGatedValidationRequest
            {
                RequiredProperty = new TestMcpStringValue("valid"), // Has [McpRequired] - should validate
                OptionalProperty = new TestMcpStringValue("valid")   // No [McpRequired] - should skip required validation
            };

            // Act
            var result = await _processor.ValidateObjectAsync(request, "testTool");

            // Assert
            Assert.True(result.IsValid, $"Validation should pass but got errors: {string.Join(", ", result.Errors.ConvertAll(e => e.Message))}");
            Assert.Empty(result.Errors);
        }

        #endregion

        #region Boundary Tests

        [Theory]
        [InlineData(1, true)]    // Min boundary inclusive
        [InlineData(100, true)]  // Max boundary inclusive  
        [InlineData(0, false)]   // Below min
        [InlineData(101, false)] // Above max
        public async Task ValidateObjectAsync_InclusiveRangeBoundaries_ShouldValidateCorrectly(
            int scoreValue, bool shouldPass)
        {
            // Arrange - Create a valid base request and only modify the score
            var request = CreateValidTestRequest();
            request.Score = new McpInt(scoreValue);

            // Act
            var result = await _processor.ValidateObjectAsync(request, "testTool");

            // Assert
            Assert.Equal(shouldPass, result.IsValid);
        }

        [Theory]
        [InlineData(2, true)]    // Within exclusive range
        [InlineData(4, true)]    // Within exclusive range
        [InlineData(1, false)]   // On min boundary (exclusive)
        [InlineData(5, false)]   // On max boundary (exclusive)
        [InlineData(0, false)]   // Below range
        [InlineData(6, false)]   // Above range
        public async Task ValidateObjectAsync_ExclusiveRangeBoundaries_ShouldValidateCorrectly(
            int priorityValue, bool shouldPass)
        {
            // Arrange - Create a valid base request and only modify the priority
            var request = CreateValidTestRequest();
            request.Priority = new McpInt(priorityValue);

            // Act  
            var result = await _processor.ValidateObjectAsync(request, "testTool");

            // Assert
            Assert.Equal(shouldPass, result.IsValid);
            
            if (!shouldPass)
            {
                var priorityError = result.Errors.FirstOrDefault(e => e.ParameterName == "Priority");
                Assert.NotNull(priorityError);
                Assert.Contains("exclusive", priorityError.Message);
            }
        }

        #endregion

        #region Test Helper Methods

        private TestRangeValidationRequest CreateValidTestRequest()
        {
            return new TestRangeValidationRequest
            {
                Score = new McpInt(50),                    // Valid range [1,100]
                Confidence = new McpFloat(0.8f),           // Valid range [0.0,1.0]
                Description = new TestMcpStringValue("Valid description"), // Valid range [5,50]
                EventDate = new McpDateTime(new DateTime(2025, 6, 15)),    // Valid range [2025-01-01,2025-12-31]
                Priority = new McpInt(3),                  // Valid exclusive range (1,5)
                Id = new TestMcpGuidValue(Guid.NewGuid())  // Valid GUID
            };
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Concrete test implementation of McpStringValue
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
        /// Concrete test implementation of McpGuidValue  
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