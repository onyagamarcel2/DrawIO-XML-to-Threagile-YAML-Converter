using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ThreagileConverter.Core.Parsing;
using System.Collections.Generic;

namespace ThreagileConverter.Tests.Parsing
{
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

        [Fact]
        public void ParseXml_WithInvalidXml_ThrowsXmlParserException()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root><unclosed>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act & Assert
            Assert.Throws<XmlParserException>(() => _parser.ParseXml(_testFilePath));
        }

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

        [Fact]
        public void ParseXml_WithNonExistentFile_ThrowsXmlParserException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.xml");

            // Act & Assert
            Assert.Throws<XmlParserException>(() => _parser.ParseXml(nonExistentPath));
        }

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

        [Fact]
        public void ValidateAgainstXsd_WithMissingXsdFile_ThrowsFileNotFound()
        {
            // Arrange
            var parser = new XmlParser(_loggerMock.Object);
            var doc = XDocument.Parse("<root/>");

            // Act & Assert
            var exception = Assert.Throws<XmlParserException>(() => parser.ValidateAgainstXsd(doc));
            Assert.Equal(XmlParserErrorType.FileNotFound, exception.ErrorType);
            // Le chemin du fichier XSD attendu peut être adapté selon l'implémentation réelle
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("n'existe pas")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void ExtractExternalReferences_WithError_ThrowsExternalReferenceError()
        {
            // Arrange
            var doc = XDocument.Parse("<root/>");
            var mockParser = new Mock<XmlParser>(_loggerMock.Object);
            mockParser.Setup(p => p.ExtractExternalReferences(It.IsAny<XDocument>()))
                .Throws(new Exception("Test error"));

            // Act & Assert
            var exception = Assert.Throws<XmlParserException>(() => mockParser.Object.ExtractExternalReferences(doc));
            Assert.Equal(XmlParserErrorType.ExternalReferenceError, exception.ErrorType);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("références externes")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void ParseXml_WithAccessDenied_ThrowsFileAccessDenied()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            File.SetAttributes(_testFilePath, FileAttributes.ReadOnly);

            try
            {
                // Act & Assert
                var exception = Assert.Throws<XmlParserException>(() => _parser.ParseXml(_testFilePath));
                Assert.Equal(XmlParserErrorType.FileAccessDenied, exception.ErrorType);
                Assert.Equal(_testFilePath, exception.FilePath);
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Accès refusé")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            }
            finally
            {
                File.SetAttributes(_testFilePath, FileAttributes.Normal);
            }
        }

        #endregion

        #region Tests de Performance

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

        [Fact]
        public void ParseXml_LogsStartAndSuccess()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
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

        #region Tests de Namespaces

        [Fact]
        public void ExtractNamespaces_WithNoNamespaces_ReturnsEmpty()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var namespaces = _parser.ExtractNamespaces(doc);

            // Assert
            Assert.Empty(namespaces);
        }

        [Fact]
        public void ExtractNamespaces_WithSingleNamespace_ReturnsNamespace()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns:ns=""http://example.com"">
    <ns:element>test</ns:element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var namespaces = _parser.ExtractNamespaces(doc).ToList();

            // Assert
            Assert.Single(namespaces);
            Assert.Equal("http://example.com", namespaces[0].ToString());
        }

        [Fact]
        public void ExtractNamespaces_WithMultipleNamespaces_ReturnsAllNamespaces()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns:ns1=""http://example1.com"" xmlns:ns2=""http://example2.com"">
    <ns1:element>test1</ns1:element>
    <ns2:element>test2</ns2:element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var namespaces = _parser.ExtractNamespaces(doc).ToList();

            // Assert
            Assert.Equal(2, namespaces.Count);
            Assert.Contains("http://example1.com", namespaces.Select(n => n.ToString()));
            Assert.Contains("http://example2.com", namespaces.Select(n => n.ToString()));
        }

        [Fact]
        public void IsNamespaceDeclared_WithDeclaredNamespace_ReturnsTrue()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns:ns=""http://example.com"">
    <ns:element>test</ns:element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var isDeclared = _parser.IsNamespaceDeclared(doc, "http://example.com");

            // Assert
            Assert.True(isDeclared);
        }

        [Fact]
        public void IsNamespaceDeclared_WithUndeclaredNamespace_ReturnsFalse()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var isDeclared = _parser.IsNamespaceDeclared(doc, "http://example.com");

            // Assert
            Assert.False(isDeclared);
        }

        [Fact]
        public void GetNamespacePrefix_WithDeclaredNamespace_ReturnsPrefix()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns:ns=""http://example.com"">
    <ns:element>test</ns:element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var prefix = _parser.GetNamespacePrefix(doc, "http://example.com");

            // Assert
            Assert.Equal("ns", prefix);
        }

        [Fact]
        public void GetNamespacePrefix_WithUndeclaredNamespace_ReturnsNull()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var prefix = _parser.GetNamespacePrefix(doc, "http://example.com");

            // Assert
            Assert.Null(prefix);
        }

        [Fact]
        public void ParseXml_WithNamespaces_LogsNamespaces()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root xmlns:ns=""http://example.com"">
    <ns:element>test</ns:element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            _parser.ParseXml(_testFilePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Namespaces trouvés")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        #endregion

        #region Tests de Validation XSD

        [Fact]
        public void ValidateAgainstXsd_WithValidXml_ReturnsValid()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"" name=""Test Diagram"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var result = _parser.ValidateAgainstXsd(doc);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateAgainstXsd_WithInvalidXml_ReturnsInvalid()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <invalid>test</invalid>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var result = _parser.ValidateAgainstXsd(doc);

            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void ValidateAgainstXsd_WithMissingRequiredAttribute_ReturnsInvalid()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var result = _parser.ValidateAgainstXsd(doc);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("host"));
        }

        [Fact]
        public void ValidateAgainstXsd_WithInvalidVersion_ReturnsInvalid()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""invalid"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var result = _parser.ValidateAgainstXsd(doc);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("version"));
        }

        [Fact]
        public void ValidateAgainstXsd_WithInvalidDateTime_ReturnsInvalid()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""invalid"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var result = _parser.ValidateAgainstXsd(doc);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("modified"));
        }

        #endregion

        #region Tests de Références Externes

        [Fact]
        public void ExtractExternalReferences_WithNoReferences_ReturnsEmpty()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var references = _parser.ExtractExternalReferences(doc);

            // Assert
            Assert.Empty(references);
        }

        [Fact]
        public void ExtractExternalReferences_WithHrefReference_ReturnsReference()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" href=""https://example.com/image.png""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var references = _parser.ExtractExternalReferences(doc).ToList();

            // Assert
            Assert.Single(references);
            Assert.Equal("https://example.com/image.png", references[0].ReferencePath);
            Assert.Equal(ExternalReferenceType.Href, references[0].ReferenceType);
        }

        [Fact]
        public void ExtractExternalReferences_WithSrcReference_ReturnsReference()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" src=""images/icon.png""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var references = _parser.ExtractExternalReferences(doc).ToList();

            // Assert
            Assert.Single(references);
            Assert.Equal("images/icon.png", references[0].ReferencePath);
            Assert.Equal(ExternalReferenceType.Source, references[0].ReferenceType);
        }

        [Fact]
        public void ExtractExternalReferences_WithDataUriReference_ReturnsReference()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" data=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="" />
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            File.WriteAllText(_testFilePath, xmlContent);
            var doc = _parser.ParseXml(_testFilePath);

            // Act
            var references = _parser.ExtractExternalReferences(doc).ToList();

            // Assert
            Assert.Single(references);
            Assert.StartsWith("data:image/png;base64,", references[0].ReferencePath);
            Assert.Equal(ExternalReferenceType.DataUri, references[0].ReferenceType);
        }

        [Fact]
        public void IsExternalReferenceValid_WithValidHref_ReturnsTrue()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "https://example.com/image.png",
                ReferenceType = ExternalReferenceType.Href
            };

            // Act
            var isValid = _parser.IsExternalReferenceValid(reference);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsExternalReferenceValid_WithInvalidHref_ReturnsFalse()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "invalid://url",
                ReferenceType = ExternalReferenceType.Href
            };

            // Act
            var isValid = _parser.IsExternalReferenceValid(reference);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsExternalReferenceValid_WithValidDataUri_ReturnsTrue()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
                ReferenceType = ExternalReferenceType.DataUri
            };

            // Act
            var isValid = _parser.IsExternalReferenceValid(reference);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsExternalReferenceValid_WithInvalidDataUri_ReturnsFalse()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "data:invalid",
                ReferenceType = ExternalReferenceType.DataUri
            };

            // Act
            var isValid = _parser.IsExternalReferenceValid(reference);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ResolveExternalReference_WithAbsolutePath_ReturnsSamePath()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "https://example.com/image.png",
                ReferenceType = ExternalReferenceType.Href
            };

            // Act
            var resolvedPath = _parser.ResolveExternalReference(reference, @"C:\base\path");

            // Assert
            Assert.Equal("https://example.com/image.png", resolvedPath);
        }

        [Fact]
        public void ResolveExternalReference_WithRelativePath_ReturnsAbsolutePath()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "images/icon.png",
                ReferenceType = ExternalReferenceType.Source
            };

            // Act
            var resolvedPath = _parser.ResolveExternalReference(reference, @"C:\base\path");

            // Assert
            Assert.Equal(Path.GetFullPath(@"C:\base\path\images\icon.png"), resolvedPath);
        }

        [Fact]
        public void ResolveExternalReference_WithDataUri_ReturnsSameUri()
        {
            // Arrange
            var reference = new ExternalReference
            {
                ReferencePath = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
                ReferenceType = ExternalReferenceType.DataUri
            };

            // Act
            var resolvedPath = _parser.ResolveExternalReference(reference, @"C:\base\path");

            // Assert
            Assert.Equal(reference.ReferencePath, resolvedPath);
        }

        #endregion

        #region Tests de Streaming

        [Fact]
        public async Task ParseXmlStreamingAsync_WithValidXml_ReturnsXDocument()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act
            var result = await _parser.ParseXmlStreamingAsync(_testFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("root", result.Root.Name.LocalName);
            Assert.Equal("test", result.Root.Element("element").Value);
        }

        [Fact]
        public async Task ParseXmlStreamingAsync_WithLargeFile_HandlesCorrectly()
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
            var result = await _parser.ParseXmlStreamingAsync(_testFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Root.Elements("element").Count());
        }

        [Fact]
        public async Task ParseXmlStreamingAsync_WithNonExistentFile_ThrowsFileNotFound()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => 
                _parser.ParseXmlStreamingAsync(nonExistentPath));
            Assert.Equal(XmlParserErrorType.FileNotFound, exception.ErrorType);
            Assert.Equal(nonExistentPath, exception.FilePath);
        }

        [Fact]
        public async Task ProcessXmlStreamingAsync_WithValidXml_ProcessesAllElements()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element id=""1"">test1</element>
    <element id=""2"">test2</element>
    <element id=""3"">test3</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var processedElements = new List<string>();

            // Act
            await _parser.ProcessXmlStreamingAsync(_testFilePath, async element =>
            {
                processedElements.Add(element.Value);
                await Task.CompletedTask;
            }, "element");

            // Assert
            Assert.Equal(3, processedElements.Count);
            Assert.Contains("test1", processedElements);
            Assert.Contains("test2", processedElements);
            Assert.Contains("test3", processedElements);
        }

        [Fact]
        public async Task ProcessXmlStreamingAsync_WithSpecificElement_ProcessesOnlyMatchingElements()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element id=""1"">test1</element>
    <other>test2</other>
    <element id=""2"">test3</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);
            var processedElements = new List<string>();

            // Act
            await _parser.ProcessXmlStreamingAsync(_testFilePath, async element =>
            {
                processedElements.Add(element.Value);
                await Task.CompletedTask;
            }, "element");

            // Assert
            Assert.Equal(2, processedElements.Count);
            Assert.Contains("test1", processedElements);
            Assert.Contains("test3", processedElements);
            Assert.DoesNotContain("test2", processedElements);
        }

        [Fact]
        public async Task ProcessXmlStreamingAsync_WithErrorInHandler_PropagatesError()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(_testFilePath, xmlContent);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _parser.ProcessXmlStreamingAsync(_testFilePath, async element =>
                {
                    throw new Exception("Test error");
                });
            });
        }

        [Fact]
        public async Task ProcessXmlStreamingAsync_WithLargeFile_HandlesCorrectly()
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
            var processedCount = 0;

            // Act
            await _parser.ProcessXmlStreamingAsync(_testFilePath, async element =>
            {
                processedCount++;
                await Task.CompletedTask;
            }, "element");

            // Assert
            Assert.Equal(1000, processedCount);
        }

        #endregion
    }
} 