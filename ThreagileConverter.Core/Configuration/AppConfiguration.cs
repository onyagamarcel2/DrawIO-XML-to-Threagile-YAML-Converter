using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Logging;

namespace ThreagileConverter.Core.Configuration;

public class AppConfiguration
{
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public bool ValidateInput { get; set; } = true;
    public bool ValidateOutput { get; set; } = true;
    public bool ExtractMetadata { get; set; } = true;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    public static AppConfiguration FromConfiguration(IConfiguration configuration)
    {
        return new AppConfiguration
        {
            InputPath = configuration["InputPath"] ?? string.Empty,
            OutputPath = configuration["OutputPath"] ?? string.Empty,
            ValidateInput = configuration.GetValue<bool>("ValidateInput", true),
            ValidateOutput = configuration.GetValue<bool>("ValidateOutput", true),
            ExtractMetadata = configuration.GetValue<bool>("ExtractMetadata", true),
            MinimumLogLevel = configuration.GetValue<LogLevel>("MinimumLogLevel", LogLevel.Information)
        };
    }
}

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppConfiguration>(configuration);
        services.AddSingleton(configuration.Get<AppConfiguration>() ?? new AppConfiguration());
        
        return services;
    }
} 