using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ThreagileConverter.Core.Parsing;

namespace ThreagileConverter.Tests.Unit.Parsing
{
    /// <summary>
    /// Tests unitaires pour la classe XmlParser.
    /// Ces tests vérifient le bon fonctionnement du parser XML dans différents scénarios.
    /// </summary>
    public class XmlParserTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly Mock<ILogger<XmlParser>> _loggerMock;
        private readonly XmlParser _parser;

        public XmlParserTests()
        {
            _testFilePath = Path.GetTempFileName();
            _loggerMock = new Mock<ILogger<XmlParser>>();
            _parser = new XmlParser(_loggerMock.Object);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        #region Tests de Parsing Réussi

        /// <summary>
        /// Vérifie que le parser peut traiter un fichier XML simple et valide.
        /// </summary>
        [Fact]
        public void ParseXml_WithValidXml_ReturnsXDocument()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            var result = _parser.ParseXml(_testFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("root", result.Root.Name.LocalName);
        }

        /// <summary>
        /// Vérifie que le parser peut traiter un fichier XML complexe avec des éléments imbriqués.
        /// </summary>
        [Fact]
        public void ParseXml_WithComplexXml_ReturnsXDocument()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element id=""1"">
        <child>value1</child>
    </element>
    <element id=""2"">
        <child>value2</child>
    </element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            var result = _parser.ParseXml(_testFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Root.Elements("element").Count());
            Assert.Equal("value1", result.Root.Element("element").Element("child").Value);
        }

        /// <summary>
        /// Vérifie que le parser peut traiter un fichier XML avec des namespaces.
        /// </summary>
        [Fact]
        public void ParseXml_WithNamespaces_ReturnsXDocument()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns:ns=""http://example.com"">
    <ns:element>test</ns:element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            var result = _parser.ParseXml(_testFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", result.Root.Element(XName.Get("element", "http://example.com")).Value);
        }

        #endregion

        #region Tests de Gestion des Erreurs

        /// <summary>
        /// Vérifie que le parser gère correctement un fichier XML invalide.
        /// </summary>
        [Fact]
        public void ParseXml_WithInvalidXml_ThrowsXmlParserException()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root><unclosed>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act & Assert
            Assert.Throws<XmlParserException>(() => _parser.ParseXml(_testFilePath));
        }

        /// <summary>
        /// Vérifie que le parser gère correctement un fichier vide.
        /// </summary>
        [Fact]
        public void ParseXml_WithEmptyFile_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, string.Empty);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _parser.ParseXml(_testFilePath));
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        /// <summary>
        /// Vérifie que le parser gère correctement un fichier inexistant.
        /// </summary>
        [Fact]
        public void ParseXml_WithNonExistentFile_ThrowsXmlParserException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.xml");

            // Act & Assert
            Assert.Throws<XmlParserException>(() => _parser.ParseXml(nonExistentPath));
        }

        /// <summary>
        /// Vérifie que le parser gère correctement un fichier invalide.
        /// </summary>
        [Fact]
        public void ParseXml_WithInvalidXml_ThrowsInvalidXml()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <unclosed>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act & Assert
            var exception = Assert.Throws<XmlParserException>(() => _parser.ParseXml(_testFilePath));
            Assert.Equal(XmlParserErrorType.InvalidXml, exception.ErrorType);
            Assert.Equal(_testFilePath, exception.FilePath);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("invalide")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        /// <summary>
        /// Vérifie que le parser gère correctement un fichier invalide.
        /// </summary>
        [Fact]
        public void ParseXml_WithInvalidXsd_ThrowsValidationError()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <invalid>test</invalid>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act & Assert
            var exception = Assert.Throws<XmlParserException>(() => _parser.ParseXml(_testFilePath));
            Assert.Equal(XmlParserErrorType.ValidationError, exception.ErrorType);
            Assert.Equal(_testFilePath, exception.FilePath);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ne respecte pas le schéma XSD")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region Tests de Performance

        /// <summary>
        /// Vérifie que le parser peut gérer un grand fichier XML.
        /// </summary>
        [Fact]
        public void ParseXml_WithLargeFile_HandlesCorrectly()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>";
            for (int i = 0; i < 1000; i++)
            {
                xmlContent += $@"
    <element id=""{i}"">
        <data>{new string('x', 100)}</data>
    </element>";
            }
            xmlContent += @"
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            var result = _parser.ParseXml(_testFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Root.Elements("element").Count());
        }

        #endregion

        #region Tests de Logging

        /// <summary>
        /// Vérifie que le parser enregistre correctement les logs de début et de succès.
        /// </summary>
        [Fact]
        public void ParseXml_LogsStartAndSuccess()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            _parser.ParseXml(_testFilePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Début du parsing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Parsing XML réussi")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion
    }
} 