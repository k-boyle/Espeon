using Disqord.Logging;
using Serilog.Events;

namespace Espeon.Logging {
    public static class LoggingHelper {
        public static LogEventLevel From(LogMessageSeverity severity) {
            return (LogEventLevel) severity;
        }
    }
}