using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using ThreagileConverter.Core.Generation;
using ThreagileConverter.Core.Mapping;
using ThreagileConverter.Core.Models;
using ThreagileConverter.Core.Parsers;
using Xunit;

namespace ThreagileConverter.IntegrationTests.Integration;

public class ConversionFlowTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _testFilePath;

    public ConversionFlowTests()
    {
        // Setup DI
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddTransient<IDrawIOParser, DrawIOParser>();
        services.AddTransient<IMapper, Mapper>();
        services.AddTransient<IYamlGenerator, YamlGenerator>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Create test file
        _testFilePath = Path.GetTempFileName();
        File.WriteAllText(_testFilePath, "<diagram><shape id=\"1\" value=\"Test\"></shape></diagram>");
    }

    [Fact]
    public async Task ConversionFlow_SimpleXml_GeneratesYaml()
    {
        // Arrange
        var parser = _serviceProvider.GetRequiredService<IDrawIOParser>();
        var mapper = _serviceProvider.GetRequiredService<IMapper>();
        var generator = _serviceProvider.GetRequiredService<IYamlGenerator>();
        
        var outputPath = Path.GetTempFileName();
        
        try
        {
            // Act
            var xml = await File.ReadAllTextAsync(_testFilePath);
            var drawioModel = await parser.ParseAsync(xml);
            var threagileModel = await mapper.MapToThreagileAsync(drawioModel);
            generator.GenerateYamlToFile(threagileModel, outputPath);
            
            // Assert
            Assert.True(File.Exists(outputPath));
            var yaml = await File.ReadAllTextAsync(outputPath);
            Assert.NotEmpty(yaml);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
            
        _serviceProvider.Dispose();
    }
} 