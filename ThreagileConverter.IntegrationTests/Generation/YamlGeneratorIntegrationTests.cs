using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Generation;
using ThreagileConverter.Core.Models;
using Xunit;

namespace ThreagileConverter.IntegrationTests.Generation;

public class YamlGeneratorIntegrationTests
{
    private readonly ILogger<YamlGenerator> _logger;
    private readonly YamlGenerator _generator;

    public YamlGeneratorIntegrationTests()
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<YamlGenerator>();
        _generator = new YamlGenerator(_logger);
    }

    [Fact]
    public void GenerateYaml_WithComplexModel_GeneratesValidYaml()
    {
        // Arrange
        var model = CreateComplexModel();

        // Act
        var yaml = _generator.GenerateYaml(model);

        // Assert
        Assert.NotNull(yaml);
        Assert.True(_generator.ValidateSchema(yaml));

        // Verify technical assets
        Assert.Contains("web-app", yaml);
        Assert.Contains("Web Application", yaml);
        Assert.Contains("Database", yaml);
        Assert.Contains("Cache", yaml);

        // Verify trust boundaries
        Assert.Contains("network-boundary", yaml);
        Assert.Contains("Network Boundary", yaml);
        Assert.Contains("execution-boundary", yaml);
        Assert.Contains("Execution Boundary", yaml);

        // Verify shared runtime
        Assert.Contains("shared-runtime", yaml);
        Assert.Contains("Shared Runtime", yaml);

        // Verify communication links
        Assert.Contains("web-to-db", yaml);
        Assert.Contains("HTTPS", yaml);
        Assert.Contains("Basic", yaml);
        Assert.Contains("Role-based", yaml);
        Assert.Contains("TLS", yaml);
    }

    [Fact]
    public void GenerateYamlToFile_WithComplexModel_CreatesValidFile()
    {
        // Arrange
        var model = CreateComplexModel();
        var filePath = Path.GetTempFileName();

        try
        {
            // Act
            _generator.GenerateYamlToFile(model, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.True(_generator.ValidateSchema(content));

            // Verify file content
            Assert.Contains("web-app", content);
            Assert.Contains("Database", content);
            Assert.Contains("Network Boundary", content);
            Assert.Contains("Shared Runtime", content);
            Assert.Contains("HTTPS", content);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void OptimizeOutput_WithComplexYaml_OptimizesCorrectly()
    {
        // Arrange
        var model = CreateComplexModel();
        var yaml = _generator.GenerateYaml(model);

        // Act
        var optimized = _generator.OptimizeOutput(yaml);

        // Assert
        Assert.NotNull(optimized);
        Assert.True(_generator.ValidateSchema(optimized));

        // Verify optimization
        var originalLines = yaml.Split('\n').Length;
        var optimizedLines = optimized.Split('\n').Length;
        Assert.True(optimizedLines <= originalLines);

        // Verify content preservation
        Assert.Contains("web-app", optimized);
        Assert.Contains("Database", optimized);
        Assert.Contains("Network Boundary", optimized);
        Assert.Contains("Shared Runtime", optimized);
        Assert.Contains("HTTPS", optimized);
    }

    private ThreagileModel CreateComplexModel()
    {
        return new ThreagileModel
        {
            TechnicalAssets = new List<TechnicalAsset>
            {
                new()
                {
                    Id = "web-app",
                    Title = "Web Application",
                    Type = "Process",
                    UsedAsServer = true,
                    Style = new ThreagileStyle
                    {
                        FillColor = "#FFFFFF",
                        StrokeColor = "#000000"
                    }
                },
                new()
                {
                    Id = "db",
                    Title = "Database",
                    Type = "DataStore",
                    UsedAsDataStore = true,
                    Style = new ThreagileStyle
                    {
                        FillColor = "#F0F0F0",
                        StrokeColor = "#000000"
                    }
                },
                new()
                {
                    Id = "cache",
                    Title = "Cache",
                    Type = "DataStore",
                    UsedAsDataStore = true,
                    Style = new ThreagileStyle
                    {
                        FillColor = "#E0E0E0",
                        StrokeColor = "#000000"
                    }
                }
            },
            TrustBoundaries = new List<TrustBoundary>
            {
                new()
                {
                    Id = "network-boundary",
                    Title = "Network Boundary",
                    Type = "Network",
                    TechnicalAssets = new List<string> { "web-app" }
                },
                new()
                {
                    Id = "execution-boundary",
                    Title = "Execution Boundary",
                    Type = "Execution",
                    TechnicalAssets = new List<string> { "db", "cache" }
                }
            },
            SharedRuntimes = new List<SharedRuntime>
            {
                new()
                {
                    Id = "shared-runtime",
                    Title = "Shared Runtime",
                    TechnicalAssets = new List<string> { "web-app" }
                }
            },
            CommunicationLinks = new List<CommunicationLink>
            {
                new()
                {
                    Id = "web-to-db",
                    Title = "Web to DB",
                    Source = "web-app",
                    Target = "db",
                    Protocol = "HTTPS",
                    Authentication = "Basic",
                    Authorization = "Role-based",
                    Encryption = "TLS"
                }
            }
        };
    }
} 