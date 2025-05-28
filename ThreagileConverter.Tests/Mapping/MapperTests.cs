using Microsoft.Extensions.Logging;
using Moq;
using ThreagileConverter.Core.Adapters;
using ThreagileConverter.Core.Mapping;
using ThreagileConverter.Core.Models;
using Xunit;

namespace ThreagileConverter.Tests.Mapping;

public class MapperTests
{
    private readonly Mock<ILogger<Mapper>> _loggerMock;
    private readonly Mapper _mapper;

    public MapperTests()
    {
        _loggerMock = new Mock<ILogger<Mapper>>();
        _mapper = new Mapper(_loggerMock.Object);
    }

    [Fact]
    public void MapToThreagile_WithValidDrawIOModel_ReturnsThreagileModel()
    {
        // Arrange
        var drawIOModel = new DrawIOModel
        {
            Shapes = new List<Shape>
            {
                new()
                {
                    Id = "shape1",
                    Type = "process",
                    Value = "Test Process",
                    Properties = new Dictionary<string, string>
                    {
                        { "description", "Test Description" },
                        { "usage", "business" }
                    },
                    Style = new Style
                    {
                        FillColor = "#FFFFFF",
                        StrokeColor = "#000000"
                    }
                }
            },
            Connections = new List<Connection>
            {
                new()
                {
                    Id = "conn1",
                    SourceId = "shape1",
                    TargetId = "shape2",
                    Value = "Test Connection",
                    Properties = new Dictionary<string, string>
                    {
                        { "protocol", "HTTPS" },
                        { "authentication", "Basic" }
                    }
                }
            }
        };

        // Act
        var result = _mapper.MapToThreagile(drawIOModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.TechnicalAssets);
        Assert.Single(result.CommunicationLinks);

        var asset = result.TechnicalAssets[0];
        Assert.Equal("shape1", asset.Id);
        Assert.Equal("Test Process", asset.Title);
        Assert.Equal("Test Description", asset.Description);
        Assert.Equal("Process", asset.Type);
        Assert.Equal("Business", asset.Usage);

        var link = result.CommunicationLinks[0];
        Assert.Equal("conn1", link.Id);
        Assert.Equal("Test Connection", link.Title);
        Assert.Equal("shape1", link.Source);
        Assert.Equal("shape2", link.Target);
        Assert.Equal("HTTPS", link.Protocol);
        Assert.Equal("Basic", link.Authentication);
    }

    [Fact]
    public void MapToThreagile_WithTrustBoundary_ProcessesCorrectly()
    {
        // Arrange
        var drawIOModel = new DrawIOModel
        {
            Shapes = new List<Shape>
            {
                new()
                {
                    Id = "boundary1",
                    Type = "trust-boundary",
                    Value = "Test Boundary",
                    Properties = new Dictionary<string, string>
                    {
                        { "boundaryType", "Network" }
                    }
                },
                new()
                {
                    Id = "asset1",
                    Type = "process",
                    Value = "Test Asset",
                    Properties = new Dictionary<string, string>
                    {
                        { "trustBoundary", "boundary1" }
                    }
                }
            }
        };

        // Act
        var result = _mapper.MapToThreagile(drawIOModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.TrustBoundaries);
        Assert.Single(result.TechnicalAssets);

        var boundary = result.TrustBoundaries[0];
        Assert.Equal("boundary1", boundary.Id);
        Assert.Equal("Test Boundary", boundary.Title);
        Assert.Equal("Network", boundary.Type);
        Assert.Single(boundary.TechnicalAssets);
        Assert.Equal("asset1", boundary.TechnicalAssets[0]);
    }

    [Fact]
    public void MapToThreagile_WithSharedRuntime_ProcessesCorrectly()
    {
        // Arrange
        var drawIOModel = new DrawIOModel
        {
            Shapes = new List<Shape>
            {
                new()
                {
                    Id = "runtime1",
                    Type = "shared-runtime",
                    Value = "Test Runtime"
                },
                new()
                {
                    Id = "asset1",
                    Type = "process",
                    Value = "Test Asset",
                    Properties = new Dictionary<string, string>
                    {
                        { "sharedRuntime", "runtime1" }
                    }
                }
            }
        };

        // Act
        var result = _mapper.MapToThreagile(drawIOModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.SharedRuntimes);
        Assert.Single(result.TechnicalAssets);

        var runtime = result.SharedRuntimes[0];
        Assert.Equal("runtime1", runtime.Id);
        Assert.Equal("Test Runtime", runtime.Title);
        Assert.Single(runtime.TechnicalAssets);
        Assert.Equal("asset1", runtime.TechnicalAssets[0]);
    }

    [Fact]
    public void ValidateTypes_WithInvalidType_SetsToUnknown()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "asset1",
                    Type = "invalid-type"
                }
            }
        };

        // Act
        _mapper.ValidateTypes(model);

        // Assert
        Assert.Equal("Unknown", model.TechnicalAssets[0].Type);
    }

    [Fact]
    public void ProcessRelations_UpdatesAssetProperties()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "asset1"
                }
            },
            CommunicationLinks = new List<CommunicationLink>
            {
                new()
                {
                    Id = "link1",
                    Source = "asset1",
                    Target = "asset2",
                    Protocol = "HTTPS",
                    Authentication = "Basic"
                }
            }
        };

        // Act
        _mapper.ProcessRelations(model);

        // Assert
        var asset = model.TechnicalAssets[0];
        Assert.Equal("HTTPS", asset.Properties["outgoingProtocol"]);
        Assert.Equal("Basic", asset.Properties["outgoingAuthentication"]);
    }

    [Fact]
    public void ConvertStyle_WithValidStyle_ReturnsThreagileStyle()
    {
        // Arrange
        var drawIOStyle = new Style
        {
            FillColor = "#FFFFFF",
            StrokeColor = "#000000",
            FontColor = "#333333",
            FontStyle = "bold",
            FontSize = "12",
            Properties = new Dictionary<string, string>
            {
                { "custom", "value" }
            }
        };

        // Act
        var result = _mapper.ConvertStyle(drawIOStyle);

        // Assert
        Assert.Equal("#FFFFFF", result.FillColor);
        Assert.Equal("#000000", result.StrokeColor);
        Assert.Equal("#333333", result.FontColor);
        Assert.Equal("bold", result.FontStyle);
        Assert.Equal("12", result.FontSize);
        Assert.Single(result.Properties);
        Assert.Equal("value", result.Properties["custom"]);
    }

    [Fact]
    public void ValidateConstraints_WithInvalidData_LogsWarnings()
    {
        // Arrange
        var model = new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "",
                    Title = "",
                    Type = ""
                }
            },
            TrustBoundaries = new List<TrustBoundary>
            {
                new()
                {
                    Id = "",
                    Title = "",
                    Type = ""
                }
            },
            CommunicationLinks = new List<CommunicationLink>
            {
                new()
                {
                    Id = "",
                    Source = "",
                    Target = ""
                }
            }
        };

        // Act
        _mapper.ValidateConstraints(model);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(6));
    }
} 