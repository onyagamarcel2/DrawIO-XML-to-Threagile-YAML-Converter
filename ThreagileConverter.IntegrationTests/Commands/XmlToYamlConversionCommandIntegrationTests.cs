using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using ThreagileConverter.Core.Commands;
using ThreagileConverter.Core.Events;
using ThreagileConverter.Core.Factories;
using ThreagileConverter.Core.Generation;
using ThreagileConverter.Core.Parsing;
using ThreagileConverter.Core.Validation;

namespace ThreagileConverter.IntegrationTests.Commands
{
    public class XmlToYamlConversionCommandIntegrationTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _sourcePath;
        private readonly string _targetPath;
        private readonly ILogger<XmlToYamlConversionCommand> _logger;
        private readonly IParserFactory _parserFactory;
        private readonly IConversionObserver _observer;
        private readonly XmlToYamlConversionCommand _command;

        public XmlToYamlConversionCommandIntegrationTests()
        {
            _sourcePath = Path.Combine(Path.GetTempPath(), $"source_{Guid.NewGuid()}.xml");
            _targetPath = Path.Combine(Path.GetTempPath(), $"target_{Guid.NewGuid()}.yaml");

            var services = new ServiceCollection();
            
            // Enregistrer les services
            services.AddSingleton<ILogger<XmlToYamlConversionCommand>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<XmlToYamlConversionCommand>());
            services.AddSingleton<ILogger<XmlParser>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<XmlParser>());
            services.AddSingleton<ILogger<XmlValidator>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<XmlValidator>());
            services.AddSingleton<ILogger<YamlGenerator>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<YamlGenerator>());
            services.AddSingleton<ILogger<ConversionObserver>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<ConversionObserver>());

            // Enregistrer les implémentations
            services.AddSingleton<XmlParser>();
            services.AddSingleton<XmlValidator>();
            services.AddSingleton<YamlGenerator>();
            services.AddSingleton<ParserFactory>();
            services.AddSingleton<ConversionObserver>();

            _serviceProvider = services.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<XmlToYamlConversionCommand>>();
            _parserFactory = _serviceProvider.GetRequiredService<ParserFactory>();
            _observer = _serviceProvider.GetRequiredService<ConversionObserver>();

            _command = new XmlToYamlConversionCommand(
                _sourcePath,
                _targetPath,
                _parserFactory,
                _logger);
        }

        public void Dispose()
        {
            if (File.Exists(_sourcePath))
                File.Delete(_sourcePath);
            if (File.Exists(_targetPath))
                File.Delete(_targetPath);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidXml_CompletesSuccessfully()
        {
            // Arrange
            var xmlContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root></root>";
            var yamlContent = "root: {}";
            File.WriteAllText(_sourcePath, xmlContent);

            // Act
            var result = await _command.ExecuteAsync(_observer);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(_sourcePath, result.SourcePath);
            Assert.Equal(_targetPath, result.TargetPath);
            Assert.Equal(yamlContent, await File.ReadAllTextAsync(_targetPath));
        }

        [Fact]
        public async Task ExecuteAsync_WithInvalidXml_ReturnsFailure()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <diagram>
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            await File.WriteAllTextAsync(_sourcePath, xmlContent);

            // Act
            var result = await _command.ExecuteAsync(_observer);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Warnings);
        }

        [Fact]
        public async Task ExecuteAsync_WithLargeXml_HandlesCorrectly()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <diagram>
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
                <mxCell id=""2"" value=""Web App"" style=""rounded=1;whiteSpace=wrap;html=1;"" vertex=""1"" parent=""1"">
                    <mxGeometry x=""120"" y=""120"" width=""120"" height=""60"" as=""geometry""/>
                </mxCell>
                <mxCell id=""3"" value=""Database"" style=""shape=cylinder3;whiteSpace=wrap;html=1;boundedLbl=1;backgroundOutline=1;size=15;"" vertex=""1"" parent=""1"">
                    <mxGeometry x=""400"" y=""120"" width=""60"" height=""80"" as=""geometry""/>
                </mxCell>
                <mxCell id=""4"" value="""" style=""endArrow=classic;html=1;exitX=1;exitY=0.5;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;"" edge=""1"" parent=""1"" source=""2"" target=""3"">
                    <mxGeometry width=""50"" height=""50"" relative=""1"" as=""geometry"">
                        <mxPoint x=""390"" y=""420"" as=""sourcePoint""/>
                        <mxPoint x=""440"" y=""370"" as=""targetPoint""/>
                    </mxGeometry>
                </mxCell>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            await File.WriteAllTextAsync(_sourcePath, xmlContent);

            // Act
            var result = await _command.ExecuteAsync(_observer);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(_sourcePath, result.SourcePath);
            Assert.Equal(_targetPath, result.TargetPath);
            var yamlContent = await File.ReadAllTextAsync(_targetPath);
            Assert.Contains("Web App", yamlContent);
            Assert.Contains("Database", yamlContent);
        }

        [Fact]
        public async Task CancelAsync_CancelsOperation()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"" name=""Test Diagram"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            await File.WriteAllTextAsync(_sourcePath, xmlContent);

            // Act
            await _command.CancelAsync();
            var result = await _command.ExecuteAsync(_observer);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ExecuteAsync_WithConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"" name=""Test Diagram"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
            await File.WriteAllTextAsync(_sourcePath, xmlContent);

            // Act
            var tasks = new Task<ConversionResult>[2];
            tasks[0] = _command.ExecuteAsync(_observer);
            tasks[1] = Task.Run(async () => {
                await _command.CancelAsync();
                return new ConversionResult(
                    false, // success
                    _sourcePath,
                    _targetPath,
                    TimeSpan.Zero,
                    0,
                    new List<string>()
                );
            });
            await Task.WhenAll(tasks);

            // Assert
            var result = await tasks[0];
            Assert.False(result.Success);
        }
    }
} 