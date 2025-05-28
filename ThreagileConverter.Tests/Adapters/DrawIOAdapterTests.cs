using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Adapters;
using ThreagileConverter.Core.Models;
using Xunit;

namespace ThreagileConverter.Tests.Adapters;

public class DrawIOAdapterTests
{
    private readonly Mock<ILogger<DrawIOAdapter>> _loggerMock;
    private readonly Mock<IMetadataExtractor> _metadataExtractorMock;
    private readonly DrawIOAdapter _adapter;

    public DrawIOAdapterTests()
    {
        _loggerMock = new Mock<ILogger<DrawIOAdapter>>();
        _metadataExtractorMock = new Mock<IMetadataExtractor>();
        _adapter = new DrawIOAdapter(_loggerMock.Object, _metadataExtractorMock.Object);
    }

    [Fact]
    public void ConvertToModel_WithValidXml_ReturnsModel()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
                <mxCell id=""2"" value=""Web Server"" style=""shape=web-server;fillColor=#dae8fc;strokeColor=#6c8ebf"" vertex=""1"" parent=""1"">
                    <mxGeometry x=""100"" y=""100"" width=""100"" height=""60""/>
                </mxCell>
                <mxCell id=""3"" value=""Database"" style=""shape=database;fillColor=#d5e8d4;strokeColor=#82b366"" vertex=""1"" parent=""1"">
                    <mxGeometry x=""300"" y=""100"" width=""100"" height=""60""/>
                </mxCell>
                <mxCell id=""4"" value="""" style=""endArrow=classic;html=1;exitX=1;exitY=0.5;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0"" edge=""1"" parent=""1"" source=""2"" target=""3"">
                    <mxGeometry width=""50"" height=""50"" relative=""1""/>
                </mxCell>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";

        var doc = XDocument.Parse(xml);
        var metadata = new Metadata
        {
            Title = "Test Diagram",
            Author = "Mozilla/5.0",
            Version = "21.0.0",
            Modified = DateTime.Parse("2024-01-01T00:00:00.000Z")
        };

        _metadataExtractorMock.Setup(x => x.ExtractMetadata(doc)).Returns(metadata);

        // Act
        var result = _adapter.ConvertToModel(doc);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(metadata, result.Metadata);
        Assert.Equal("app.diagrams.net", result.Properties["host"]);
        Assert.Equal("2024-01-01T00:00:00.000Z", result.Properties["modified"]);
        Assert.Equal("21.0.0", result.Properties["version"]);
    }

    [Fact]
    public void ExtractStyles_WithValidStyle_ReturnsStyle()
    {
        // Arrange
        var element = new XElement("mxCell",
            new XAttribute("style", "shape=web-server;fillColor=#dae8fc;strokeColor=#6c8ebf;fontColor=#000000;fontStyle=0;fontSize=12"));

        // Act
        var result = _adapter.ExtractStyles(element);

        // Assert
        Assert.Equal("web-server", result.Shape);
        Assert.Equal("#dae8fc", result.FillColor);
        Assert.Equal("#6c8ebf", result.StrokeColor);
        Assert.Equal("#000000", result.FontColor);
        Assert.Equal("0", result.FontStyle);
        Assert.Equal("12", result.FontSize);
    }

    [Fact]
    public void ProcessShapes_WithValidShapes_ProcessesShapes()
    {
        // Arrange
        var elements = new List<Element>
        {
            new XElement("mxCell",
                new XAttribute("id", "1"),
                new XAttribute("vertex", "1"),
                new XAttribute("value", "Web Server"),
                new XAttribute("style", "shape=web-server;fillColor=#dae8fc;strokeColor=#6c8ebf"))
        };

        // Act
        _adapter.ProcessShapes(elements);

        // Assert
        // Note: Since ProcessShapes is void and modifies internal state,
        // we can only verify it doesn't throw exceptions
    }

    [Fact]
    public void ProcessConnections_WithValidConnections_ProcessesConnections()
    {
        // Arrange
        var elements = new List<Element>
        {
            new XElement("mxCell",
                new XAttribute("id", "1"),
                new XAttribute("edge", "1"),
                new XAttribute("source", "2"),
                new XAttribute("target", "3"),
                new XAttribute("style", "endArrow=classic;html=1"))
        };

        // Act
        _adapter.ProcessConnections(elements);

        // Assert
        // Note: Since ProcessConnections is void and modifies internal state,
        // we can only verify it doesn't throw exceptions
    }
} 