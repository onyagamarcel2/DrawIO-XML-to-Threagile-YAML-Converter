using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Validation;
using Xunit;

namespace ThreagileConverter.Tests.Validation;

public class DrawIOValidatorTests
{
    private readonly DrawIOValidator _validator;
    private readonly Mock<ILogger<DrawIOValidator>> _loggerMock;

    public DrawIOValidatorTests()
    {
        _loggerMock = new Mock<ILogger<DrawIOValidator>>();
        _validator = new DrawIOValidator(_loggerMock.Object);
    }

    [Fact]
    public void Validate_WithValidStructure_ReturnsNoErrors()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                        <rectangle id=""1"" x=""10"" y=""10"" width=""100"" height=""50"" value=""Asset 1"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf""/>
                        <mxCell id=""2"" source=""1"" target=""3"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"" points=""100,50;150,50;150,100;200,100""/>
                        <rectangle id=""3"" x=""200"" y=""100"" width=""100"" height=""50"" value=""Asset 2"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var errors = _validator.Validate(element);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithMissingDiagram_ReturnsError()
    {
        // Arrange
        var xml = @"<mxfile></mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var errors = _validator.Validate(element);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Élément diagram manquant", errors[0].Message);
        Assert.Equal(ValidationSeverity.Error, errors[0].Severity);
    }

    [Fact]
    public void Validate_WithMissingMxGraphModel_ReturnsError()
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
        var errors = _validator.Validate(element);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Élément mxGraphModel manquant", errors[0].Message);
        Assert.Equal(ValidationSeverity.Error, errors[0].Severity);
    }

    [Fact]
    public void Validate_WithInvalidShape_ReturnsErrors()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                        <rectangle id="""" x=""invalid"" y=""10"" width=""100"" height=""50"" value="""" style=""""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var errors = _validator.Validate(element);

        // Assert
        Assert.Equal(5, errors.Count);
        Assert.Contains(errors, e => e.Message == "Forme sans identifiant" && e.Severity == ValidationSeverity.Error);
        Assert.Contains(errors, e => e.Message.Contains("sans valeur") && e.Severity == ValidationSeverity.Warning);
        Assert.Contains(errors, e => e.Message.Contains("sans style") && e.Severity == ValidationSeverity.Warning);
        Assert.Contains(errors, e => e.Message.Contains("coordonnée X invalide") && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_WithInvalidRelation_ReturnsErrors()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                        <rectangle id=""1"" x=""10"" y=""10"" width=""100"" height=""50"" value=""Asset 1"" style=""fillColor=#dae8fc;strokeColor=#6c8ebf""/>
                        <mxCell id="""" source=""1"" target=""3"" style="""" points=""invalid""/>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var errors = _validator.Validate(element);

        // Assert
        Assert.Equal(3, errors.Count);
        Assert.Contains(errors, e => e.Message == "Relation sans identifiant" && e.Severity == ValidationSeverity.Error);
        Assert.Contains(errors, e => e.Message.Contains("cible 3 inexistante") && e.Severity == ValidationSeverity.Error);
        Assert.Contains(errors, e => e.Message.Contains("points de contrôle invalides") && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_WithEmptyDiagram_ReturnsWarning()
    {
        // Arrange
        var xml = @"<mxfile>
            <diagram>
                <mxGraphModel>
                    <root>
                    </root>
                </mxGraphModel>
            </diagram>
        </mxfile>";
        var element = XElement.Parse(xml);

        // Act
        var errors = _validator.Validate(element);

        // Assert
        Assert.Single(errors);
        Assert.Equal("Aucun élément dans le diagramme", errors[0].Message);
        Assert.Equal(ValidationSeverity.Warning, errors[0].Severity);
    }
} 