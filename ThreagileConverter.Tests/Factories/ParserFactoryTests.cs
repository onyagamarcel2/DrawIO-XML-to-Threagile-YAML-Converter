using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ThreagileConverter.Core.Factories;
using ThreagileConverter.Core.Parsing;
using ThreagileConverter.Core.Validation;
using ThreagileConverter.Core.Generation;

namespace ThreagileConverter.Tests.Factories
{
    public class ParserFactoryTests
    {
        private readonly Mock<ILogger<ParserFactory>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly ParserFactory _factory;

        public ParserFactoryTests()
        {
            _loggerMock = new Mock<ILogger<ParserFactory>>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _factory = new ParserFactory(_loggerMock.Object, _serviceProviderMock.Object);
        }

        [Fact]
        public void CreateParser_WithXmlType_ReturnsXmlParser()
        {
            // Arrange
            var xmlParser = new Mock<IParser>().Object;
            _serviceProviderMock.Setup(x => x.GetService(typeof(XmlParser)))
                .Returns(xmlParser);

            // Act
            var result = _factory.CreateParser("xml");

            // Assert
            Assert.Same(xmlParser, result);
        }

        [Fact]
        public void CreateParser_WithYamlType_ReturnsYamlParser()
        {
            // Arrange
            var yamlParser = new Mock<IParser>().Object;
            _serviceProviderMock.Setup(x => x.GetService(typeof(YamlParser)))
                .Returns(yamlParser);

            // Act
            var result = _factory.CreateParser("yaml");

            // Assert
            Assert.Same(yamlParser, result);
        }

        [Fact]
        public void CreateParser_WithInvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.CreateParser("invalid"));
        }

        [Fact]
        public void CreateValidator_WithXmlType_ReturnsXmlValidator()
        {
            // Arrange
            var xmlValidator = new Mock<IValidator>().Object;
            _serviceProviderMock.Setup(x => x.GetService(typeof(XmlValidator)))
                .Returns(xmlValidator);

            // Act
            var result = _factory.CreateValidator("xml");

            // Assert
            Assert.Same(xmlValidator, result);
        }

        [Fact]
        public void CreateValidator_WithYamlType_ReturnsYamlValidator()
        {
            // Arrange
            var yamlValidator = new Mock<IValidator>().Object;
            _serviceProviderMock.Setup(x => x.GetService(typeof(YamlValidator)))
                .Returns(yamlValidator);

            // Act
            var result = _factory.CreateValidator("yaml");

            // Assert
            Assert.Same(yamlValidator, result);
        }

        [Fact]
        public void CreateValidator_WithInvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.CreateValidator("invalid"));
        }

        [Fact]
        public void CreateGenerator_WithYamlType_ReturnsYamlGenerator()
        {
            // Arrange
            var yamlGenerator = new Mock<IGenerator>().Object;
            _serviceProviderMock.Setup(x => x.GetService(typeof(YamlGenerator)))
                .Returns(yamlGenerator);

            // Act
            var result = _factory.CreateGenerator("yaml");

            // Assert
            Assert.Same(yamlGenerator, result);
        }

        [Fact]
        public void CreateGenerator_WithInvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.CreateGenerator("invalid"));
        }
    }
} 