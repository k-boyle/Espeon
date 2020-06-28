using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SerilogLogger = Serilog.ILogger;
using DisqordLogger = Disqord.Logging.ILogger;

namespace Espeon {
    public static class LoggerFactory {
        private const string LoggingTemplate = "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level,-11}] ({SourceContext,-20}) {Message}{NewLine}{Exception}";
        
        public static SerilogLogger Create(Config config) {
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
        
        public static DisqordLogger CreateAdaptedLogger(SerilogLogger logger) {
            return new DisqordSerilogAdapter(logger);
        }
    }
}