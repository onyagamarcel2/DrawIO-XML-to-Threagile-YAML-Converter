using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ThreagileConverter.Core.Commands;
using ThreagileConverter.Core.Events;
using ThreagileConverter.Core.Factories;
using ThreagileConverter.Core.Parsing;
using System.Xml.Linq;
using ThreagileConverter.Core.Validation;
using ThreagileConverter.Core.Generation;

namespace ThreagileConverter.Tests.Commands
{
    public class XmlToYamlConversionCommandTests : IDisposable
    {
        private readonly string _sourcePath;
        private readonly string _targetPath;
        private readonly Mock<ILogger<XmlToYamlConversionCommand>> _loggerMock;
        private readonly Mock<IParserFactory> _parserFactoryMock;
        private readonly Mock<IParser> _xmlParserMock;
        private readonly Mock<IValidator> _xmlValidatorMock;
        private readonly Mock<IGenerator> _yamlGeneratorMock;
        private readonly Mock<IConversionObserver> _observerMock;
        private readonly XmlToYamlConversionCommand _command;

        public XmlToYamlConversionCommandTests()
        {
            _sourcePath = Path.GetTempFileName();
            _targetPath = Path.GetTempFileName();
            _loggerMock = new Mock<ILogger<XmlToYamlConversionCommand>>();
            _parserFactoryMock = new Mock<IParserFactory>();
            _xmlParserMock = new Mock<IParser>();
            _xmlValidatorMock = new Mock<IValidator>();
            _yamlGeneratorMock = new Mock<IGenerator>();
            _observerMock = new Mock<IConversionObserver>();

            _parserFactoryMock.Setup(x => x.CreateParser("xml")).Returns(_xmlParserMock.Object);
            _parserFactoryMock.Setup(x => x.CreateValidator("xml")).Returns(_xmlValidatorMock.Object);
            _parserFactoryMock.Setup(x => x.CreateGenerator("yaml")).Returns(_yamlGeneratorMock.Object);

            _command = new XmlToYamlConversionCommand(
                _sourcePath,
                _targetPath,
                _parserFactoryMock.Object,
                _loggerMock.Object);
        }

        public void Dispose()
        {
            if (File.Exists(_sourcePath))
                File.Delete(_sourcePath);
            if (File.Exists(_targetPath))
                File.Delete(_targetPath);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidXml_CompletesSuccessfully()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";
            var yamlContent = "root: {}";
            var xmlModel = XDocument.Parse(xmlContent);

            File.WriteAllText(_sourcePath, xmlContent);

            _xmlValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<XDocument>()))
                .ReturnsAsync(ThreagileConverter.Core.Validation.ValidationResult.Success());

            _xmlParserMock.Setup(x => x.ParseXmlAsync(It.IsAny<string>()))
                .ReturnsAsync(xmlModel);

            _yamlGeneratorMock.Setup(x => x.GenerateAsync(It.IsAny<XDocument>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _command.ExecuteAsync(_observerMock.Object);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(_sourcePath, result.SourcePath);
            Assert.Equal(_targetPath, result.TargetPath);
            Assert.Equal(yamlContent, await File.ReadAllTextAsync(_targetPath));

            _observerMock.Verify(x => x.OnConversionStartedAsync(_sourcePath, _targetPath), Times.Once);
            _observerMock.Verify(x => x.OnConversionProgressAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(4));
            _observerMock.Verify(x => x.OnConversionCompletedAsync(result), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidXml_ReturnsFailure()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root><unclosed>";
            var xmlModel = XDocument.Parse(xmlContent);
            File.WriteAllText(_sourcePath, xmlContent);

            _xmlValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<XDocument>()))
                .ReturnsAsync(ThreagileConverter.Core.Validation.ValidationResult.Failure(new[] { new ValidationError("Invalid XML") }));

            // Act
            var result = await _command.ExecuteAsync(_observerMock.Object);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Warnings);

            _observerMock.Verify(x => x.OnConversionStartedAsync(_sourcePath, _targetPath), Times.Once);
            _observerMock.Verify(x => x.OnConversionProgressAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_WhenCancelled_ReturnsFailure()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";
            var xmlModel = XDocument.Parse(xmlContent);
            File.WriteAllText(_sourcePath, xmlContent);

            _xmlValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<XDocument>()))
                .ReturnsAsync(ThreagileConverter.Core.Validation.ValidationResult.Success());

            _xmlParserMock.Setup(x => x.ParseXmlAsync(It.IsAny<string>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act
            var result = await _command.ExecuteAsync(_observerMock.Object);

            // Assert
            Assert.False(result.Success);
            Assert.Empty(result.Warnings);

            _observerMock.Verify(x => x.OnConversionStartedAsync(_sourcePath, _targetPath), Times.Once);
            _observerMock.Verify(x => x.OnConversionProgressAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CancelAsync_CancelsOperation()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";
            var xmlModel = XDocument.Parse(xmlContent);
            File.WriteAllText(_sourcePath, xmlContent);

            _xmlValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<XDocument>()))
                .ReturnsAsync(ThreagileConverter.Core.Validation.ValidationResult.Success());

            _xmlParserMock.Setup(x => x.ParseXmlAsync(It.IsAny<string>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act
            await _command.CancelAsync();
            var result = await _command.ExecuteAsync(_observerMock.Object);

            // Assert
            Assert.False(result.Success);
        }
    }
} 