using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Espeon {
    public static class LoggerFactory {
        private const string LoggingTemplate = "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level,-11}] ({SourceContext,-19}) {Message}{NewLine}{Exception}";
        
        public static ILogger Create(Config config) {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(config.Logging.Level);
            if (config.Logging.WriteToConsole) {
                loggerConfiguration.WriteTo.Console(outputTemplate: LoggingTemplate, theme: SystemConsoleTheme.Colored);
            }

            if (config.Logging.WriteToFile) {
                loggerConfiguration.WriteTo.File(
                    config.Logging.Path,
                    rollingInterval: config.Logging.RollingInterval,
                    rollOnFileSizeLimit: true,
                    outputTemplate: LoggingTemplate);
            }

            return loggerConfiguration.CreateLogger();
        }
    }
}