using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using ThreagileConverter.Core.Validation;

namespace ThreagileConverter.IntegrationTests.Validation
{
    public class XmlValidationStrategyIntegrationTests
    {
        private readonly ILogger<XmlValidationStrategy> _logger;
        private readonly XmlValidationStrategy _strategy;

        public XmlValidationStrategyIntegrationTests()
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<XmlValidationStrategy>();
            _strategy = new XmlValidationStrategy(_logger);
        }

        [Fact]
        public async Task ValidateAsync_WithValidXmlFile_ReturnsSuccess()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
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
                await File.WriteAllTextAsync(tempFile, xmlContent);

                // Act
                var result = await _strategy.ValidateAsync(await File.ReadAllTextAsync(tempFile));

                // Assert
                Assert.True(result.IsValid);
                Assert.Empty(result.Errors);
                Assert.Empty(result.Warnings);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateAsync_WithInvalidXmlFile_ReturnsFailure()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
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
                await File.WriteAllTextAsync(tempFile, xmlContent);

                // Act
                var result = await _strategy.ValidateAsync(await File.ReadAllTextAsync(tempFile));

                // Assert
                Assert.False(result.IsValid);
                Assert.NotEmpty(result.Errors);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateAsync_WithLargeXmlFile_HandlesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2024-01-01T00:00:00.000Z"" agent=""Mozilla/5.0"" version=""21.0.0"">
    <diagram id=""test"" name=""Test Diagram"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>";
                
                // Ajouter beaucoup d'éléments pour créer un grand fichier
                for (int i = 2; i < 1000; i++)
                {
                    xmlContent += $@"
                <mxCell id=""{i}"" parent=""1"" value=""Test {i}""/>";
                }

                xmlContent += @"
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";

                await File.WriteAllTextAsync(tempFile, xmlContent);

                // Act
                var result = await _strategy.ValidateAsync(await File.ReadAllTextAsync(tempFile));

                // Assert
                Assert.True(result.IsValid);
                Assert.Empty(result.Errors);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateAsync_WithMissingAttributes_ReturnsWarnings()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile>
    <diagram id=""test"">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
                await File.WriteAllTextAsync(tempFile, xmlContent);

                // Act
                var result = await _strategy.ValidateAsync(await File.ReadAllTextAsync(tempFile));

                // Assert
                Assert.True(result.IsValid);
                Assert.Empty(result.Errors);
                Assert.NotEmpty(result.Warnings);
                Assert.Contains(result.Warnings, w => w.Code == "WARN_MISSING_HOST");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateAsync_WithEmptyAttributes_ReturnsWarnings()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified="""">
    <diagram id=""test"" name="""">
        <mxGraphModel>
            <root>
                <mxCell id=""0""/>
                <mxCell id=""1"" parent=""0""/>
            </root>
        </mxGraphModel>
    </diagram>
</mxfile>";
                await File.WriteAllTextAsync(tempFile, xmlContent);

                // Act
                var result = await _strategy.ValidateAsync(await File.ReadAllTextAsync(tempFile));

                // Assert
                Assert.True(result.IsValid);
                Assert.Empty(result.Errors);
                Assert.NotEmpty(result.Warnings);
                Assert.Contains(result.Warnings, w => w.Code == "WARN_EMPTY_ATTRIBUTE");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task ValidateAsync_WithConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
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
                await File.WriteAllTextAsync(tempFile, xmlContent);

                // Act
                var tasks = new Task<ValidationResult>[10];
                for (int i = 0; i < 10; i++)
                {
                    tasks[i] = _strategy.ValidateAsync(await File.ReadAllTextAsync(tempFile));
                }
                var results = await Task.WhenAll(tasks);

                // Assert
                foreach (var result in results)
                {
                    Assert.True(result.IsValid);
                    Assert.Empty(result.Errors);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
} 