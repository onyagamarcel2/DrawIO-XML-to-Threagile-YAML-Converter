using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ThreagileConverter.Core.Events
{
    public class ConversionObserver : IConversionObserver
    {
        private readonly ILogger<ConversionObserver> _logger;

        public ConversionObserver(ILogger<ConversionObserver> logger)
        {
            _logger = logger;
        }

        public Task OnConversionStartedAsync(string sourcePath, string targetPath)
        {
            _logger.LogInformation("Conversion started from {SourcePath} to {TargetPath}", sourcePath, targetPath);
            return Task.CompletedTask;
        }

        public Task OnConversionProgressAsync(int progress, string message)
        {
            _logger.LogInformation("Progress {Progress}%: {Message}", progress, message);
            return Task.CompletedTask;
        }

        public Task OnConversionCompletedAsync(ConversionResult result)
        {
            if (result.Success)
            {
                _logger.LogInformation("Conversion completed successfully in {Duration}ms", result.Duration.TotalMilliseconds);
                if (result.Warnings.Count > 0)
                {
                    foreach (var warning in result.Warnings)
                    {
                        _logger.LogWarning(warning);
                    }
                }
            }
            else
            {
                _logger.LogError("Conversion failed after {Duration}ms", result.Duration.TotalMilliseconds);
            }
            return Task.CompletedTask;
        }

        public Task OnConversionErrorAsync(Exception error)
        {
            _logger.LogError(error, "Conversion error occurred");
            return Task.CompletedTask;
        }
    }
} 