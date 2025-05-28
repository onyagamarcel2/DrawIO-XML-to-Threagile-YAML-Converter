using Microsoft.Extensions.Logging;

namespace ThreagileConverter.Core.Logging;

public static class LoggingConfiguration
{
    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder builder)
    {
        builder
            .SetMinimumLevel(LogLevel.Information)
            .AddConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                options.Format = Microsoft.Extensions.Logging.Console.ConsoleLoggerFormat.Default;
            });

        return builder;
    }
} 