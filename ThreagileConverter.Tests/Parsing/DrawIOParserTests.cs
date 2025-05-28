using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Models;
using ThreagileConverter.Core.Parsing;
using Xunit;

namespace ThreagileConverter.Tests.Parsing;

public class DrawIOParserTests
{
    private readonly Mock<ILogger<DrawIOParser>> _loggerMock;
    private readonly DrawIOParser _parser;

    public DrawIOParserTests()
    {
        _loggerMock = new Mock<ILogger<DrawIOParser>>();
        _parser = new DrawIOParser(_loggerMock.Object);
    }

    [Fact]
    public void ExtractStyles_WithValidStyles_ExtractsCorrectly()
    {
        // Arrange
        var xml = @"
            <mxfile>
                <diagram>
                    <mxGraphModel>
                        <root>
                            <mxCell id='1' style='fillColor=#dae8fc;strokeColor=#6c8ebf;fontColor=#000000;fontSize=12;fontStyle=0;shape=rectangle;'/>
                            <mxCell id='2' style='fillColor=#d5e8d4;strokeColor=#82b366;fontColor=#000000;fontSize=12;fontStyle=1;shape=ellipse;'/>
                            <UserObject id='3' style='fillColor=#ffe6cc;strokeColor=#d79b00;fontColor=#000000;fontSize=14;fontStyle=2;shape=hexagon;'/>
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>";

        var element = XElement.Parse(xml);

        // Act
        var styles = _parser.ExtractStyles(element);

        // Assert
        Assert.Equal(3, styles.Count);
        
        // Vérifier le style de la première cellule
        var style1 = styles["1"];
        Assert.Equal("#dae8fc", style1.FillColor);
        Assert.Equal("#6c8ebf", style1.StrokeColor);
        Assert.Equal("#000000", style1.FontColor);
        Assert.Equal("12", style1.FontSize);
        Assert.Equal("0", style1.FontStyle);
        Assert.Equal("rectangle", style1.Shape);

        // Vérifier le style de la deuxième cellule
        var style2 = styles["2"];
        Assert.Equal("#d5e8d4", style2.FillColor);
        Assert.Equal("#82b366", style2.StrokeColor);
        Assert.Equal("ellipse", style2.Shape);
        Assert.Equal("1", style2.FontStyle);

        // Vérifier le style de l'objet utilisateur
        var style3 = styles["3"];
        Assert.Equal("#ffe6cc", style3.FillColor);
        Assert.Equal("#d79b00", style3.StrokeColor);
        Assert.Equal("14", style3.FontSize);
        Assert.Equal("2", style3.FontStyle);
        Assert.Equal("hexagon", style3.Shape);
    }

    [Fact]
    public void ExtractStyles_WithInvalidStyle_HandlesGracefully()
    {
        // Arrange
        var xml = @"
            <mxfile>
                <diagram>
                    <mxGraphModel>
                        <root>
                            <mxCell id='1' style='invalid-style'/>
                            <mxCell id='2' style='fillColor=#dae8fc;'/>
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>";

        var element = XElement.Parse(xml);

        // Act
        var styles = _parser.ExtractStyles(element);

        // Assert
        Assert.Single(styles);
        Assert.Equal("#dae8fc", styles["2"].FillColor);
    }

    [Fact]
    public void ExtractStyles_WithEmptyStyle_ReturnsEmptyDictionary()
    {
        // Arrange
        var xml = @"
            <mxfile>
                <diagram>
                    <mxGraphModel>
                        <root>
                            <mxCell id='1'/>
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>";

        var element = XElement.Parse(xml);

        // Act
        var styles = _parser.ExtractStyles(element);

        // Assert
        Assert.Empty(styles);
    }

    [Fact]
    public void ExtractMetadata_WithValidMetadata_ExtractsCorrectly()
    {
        // Arrange
        var xml = @"
            <mxfile>
                <diagram name='Test Diagram' description='Test Description' author='Test Author' 
                         created='2024-01-01' modified='2024-01-02' version='1.0' tags='tag1,tag2'
                         custom1='value1' custom2='value2'>
                    <mxGraphModel>
                        <root>
                            <mxCell id='1'/>
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>";

        var element = XElement.Parse(xml);

        // Act
        var metadata = _parser.ExtractMetadata(element);

        // Assert
        Assert.Equal("Test Diagram", metadata.Title);
        Assert.Equal("Test Description", metadata.Description);
        Assert.Equal("Test Author", metadata.Author);
        Assert.Equal("2024-01-01", metadata.Created);
        Assert.Equal("2024-01-02", metadata.Modified);
        Assert.Equal("1.0", metadata.Version);
        Assert.Equal(2, metadata.Tags.Count);
        Assert.Contains("tag1", metadata.Tags);
        Assert.Contains("tag2", metadata.Tags);
        Assert.Equal(2, metadata.Properties.Count);
        Assert.Equal("value1", metadata.Properties["custom1"]);
        Assert.Equal("value2", metadata.Properties["custom2"]);
    }

    [Fact]
    public void ExtractMetadata_WithMissingDiagram_ReturnsEmptyMetadata()
    {
        // Arrange
        var xml = @"
            <mxfile>
                <mxGraphModel>
                    <root>
                        <mxCell id='1'/>
                    </root>
                </mxGraphModel>
            </mxfile>";

        var element = XElement.Parse(xml);

        // Act
        var metadata = _parser.ExtractMetadata(element);

        // Assert
        Assert.Null(metadata.Title);
        Assert.Null(metadata.Description);
        Assert.Null(metadata.Author);
        Assert.Empty(metadata.Tags);
        Assert.Empty(metadata.Properties);
    }

