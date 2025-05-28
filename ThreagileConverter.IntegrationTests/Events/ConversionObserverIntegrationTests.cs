using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using ThreagileConverter.Core.Events;
using Serilog;

namespace ThreagileConverter.IntegrationTests.Events
{
    public class ConversionObserverIntegrationTests
    {
        private readonly ILogger<ConversionObserver> _logger;
        private readonly ConversionObserver _observer;
        private readonly string _logFilePath;

        public ConversionObserverIntegrationTests()
        {
            _logFilePath = Path.Combine(Path.GetTempPath(), $"conversion_log_{Guid.NewGuid()}.log");
            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddConsole()
                .AddSerilog(new LoggerConfiguration()
                    .WriteTo.File(_logFilePath)
                    .CreateLogger()));
            _logger = loggerFactory.CreateLogger<ConversionObserver>();
            _observer = new ConversionObserver(_logger);
        }

        public void Dispose()
        {
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        [Fact]
        public async Task FullConversionCycle_LogsAllEvents()
        {
            // Arrange
            var sourcePath = "test.xml";
            var targetPath = "test.yaml";
            var result = ConversionResult.CreateSuccess(sourcePath, targetPath, TimeSpan.FromSeconds(1), 10);

            // Act
            await _observer.OnConversionStartedAsync(sourcePath, targetPath);
            await _observer.OnConversionProgressAsync(25, "Parsing XML");
            await _observer.OnConversionProgressAsync(50, "Converting to YAML");
            await _observer.OnConversionProgressAsync(75, "Validating output");
            await _observer.OnConversionCompletedAsync(result);

            // Assert
            var logContent = await File.ReadAllTextAsync(_logFilePath);
            Assert.Contains("Conversion started", logContent);
            Assert.Contains("Parsing XML", logContent);
            Assert.Contains("Converting to YAML", logContent);
            Assert.Contains("Validating output", logContent);
            Assert.Contains("Conversion completed", logContent);
        }

        [Fact]
        public async Task ErrorHandling_LogsError()
        {
            // Arrange
            var sourcePath = "test.xml";
            var targetPath = "test.yaml";
            var error = new Exception("Test error");

            // Act
            await _observer.OnConversionStartedAsync(sourcePath, targetPath);
            await _observer.OnConversionErrorAsync(error);

            // Assert
            var logContent = await File.ReadAllTextAsync(_logFilePath);
            Assert.Contains("Conversion started", logContent);
            Assert.Contains("Test error", logContent);
        }

        [Fact]
        public async Task ProgressUpdates_LogsAllProgress()
        {
            // Arrange
            var sourcePath = "test.xml";
            var targetPath = "test.yaml";

            // Act
            await _observer.OnConversionStartedAsync(sourcePath, targetPath);
            for (int i = 0; i <= 100; i += 10)
            {
                await _observer.OnConversionProgressAsync(i, $"Progress {i}%");
            }

            // Assert
            var logContent = await File.ReadAllTextAsync(_logFilePath);
            Assert.Contains("Conversion started", logContent);
            for (int i = 0; i <= 100; i += 10)
            {
                Assert.Contains($"Progress {i}%", logContent);
            }
        }

        [Fact]
        public async Task ConcurrentEvents_HandlesCorrectly()
        {
            // Arrange
            var sourcePath = "test.xml";
            var targetPath = "test.yaml";
            var tasks = new Task[10];

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = _observer.OnConversionProgressAsync(i * 10, $"Progress {i * 10}%");
            }
            await Task.WhenAll(tasks);

            // Assert
            var logContent = await File.ReadAllTextAsync(_logFilePath);
            for (int i = 0; i < 10; i++)
            {
                Assert.Contains($"Progress {i * 10}%", logContent);
            }
        }

        [Fact]
        public async Task CompletionWithWarnings_LogsWarnings()
        {
            // Arrange
            var sourcePath = "test.xml";
            var targetPath = "test.yaml";
            var warnings = new[] { "Warning 1", "Warning 2" };
            var result = ConversionResult.CreateSuccess(sourcePath, targetPath, TimeSpan.FromSeconds(1), 10, warnings);

            // Act
            await _observer.OnConversionStartedAsync(sourcePath, targetPath);
            await _observer.OnConversionCompletedAsync(result);

            // Assert
            var logContent = await File.ReadAllTextAsync(_logFilePath);
            Assert.Contains("Conversion started", logContent);
            Assert.Contains("Conversion completed", logContent);
            Assert.Contains("Warning 1", logContent);
            Assert.Contains("Warning 2", logContent);
        }

        [Fact]
        public async Task Failure_LogsFailure()
        {
            // Arrange
            var sourcePath = "test.xml";
            var targetPath = "test.yaml";
            var result = ConversionResult.CreateFailure(sourcePath, targetPath, TimeSpan.FromSeconds(1));

            // Act
            await _observer.OnConversionStartedAsync(sourcePath, targetPath);
            await _observer.OnConversionCompletedAsync(result);

            // Assert
            var logContent = await File.ReadAllTextAsync(_logFilePath);
            Assert.Contains("Conversion started", logContent);
            Assert.Contains("Conversion failed", logContent);
        }
    }
} 