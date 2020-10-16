using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SerilogLogger = Serilog.ILogger;
using DisqordLogger = Disqord.Logging.ILogger;

namespace Espeon {
    public static class LoggerFactory {
        private const string LoggingTemplate = "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level,-11}] ({SourceContext,-20}) {Message}{NewLine}{Exception}";
        
        public static SerilogLogger Create(IOptions<Logging> loggingOptions) {
            var logging = loggingOptions.Value;
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(logging.Level);
            if (logging.WriteToConsole) {
                loggerConfiguration.WriteTo.Console(outputTemplate: LoggingTemplate, theme: SystemConsoleTheme.Colored);
            }

            if (logging.WriteToFile) {
                loggerConfiguration.WriteTo.File(
                    logging.Path,
                    rollingInterval: logging.RollingInterval,
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