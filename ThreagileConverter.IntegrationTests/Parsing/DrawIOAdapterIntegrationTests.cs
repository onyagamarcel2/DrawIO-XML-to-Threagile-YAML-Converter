using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Models;
using ThreagileConverter.Core.Parsing;
using ThreagileConverter.Core.Validation;
using Xunit;

namespace ThreagileConverter.IntegrationTests.Parsing;

public class DrawIOAdapterIntegrationTests
{
    private readonly DrawIOParser _parser;
    private readonly DrawIOValidator _validator;
    private readonly Mock<ILogger<DrawIOParser>> _parserLoggerMock;
    private readonly Mock<ILogger<DrawIOValidator>> _validatorLoggerMock;

    public DrawIOAdapterIntegrationTests()
    {
        _parserLoggerMock = new Mock<ILogger<DrawIOParser>>();
        _validatorLoggerMock = new Mock<ILogger<DrawIOValidator>>();
        _parser = new DrawIOParser(_parserLoggerMock.Object);
        _validator = new DrawIOValidator(_validatorLoggerMock.Object);
    }

    [Fact]
    public void ProcessComplexDiagram_WithValidData_ProcessesCorrectly()
    {
        // Arrange
        var xml = CreateComplexDiagramXml();
        var element = XElement.Parse(xml);

        // Act
        var metadata = _parser.ExtractMetadata(element);
        var styles = _parser.ExtractStyles(element);
        var shapes = _parser.ExtractShapes(element);
        var relations = _parser.ExtractRelations(element);
        var validationErrors = _validator.Validate(element);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("Test Diagram", metadata.Title);
        Assert.Equal("Test Description", metadata.Description);
        Assert.Equal("Test Author", metadata.Author);

        Assert.NotEmpty(styles);
        Assert.Contains(styles, s => s.FillColor == "#dae8fc" && s.StrokeColor == "#6c8ebf");

        Assert.Equal(3, shapes.Count);
        var asset1 = shapes.First(s => s.Id == "1");
        Assert.Equal("rectangle", asset1.Type);
        Assert.Equal("Asset 1", asset1.Value);
        Assert.Equal(1, asset1.Children.Count);

        Assert.Equal(2, relations.Count);
        var relation1 = relations.First(r => r.Id == "2");
        Assert.Equal("1", relation1.Source);
        Assert.Equal("3", relation1.Target);
        Assert.Equal(4, relation1.Points.Count);

        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ProcessComplexDiagram_WithInvalidData_ReportsErrors()
    {
        // Arrange
        var xml = CreateInvalidDiagramXml();
        var element = XElement.Parse(xml);

        // Act
        var metadata = _parser.ExtractMetadata(element);
        var styles = _parser.ExtractStyles(element);
        var shapes = _parser.ExtractShapes(element);
        var relations = _parser.ExtractRelations(element);
        var validationErrors = _validator.Validate(element);

        // Assert
        Assert.NotNull(metadata);
        Assert.Empty(styles);
        Assert.Empty(shapes);
        Assert.Empty(relations);
        Assert.NotEmpty(validationErrors);
        Assert.Contains(validationErrors, e => e.Message == "Élément mxGraphModel manquant");
    }

    [Fact]
    public void ProcessComplexDiagram_WithPartialData_ProcessesCorrectly()
    {
        // Arrange
        var xml = CreatePartialDiagramXml();
        var element = XElement.Parse(xml);

        // Act
        var metadata = _parser.ExtractMetadata(element);
        var styles = _parser.ExtractStyles(element);
        var shapes = _parser.ExtractShapes(element);
        var relations = _parser.ExtractRelations(element);
        var validationErrors = _validator.Validate(element);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("Test Diagram", metadata.Title);
        Assert.Null(metadata.Description);
        Assert.Null(metadata.Author);

        Assert.NotEmpty(styles);
        Assert.Single(shapes);
        Assert.Empty(relations);
        Assert.Contains(validationErrors, e => e.Message == "Aucun élément dans le diagramme");
    }

    private string CreateComplexDiagramXml()
    {
        return @"<mxfile>
            <diagram title=""Test Diagram"" description=""Test Description"" author=""Test Author"">
                <mxGraphModel>
                    <root>
                        <rectangle id=""1"" x=""10"" y=""10"" width=""100"" height=""50"" value=""Asset 1"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf"">
                            <circle id=""2"" x=""20"" y=""20"" width=""30"" height=""30"" value=""Child 1"" style=""fillColor=#d5e8d4;strokeColor=#82b366""/>
                        </rectangle>
                        <mxCell id=""3"" source=""1"" target=""4"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"" points=""100,50;150,50;150,100;200,100""/>
                        <rectangle id=""4"" x=""200"" y=""100"" width=""100"" height=""50"" value=""Asset 2"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf""/>
                        <mxCell id=""5"" source=""4"" target=""6"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"" points=""300,50;350,50;350,100;400,100""/>
                        <rectangle id=""6"" x=""400"" y=""100"" width=""100"" height=""50"" value=""Asset 3"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
    }

    private string CreateInvalidDiagramXml()
    {
        return @"<mxfile>
            <diagram title=""Test Diagram"">
                <invalid>
                    <root>
                        <rectangle id=""1"" value=""Asset 1""/>
                    </root>
                </invalid>
            </diagram>
        </mxfile>";
    }

    private string CreatePartialDiagramXml()
    {
        return @"<mxfile>
            <diagram title=""Test Diagram"">
                <mxGraphModel>
                    <root>
                        <rectangle id=""1"" x=""10"" y=""10"" width=""100"" height=""50"" value=""Asset 1"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
    }
} 