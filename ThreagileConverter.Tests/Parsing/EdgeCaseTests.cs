using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Parsing;
using Xunit;
using System.Xml.Linq;
using System.Text;

namespace ThreagileConverter.Tests.Parsing
{
    /// <summary>
    /// Tests for edge cases in parsing, including empty files, invalid formats, and malformed XML.
    /// </summary>
    public class EdgeCaseTests : IDisposable
    {
        private readonly Mock<ILogger<XmlParser>> _mockLogger;
        private readonly XmlParser _xmlParser;
        private readonly Mock<ILogger<DrawIOParser>> _mockDrawIOLogger;
        private readonly DrawIOParser _drawIOParser;
        private readonly string _testFilePath;

        public EdgeCaseTests()
        {
            _mockLogger = new Mock<ILogger<XmlParser>>();
            _xmlParser = new XmlParser(_mockLogger.Object);
            _mockDrawIOLogger = new Mock<ILogger<DrawIOParser>>();
            _drawIOParser = new DrawIOParser(_mockDrawIOLogger.Object);
            _testFilePath = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public async Task ParseXml_WithEmptyFile_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, string.Empty);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => _xmlParser.ParseAsync(_testFilePath));
            Assert.Equal(XmlParserErrorType.EmptyFile, exception.ErrorType);
        }

        [Fact]
        public async Task ParseXml_WithWhitespaceOnlyFile_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "   \n\t   \r\n");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => _xmlParser.ParseAsync(_testFilePath));
            Assert.Equal(XmlParserErrorType.EmptyFile, exception.ErrorType);
        }

        [Fact]
        public async Task ParseXml_WithNonXmlFile_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "This is not an XML file");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => _xmlParser.ParseAsync(_testFilePath));
            Assert.Equal(XmlParserErrorType.InvalidXml, exception.ErrorType);
        }

        [Fact]
        public async Task ParseXml_WithMalformedXml_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "<root><unclosed>");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => _xmlParser.ParseAsync(_testFilePath));
            Assert.Equal(XmlParserErrorType.InvalidXml, exception.ErrorType);
        }

        [Fact]
        public async Task ParseXml_WithInvalidEncoding_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "<?xml version=\"1.0\" encoding=\"invalid\"?><root></root>");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => _xmlParser.ParseAsync(_testFilePath));
            Assert.Equal(XmlParserErrorType.InvalidXml, exception.ErrorType);
        }

        [Fact]
        public async Task ParseXml_WithInvalidNamespace_ThrowsException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "<root xmlns:prefix=\"invalid namespace\"></root>");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<XmlParserException>(() => _xmlParser.ParseAsync(_testFilePath));
            Assert.Equal(XmlParserErrorType.InvalidXml, exception.ErrorType);
        }

        [Fact]
        public void ParseDrawIO_WithEmptyDocument_ReturnsEmptyModel()
        {
            // Arrange
            var document = new XDocument(new XElement("mxfile"));

            // Act
            var model = _drawIOParser.Parse(document);

            // Assert
            Assert.NotNull(model);
            Assert.Empty(model.Shapes);
            Assert.Empty(model.Relations);
            Assert.NotNull(model.Metadata);
            Assert.Empty(model.Metadata.Tags);
            Assert.Empty(model.Metadata.Properties);
        }

        [Fact]
        public void ParseDrawIO_WithNullDocument_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _drawIOParser.Parse(null));
        }

        [Fact]
        public void ParseDrawIO_WithInvalidRoot_ThrowsArgumentException()
        {
            // Arrange
            var document = new XDocument(new XElement("invalid"));

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _drawIOParser.Parse(document));
            Assert.Contains("Expected root element 'mxfile'", exception.Message);
        }

        [Fact]
        public void ParseDrawIO_WithMissingDiagram_ReturnsEmptyModel()
        {
            // Arrange
            var document = new XDocument(
                new XElement("mxfile",
                    new XElement("something-else")
                )
            );

            // Act
            var model = _drawIOParser.Parse(document);

            // Assert
            Assert.NotNull(model);
            Assert.Empty(model.Shapes);
            Assert.Empty(model.Relations);
        }

        [Fact]
        public void ParseDrawIO_WithCorruptedStyleData_HandlesGracefully()
        {
            // Arrange
            var document = new XDocument(
                new XElement("mxfile",
                    new XElement("diagram",
                        new XElement("mxGraphModel",
                            new XElement("root",
                                new XElement("mxCell", new XAttribute("id", "0")),
                                new XElement("mxCell", new XAttribute("id", "1"), new XAttribute("parent", "0")),
                                new XElement("mxCell", 
                                    new XAttribute("id", "2"), 
                                    new XAttribute("parent", "1"),
                                    new XAttribute("style", "fillColor=#f8cecc;strokeColor=#b85450;corrupted="),
                                    new XAttribute("value", "Test")
                                )
                            )
                        )
                    )
                )
            );

            // Act
            var model = _drawIOParser.Parse(document);

            // Assert
            Assert.NotNull(model);
            Assert.Single(model.Shapes);
            var shape = model.Shapes[0];
            Assert.Equal("2", shape.Id);
            Assert.Equal("Test", shape.Value);
            Assert.NotNull(shape.Style);
            Assert.Equal("#f8cecc", shape.Style.FillColor);
            Assert.Equal("#b85450", shape.Style.StrokeColor);
        }

        [Fact]
        public void ParseDrawIO_WithExtremelyLargeValues_HandlesGracefully()
        {
            // Arrange
            var largeValue = new string('X', 1000000); // 1MB string
            var document = new XDocument(
                new XElement("mxfile",
                    new XElement("diagram",
                        new XElement("mxGraphModel",
                            new XElement("root",
                                new XElement("mxCell", new XAttribute("id", "0")),
                                new XElement("mxCell", new XAttribute("id", "1"), new XAttribute("parent", "0")),
                                new XElement("mxCell", 
                                    new XAttribute("id", "2"), 
                                    new XAttribute("parent", "1"),
                                    new XAttribute("value", largeValue)
                                )
                            )
                        )
                    )
                )
            );

            // Act
            var model = _drawIOParser.Parse(document);

            // Assert
            Assert.NotNull(model);
            Assert.Single(model.Shapes);
            var shape = model.Shapes[0];
            Assert.Equal("2", shape.Id);
            Assert.Equal(largeValue, shape.Value);
        }

        [Fact]
        public void ParseDrawIO_WithInvalidGeometry_HandlesGracefully()
        {
            // Arrange
            var document = new XDocument(
                new XElement("mxfile",
                    new XElement("diagram",
                        new XElement("mxGraphModel",
                            new XElement("root",
                                new XElement("mxCell", new XAttribute("id", "0")),
                                new XElement("mxCell", new XAttribute("id", "1"), new XAttribute("parent", "0")),
                                new XElement("mxCell", 
                                    new XAttribute("id", "2"), 
                                    new XAttribute("parent", "1"),
                                    new XAttribute("value", "Test"),
                                    new XElement("mxGeometry", 
                                        new XAttribute("x", "invalid"),
                                        new XAttribute("y", "invalid"),
                                        new XAttribute("width", "invalid"),
                                        new XAttribute("height", "invalid")
                                    )
                                )
                            )
                        )
                    )
                )
            );

            // Act
            var model = _drawIOParser.Parse(document);

            // Assert
            Assert.NotNull(model);
            Assert.Single(model.Shapes);
            var shape = model.Shapes[0];
            Assert.Equal("2", shape.Id);
            Assert.Equal("Test", shape.Value);
        }

        [Fact]
        public void ParseDrawIO_WithBinaryData_HandlesGracefully()
        {
            // Arrange
            byte[] binaryData = new byte[1000];
            new Random().NextBytes(binaryData);
            string base64Data = Convert.ToBase64String(binaryData);
            
            var document = new XDocument(
                new XElement("mxfile",
                    new XElement("diagram",
                        base64Data
                    )
                )
            );

            // Act & Assert
            try
            {
                var model = _drawIOParser.Parse(document);
                Assert.NotNull(model);
                Assert.Empty(model.Shapes);
                Assert.Empty(model.Relations);
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Parser should handle binary data gracefully but threw: {ex.Message}");
            }
        }
    }
} 