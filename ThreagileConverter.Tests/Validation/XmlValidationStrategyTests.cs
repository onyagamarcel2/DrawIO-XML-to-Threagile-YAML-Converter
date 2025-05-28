using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ThreagileConverter.Core.Validation;

namespace ThreagileConverter.Tests.Validation
{
    public class XmlValidationStrategyTests
    {
        private readonly Mock<ILogger<XmlValidationStrategy>> _loggerMock;
        private readonly XmlValidationStrategy _strategy;

        public XmlValidationStrategyTests()
        {
            _loggerMock = new Mock<ILogger<XmlValidationStrategy>>();
            _strategy = new XmlValidationStrategy(_loggerMock.Object);
        }

        [Fact]
        public void CanValidate_WithValidXml_ReturnsTrue()
        {
            // Arrange
            var content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";

            // Act
            var result = _strategy.CanValidate(content);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanValidate_WithInvalidXml_ReturnsFalse()
        {
            // Arrange
            var content = "invalid content";

            // Act
            var result = _strategy.CanValidate(content);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanValidate_WithEmptyContent_ReturnsFalse()
        {
            // Arrange
            var content = "";

            // Act
            var result = _strategy.CanValidate(content);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_WithValidXml_ReturnsSuccess()
        {
            // Arrange
            var content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";

            // Act
            var result = await _strategy.ValidateAsync(content);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public async Task ValidateAsync_WithInvalidXml_ReturnsFailure()
        {
            // Arrange
            var content = "<root><unclosed>";

            // Act
            var result = await _strategy.ValidateAsync(content);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WithEmptyContent_ReturnsFailure()
        {
            // Arrange
            var content = "";

            // Act
            var result = await _strategy.ValidateAsync(content);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WithMissingHostAttribute_ReturnsWarning()
        {
            // Arrange
            var content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><mxfile></mxfile>";

            // Act
            var result = await _strategy.ValidateAsync(content);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.NotEmpty(result.Warnings);
            Assert.Contains(result.Warnings, w => w.Code == "WARN_MISSING_HOST");
        }

        [Fact]
        public async Task ValidateAsync_WithEmptyAttribute_ReturnsWarning()
        {
            // Arrange
            var content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root attr=\"\"></root>";

            // Act
            var result = await _strategy.ValidateAsync(content);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.NotEmpty(result.Warnings);
            Assert.Contains(result.Warnings, w => w.Code == "WARN_EMPTY_ATTRIBUTE");
        }
    }
} 