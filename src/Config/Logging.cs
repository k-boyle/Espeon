using Serilog;
using Serilog.Events;

namespace Espeon {
    public class Logging {
        public bool WriteToFile { get; set; }
        public bool WriteToConsole { get; set; }
        public string Path { get; set; }
        public LogEventLevel Level { get; set; }
        public RollingInterval RollingInterval { get; set; }
    }
}