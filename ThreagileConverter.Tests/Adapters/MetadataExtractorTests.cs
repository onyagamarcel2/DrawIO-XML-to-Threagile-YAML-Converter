using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Adapters;
using ThreagileConverter.Core.Models;
using Xunit;

namespace ThreagileConverter.Tests.Adapters;

public class MetadataExtractorTests
{
    private readonly Mock<ILogger<MetadataExtractor>> _loggerMock;
    private readonly MetadataExtractor _extractor;

    public MetadataExtractorTests()
    {
        _loggerMock = new Mock<ILogger<MetadataExtractor>>();
        _extractor = new MetadataExtractor(_loggerMock.Object);
    }

    [Fact]
    public void ExtractMetadata_WithValidXml_ReturnsMetadata()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"" name=""Test Diagram"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";

        var doc = XDocument.Parse(xml);

        // Act
        var result = _extractor.ExtractMetadata(doc);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Diagram", result.Title);
        Assert.Equal("Mozilla/5.0", result.Author);
        Assert.Equal("21.0.0", result.Version);
        Assert.Equal(DateTime.Parse("2024-01-01T00:00:00.000Z"), result.Modified);
        Assert.Equal("app.diagrams.net", result.Properties["host"]);
    }

    [Fact]
    public void ExtractMetadata_WithMissingAttributes_ReturnsPartialMetadata()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";

        var doc = XDocument.Parse(xml);

        // Act
        var result = _extractor.ExtractMetadata(doc);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Author);
        Assert.Equal(string.Empty, result.Version);
        Assert.Equal(default(DateTime), result.Modified);
    }

    [Fact]
    public void ExtractMetadata_WithInvalidXml_ThrowsException()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<invalid>
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</invalid>";

        var doc = XDocument.Parse(xml);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _extractor.ExtractMetadata(doc));
    }
} 