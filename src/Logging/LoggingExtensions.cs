using Serilog;
using Serilog.Configuration;

namespace Espeon {
    public static class LoggingExtensions {
        public static LoggerConfiguration WithClassName(this LoggerEnrichmentConfiguration configuration) {
            return configuration.With<ClassNameEnricher>();
        }
    }
}