    [Fact]
    public void ExtractMetadata_WithPartialMetadata_ExtractsAvailableData()
    {
        // Arrange
        var xml = @"
            <mxfile>
                <diagram name='Test Diagram' author='Test Author'>
                    <mxGraphModel>
                        <root>
                            <mxCell id='1'/>
                        </root>
                    </mxGraphModel>
                </diagram>
            </mxfile>";

        var element = XElement.Parse(xml);

        // Act
        var metadata = _parser.ExtractMetadata(element);

        // Assert
        Assert.Equal("Test Diagram", metadata.Title);
        Assert.Equal("Test Author", metadata.Author);
        Assert.Null(metadata.Description);
        Assert.Null(metadata.Created);
        Assert.Null(metadata.Modified);
        Assert.Null(metadata.Version);
        Assert.Empty(metadata.Tags);
        Assert.Empty(metadata.Properties);
    }

    [Fact]
    public void ExtractShapes_WithValidShapes_ExtractsCorrectly()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                        <rectangle id=""1"" x=""10"" y=""10"" width=""100"" height=""50"" value=""Asset 1"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf"">
                            <circle id=""2"" x=""20"" y=""20"" width=""30"" height=""30"" value=""Child 1"" style=""fillColor=#d5e8d4;strokeColor=#82b366""/>
                        </rectangle>
                        <ellipse id=""3"" x=""150"" y=""10"" width=""100"" height=""50"" value=""Asset 2"" style=""fillColor=#ffe6cc;strokeColor=#d79b00""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var shapes = _parser.ExtractShapes(element);

        // Assert
        Assert.Equal(2, shapes.Count);

        var asset1 = shapes[0];
        Assert.Equal("1", asset1.Id);
        Assert.Equal("rectangle", asset1.Type);
        Assert.Equal("Asset 1", asset1.Value);
        Assert.Equal(10, asset1.X);
        Assert.Equal(10, asset1.Y);
        Assert.Equal(100, asset1.Width);
        Assert.Equal(50, asset1.Height);
        Assert.Equal(1, asset1.Children.Count);

        var child1 = asset1.Children[0];
        Assert.Equal("2", child1.Id);
        Assert.Equal("circle", child1.Type);
        Assert.Equal("Child 1", child1.Value);
        Assert.Equal("1", child1.Parent);

        var asset2 = shapes[1];
        Assert.Equal("3", asset2.Id);
        Assert.Equal("ellipse", asset2.Type);
        Assert.Equal("Asset 2", asset2.Value);
        Assert.Empty(asset2.Children);
    }

    [Fact]
    public void ExtractShapes_WithMissingDiagram_ReturnsEmptyList()
    {
        // Arrange
        var xml = @"<mxfile></mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var shapes = _parser.ExtractShapes(element);

        // Assert
        Assert.Empty(shapes);
    }

    [Fact]
    public void ExtractShapes_WithInvalidStructure_ReturnsEmptyList()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <invalid>
                    <root>
                        <rectangle id=""1"" value=""Asset 1""/>
                    </root>
                </invalid>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var shapes = _parser.ExtractShapes(element);

        // Assert
        Assert.Empty(shapes);
    }

    [Fact]
    public void ExtractRelations_WithValidRelations_ExtractsCorrectly()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                        <mxCell id=""1"" source=""2"" target=""3"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"" points=""100,50;150,50;150,100;200,100""/>
                        <mxCell id=""2"" source=""4"" target=""5"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"" points=""300,50;350,50;350,100;400,100""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var relations = _parser.ExtractRelations(element);

        // Assert
        Assert.Equal(2, relations.Count);

        var relation1 = relations[0];
        Assert.Equal("1", relation1.Id);
        Assert.Equal("mxCell", relation1.Type);
        Assert.Equal("2", relation1.Source);
        Assert.Equal("3", relation1.Target);
        Assert.Equal(4, relation1.Points.Count);
        Assert.Equal(100, relation1.Points[0].X);
        Assert.Equal(50, relation1.Points[0].Y);
        Assert.Equal(200, relation1.Points[3].X);
        Assert.Equal(100, relation1.Points[3].Y);

        var relation2 = relations[1];
        Assert.Equal("2", relation2.Id);
        Assert.Equal("mxCell", relation2.Type);
        Assert.Equal("4", relation2.Source);
        Assert.Equal("5", relation2.Target);
        Assert.Equal(4, relation2.Points.Count);
        Assert.Equal(300, relation2.Points[0].X);
        Assert.Equal(50, relation2.Points[0].Y);
        Assert.Equal(400, relation2.Points[3].X);
        Assert.Equal(100, relation2.Points[3].Y);
    }

    [Fact]
    public void ExtractRelations_WithMissingDiagram_ReturnsEmptyList()
    {
        // Arrange
        var xml = @"<mxfile></mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var relations = _parser.ExtractRelations(element);

        // Assert
        Assert.Empty(relations);
    }

    [Fact]
    public void ExtractRelations_WithInvalidStructure_ReturnsEmptyList()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <invalid>
                    <root>
                        <mxCell id=""1"" source=""2"" target=""3""/>
                    </root>
                </invalid>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var relations = _parser.ExtractRelations(element);

        // Assert
        Assert.Empty(relations);
    }

    [Fact]
    public void ExtractRelations_WithInvalidCells_ReturnsEmptyList()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                        <mxCell id=""1"" source=""2""/>
                        <mxCell id=""2"" target=""3""/>
                        <mxCell id=""3""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var relations = _parser.ExtractRelations(element);

        // Assert
        Assert.Empty(relations);
    }
} 