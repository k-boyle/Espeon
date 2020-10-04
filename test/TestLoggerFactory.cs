using Serilog;

namespace Espeon.Test {
    public static class TestLoggerFactory {
        public static ILogger Create() {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}