using Disqord.Logging;
using Microsoft.Extensions.Logging;
using System;
using ILogger = Disqord.Logging.ILogger;

namespace Espeon {
    public class DisqordLogger : ILogger {
        private readonly ILogger<Discord> _logger;

        public DisqordLogger(ILogger<Discord> logger) {
            this._logger = logger;
        }

        public void Dispose() {
        }

        public void Log(object sender, LogEventArgs e) {
            this._logger.Log((LogLevel) e.Severity, e.Message);
        }

        public event EventHandler<LogEventArgs> Logged;
    }
}