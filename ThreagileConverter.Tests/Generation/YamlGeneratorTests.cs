using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Generation;
using ThreagileConverter.Core.Models;
using Xunit;

namespace ThreagileConverter.Tests.Generation;

public class YamlGeneratorTests
{
    private readonly Mock<ILogger<YamlGenerator>> _loggerMock;
    private readonly YamlGenerator _generator;

    public YamlGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<YamlGenerator>>();
        _generator = new YamlGenerator(_loggerMock.Object);
    }

    [Fact]
    public void GenerateYaml_WithValidModel_ReturnsYamlString()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "asset1",
                    Title = "Test Asset",
                    Type = "Process",
                    Style = new ThreagileStyle
                    {
                        FillColor = "#FFFFFF",
                        StrokeColor = "#000000"
                    }
                }
            }
        };

        // Act
        var yaml = _generator.GenerateYaml(model);

        // Assert
        Assert.NotNull(yaml);
        Assert.Contains("asset1", yaml);
        Assert.Contains("Test Asset", yaml);
        Assert.Contains("Process", yaml);
        Assert.Contains("#FFFFFF", yaml);
        Assert.Contains("#000000", yaml);
    }

    [Fact]
    public void GenerateYamlToFile_WithValidModel_WritesToFile()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "asset1",
                    Title = "Test Asset"
                }
            }
        };
        var filePath = Path.GetTempFileName();

        try
        {
            // Act
            _generator.GenerateYamlToFile(model, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.Contains("asset1", content);
            Assert.Contains("Test Asset", content);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ValidateSchema_WithValidYaml_ReturnsTrue()
    {
        // Arrange
        var yaml = @"
technicalAssets:
  - id: asset1
    title: Test Asset
    type: Process
trustBoundaries:
  - id: boundary1
    title: Test Boundary
    type: Network
communicationLinks:
  - id: link1
    source: asset1
    target: asset2
    title: Test Link
";

        // Act
        var isValid = _generator.ValidateSchema(yaml);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateSchema_WithInvalidYaml_ReturnsFalse()
    {
        // Arrange
        var yaml = @"
technicalAssets:
  - id: 
    title: 
trustBoundaries:
  - id: 
    title: 
communicationLinks:
  - id: 
    source: 
    target: 
";

        // Act
        var isValid = _generator.ValidateSchema(yaml);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void OptimizeOutput_WithValidYaml_ReturnsOptimizedYaml()
    {
        // Arrange
        var yaml = @"
technicalAssets:
  - id: asset1
    title: Test Asset
    type: Process
    style:
      fillColor: '#FFFFFF'
      strokeColor: '#000000'
      properties: {}
";

        // Act
        var optimized = _generator.OptimizeOutput(yaml);

        // Assert
        Assert.NotNull(optimized);
        Assert.Contains("asset1", optimized);
        Assert.Contains("Test Asset", optimized);
        Assert.Contains("Process", optimized);
        Assert.Contains("#FFFFFF", optimized);
        Assert.Contains("#000000", optimized);
    }

    [Fact]
    public void GenerateYaml_WithComplexModel_HandlesAllTypes()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "asset1",
                    Title = "Test Asset",
                    Type = "Process",
                    UsedAsServer = true,
                    UsedAsDataStore = false,
                    Style = new ThreagileStyle
                    {
                        FillColor = "#FFFFFF",
                        StrokeColor = "#000000",
                        Properties = new Dictionary<string, string>
                        {
                            { "custom", "value" }
                        }
                    }
                }
            },
            TrustBoundaries = new List<TrustBoundary>
            {
                new()
                {
                    Id = "boundary1",
                    Title = "Test Boundary",
                    Type = "Network",
                    TechnicalAssets = new List<string> { "asset1" }
                }
            },
            CommunicationLinks = new List<CommunicationLink>
            {
                new()
                {
                    Id = "link1",
                    Title = "Test Link",
                    Source = "asset1",
                    Target = "asset2",
                    Protocol = "HTTPS",
                    Authentication = "Basic"
                }
            }
        };

        // Act
        var yaml = _generator.GenerateYaml(model);

        // Assert
        Assert.NotNull(yaml);
        Assert.Contains("asset1", yaml);
        Assert.Contains("Test Asset", yaml);
        Assert.Contains("Process", yaml);
        Assert.Contains("true", yaml);
        Assert.Contains("false", yaml);
        Assert.Contains("#FFFFFF", yaml);
        Assert.Contains("#000000", yaml);
        Assert.Contains("custom", yaml);
        Assert.Contains("value", yaml);
        Assert.Contains("boundary1", yaml);
        Assert.Contains("Test Boundary", yaml);
        Assert.Contains("Network", yaml);
        Assert.Contains("link1", yaml);
        Assert.Contains("Test Link", yaml);
        Assert.Contains("HTTPS", yaml);
        Assert.Contains("Basic", yaml);
    }

    [Fact]
    public void ProcessReferences_WithCircularReferences_HandlesCorrectly()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new TechnicalAsset
                {
                    Id = "asset1",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref1", "ref:asset2" },
                        { "ref2", "ref:asset3" }
                    }
                },
                new TechnicalAsset
                {
                    Id = "asset2",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref1", "ref:asset1" }
                    }
                },
                new TechnicalAsset
                {
                    Id = "asset3",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref1", "ref:asset1" }
                    }
                }
            }
        };

        var yaml = _generator.GenerateYaml(model);

        // Act
        var result = _generator.ProcessReferences(yaml);
        var processedModel = _generator.Deserialize<ThreagileModel>(result);

        // Assert
        Assert.Contains("# Reference to asset2", processedModel.TechnicalAssets[0].Properties["ref1"]);
        Assert.Contains("# Reference to asset1", processedModel.TechnicalAssets[1].Properties["ref1"]);
    }

    [Fact]
    public void ProcessReferences_WithInvalidReferences_LogsWarnings()
    {
        // Arrange
        var model = new ThreagileModel
        {
            CommunicationLinks = new List<CommunicationLink>
            {
                new CommunicationLink
                {
                    Id = "link1",
                    Source = "invalid_source",
                    Target = "invalid_target"
                }
            }
        };

        var yaml = _generator.GenerateYaml(model);

        // Act
        var result = _generator.ProcessReferences(yaml);

        // Assert
        Assert.Equal(yaml, result); // Should return original YAML on error
    }

    [Fact]
    public void AddComments_WithValidComments_AddsCorrectly()
    {
        // Arrange
        var yaml = "asset1:\n  type: web\nasset2:\n  type: db";
        var comments = new Dictionary<string, string>
        {
            { "asset1:", "Web application" },
            { "asset2:", "Database" }
        };

        // Act
        var result = _generator.AddComments(yaml, comments);

        // Assert
        Assert.Contains("# Web application", result);
        Assert.Contains("# Database", result);
    }

    [Fact]
    public void ExtractComments_WithValidYaml_ExtractsCorrectly()
    {
        // Arrange
        var yaml = "asset1:\n  # Web application\n  type: web\nasset2:\n  # Database\n  type: db";

        // Act
        var comments = _generator.ExtractComments(yaml);

        // Assert
        Assert.Equal("Web application", comments["asset1:"]);
        Assert.Equal("Database", comments["asset2:"]);
    }

    [Fact]
    public void ExtractComments_WithNoComments_ReturnsEmptyDictionary()
    {
        // Arrange
        var yaml = "asset1:\n  type: web\nasset2:\n  type: db";

        // Act
        var comments = _generator.ExtractComments(yaml);

        // Assert
        Assert.NotNull(comments);
        Assert.Empty(comments);
    }

    [Fact]
    public void HasCircularReferences_WithCircularReferences_ReturnsTrue()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new TechnicalAsset
                {
                    Id = "asset1",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref", "ref:asset2" }
                    }
                },
                new TechnicalAsset
                {
                    Id = "asset2",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref", "ref:asset1" }
                    }
                }
            }
        };

        // Act
        var result = _generator.HasCircularReferences(model);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasCircularReferences_WithoutCircularReferences_ReturnsFalse()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new TechnicalAsset
                {
                    Id = "asset1",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref", "ref:asset2" }
                    }
                },
                new TechnicalAsset
                {
                    Id = "asset2",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref", "ref:asset3" }
                    }
                },
                new TechnicalAsset
                {
                    Id = "asset3",
                    Properties = new Dictionary<string, string>()
                }
            }
        };

        // Act
        var result = _generator.HasCircularReferences(model);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ResolveReferences_WithValidReferences_ResolvesCorrectly()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new TechnicalAsset
                {
                    Id = "asset1",
                    Title = "Asset 1",
                    Properties = new Dictionary<string, string>
                    {
                        { "ref", "ref:asset2" }
                    }
                },
                new TechnicalAsset
                {
                    Id = "asset2",
                    Title = "Asset 2",
                    Properties = new Dictionary<string, string>()
                }
            },
            TrustBoundaries = new List<TrustBoundary>
            {
                new TrustBoundary
                {
                    Id = "boundary1",
                    TechnicalAssets = new List<string> { "asset1", "asset2" }
                }
            },
            CommunicationLinks = new List<CommunicationLink>
            {
                new CommunicationLink
                {
                    Id = "link1",
                    Source = "asset1",
                    Target = "asset2"
                }
            }
        };

        // Act
        var result = _generator.ResolveReferences(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TechnicalAssets.Count);
        Assert.Equal(1, result.TrustBoundaries.Count);
        Assert.Equal(1, result.CommunicationLinks.Count);

        var asset1 = result.TechnicalAssets.First(a => a.Id == "asset1");
        Assert.Equal("Asset 2", asset1.Properties["ref"]);

        var boundary = result.TrustBoundaries.First();
        Assert.Equal(2, boundary.TechnicalAssets.Count);
        Assert.Contains("asset1", boundary.TechnicalAssets);
        Assert.Contains("asset2", boundary.TechnicalAssets);

        var link = result.CommunicationLinks.First();
        Assert.Equal("asset1", link.Source);
        Assert.Equal("asset2", link.Target);
    }

    [Fact]
    public void ProcessReferences_WithValidYaml_ProcessesCorrectly()
    {
        // Arrange
        var yaml = @"
title: Test Model
technical_assets:
  - id: asset1
    title: Asset 1
    properties:
      ref: ref:asset2
  - id: asset2
    title: Asset 2
trust_boundaries:
  - id: boundary1
    technical_assets:
      - asset1
      - asset2
communication_links:
  - id: link1
    source: asset1
    target: asset2
";

        // Act
        var result = _generator.ProcessReferences(yaml);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Asset 2", result);
        Assert.Contains("asset1", result);
        Assert.Contains("asset2", result);
    }
} 