using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Adapters;
using ThreagileConverter.Core.Mapping;
using ThreagileConverter.Core.Models;
using Xunit;

namespace ThreagileConverter.IntegrationTests.Mapping;

public class MapperIntegrationTests
{
    private readonly ILogger<Mapper> _logger;
    private readonly Mapper _mapper;

    public MapperIntegrationTests()
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<Mapper>();
        _mapper = new Mapper(_logger);
    }

    [Fact]
    public void MapToThreagile_WithComplexDrawIOModel_ProcessesCorrectly()
    {
        // Arrange
        var drawIOModel = CreateComplexDrawIOModel();

        // Act
        var result = _mapper.MapToThreagile(drawIOModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TechnicalAssets.Count);
        Assert.Equal(2, result.TrustBoundaries.Count);
        Assert.Single(result.SharedRuntimes);
        Assert.Equal(4, result.CommunicationLinks.Count);

        // Verify technical assets
        var webApp = result.TechnicalAssets.First(a => a.Type == "WebApplication");
        Assert.Equal("web-app", webApp.Id);
        Assert.Equal("Web Application", webApp.Title);
        Assert.True(webApp.UsedAsServer);

        var database = result.TechnicalAssets.First(a => a.Type == "Database");
        Assert.Equal("db", database.Id);
        Assert.Equal("Database", database.Title);
        Assert.True(database.UsedAsDataStore);

        // Verify trust boundaries
        var networkBoundary = result.TrustBoundaries.First(b => b.Type == "Network");
        Assert.Equal("network-boundary", networkBoundary.Id);
        Assert.Equal(2, networkBoundary.TechnicalAssets.Count);

        var executionBoundary = result.TrustBoundaries.First(b => b.Type == "Execution");
        Assert.Equal("execution-boundary", executionBoundary.Id);
        Assert.Single(executionBoundary.TechnicalAssets);

        // Verify shared runtime
        var runtime = result.SharedRuntimes.First();
        Assert.Equal("shared-runtime", runtime.Id);
        Assert.Equal(2, runtime.TechnicalAssets.Count);

        // Verify communication links
        var webToDbLink = result.CommunicationLinks.First(l => l.Source == "web-app" && l.Target == "db");
        Assert.Equal("web-to-db", webToDbLink.Id);
        Assert.Equal("HTTPS", webToDbLink.Protocol);
        Assert.Equal("Basic", webToDbLink.Authentication);
    }

    [Fact]
    public void MapToThreagile_WithInvalidData_HandlesGracefully()
    {
        // Arrange
        var drawIOModel = new DrawIOModel
        {
            Shapes = new List<Shape>
            {
                new()
                {
                    Id = "invalid-shape",
                    Type = "invalid-type",
                    Properties = new Dictionary<string, string>
                    {
                        { "invalid-property", "invalid-value" }
                    }
                }
            },
            Connections = new List<Connection>
            {
                new()
                {
                    Id = "invalid-conn",
                    SourceId = "non-existent-source",
                    TargetId = "non-existent-target"
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
        Assert.Equal("invalid-shape", asset.Id);
        Assert.Equal("Unknown", asset.Type);

        var link = result.CommunicationLinks[0];
        Assert.Equal("invalid-conn", link.Id);
        Assert.Equal("non-existent-source", link.Source);
        Assert.Equal("non-existent-target", link.Target);
    }

    private DrawIOModel CreateComplexDrawIOModel()
    {
        return new DrawIOModel
        {
            Shapes = new List<Shape>
            {
                new()
                {
                    Id = "web-app",
                    Type = "web-application",
                    Value = "Web Application",
                    Properties = new Dictionary<string, string>
                    {
                        { "description", "Main web application" },
                        { "usage", "business" },
                        { "usedAsServer", "true" },
                        { "trustBoundary", "network-boundary" },
                        { "sharedRuntime", "shared-runtime" }
                    }
                },
                new()
                {
                    Id = "db",
                    Type = "database",
                    Value = "Database",
                    Properties = new Dictionary<string, string>
                    {
                        { "description", "Main database" },
                        { "usage", "business" },
                        { "usedAsDataStore", "true" },
                        { "trustBoundary", "execution-boundary" },
                        { "sharedRuntime", "shared-runtime" }
                    }
                },
                new()
                {
                    Id = "cache",
                    Type = "cache",
                    Value = "Cache",
                    Properties = new Dictionary<string, string>
                    {
                        { "description", "Application cache" },
                        { "usage", "business" },
                        { "trustBoundary", "network-boundary" }
                    }
                },
                new()
                {
                    Id = "network-boundary",
                    Type = "trust-boundary",
                    Value = "Network Boundary",
                    Properties = new Dictionary<string, string>
                    {
                        { "boundaryType", "Network" }
                    }
                },
                new()
                {
                    Id = "execution-boundary",
                    Type = "trust-boundary",
                    Value = "Execution Boundary",
                    Properties = new Dictionary<string, string>
                    {
                        { "boundaryType", "Execution" }
                    }
                },
                new()
                {
                    Id = "shared-runtime",
                    Type = "shared-runtime",
                    Value = "Shared Runtime"
                }
            },
            Connections = new List<Connection>
            {
                new()
                {
                    Id = "web-to-db",
                    SourceId = "web-app",
                    TargetId = "db",
                    Value = "Web to DB",
                    Properties = new Dictionary<string, string>
                    {
                        { "protocol", "HTTPS" },
                        { "authentication", "Basic" },
                        { "authorization", "Role-based" },
                        { "encryption", "TLS" }
                    }
                },
                new()
                {
                    Id = "web-to-cache",
                    SourceId = "web-app",
                    TargetId = "cache",
                    Value = "Web to Cache",
                    Properties = new Dictionary<string, string>
                    {
                        { "protocol", "HTTP" }
                    }
                },
                new()
                {
                    Id = "cache-to-db",
                    SourceId = "cache",
                    TargetId = "db",
                    Value = "Cache to DB",
                    Properties = new Dictionary<string, string>
                    {
                        { "protocol", "TCP" }
                    }
                },
                new()
                {
                    Id = "db-to-cache",
                    SourceId = "db",
                    TargetId = "cache",
                    Value = "DB to Cache",
                    Properties = new Dictionary<string, string>
                    {
                        { "protocol", "TCP" }
                    }
                }
            }
        };
    }
} 