using Disqord.Logging;
using Serilog.Events;
using System;
using SerilogLogger = Serilog.ILogger;

namespace Espeon {
    public class DisqordSerilogAdapter : ILogger {
        private readonly SerilogLogger _logger;
        
        public DisqordSerilogAdapter(SerilogLogger logger) {
            this._logger = logger;
        }

        public void Dispose() {
        }

        public void Log(object sender, LogEventArgs e) {
            this._logger.ForContext("SourceContext", sender.GetType().Name)
                .Write((LogEventLevel) e.Severity, e.Exception, e.Message);
        }

        public event EventHandler<LogEventArgs> Logged;
    }
}