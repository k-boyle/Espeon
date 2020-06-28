using Disqord.Logging;
using System;
using SerilogLogger = Serilog.ILogger;

namespace Espeon {
    public class DisqordSerilogAdapter : ILogger {
        private readonly SerilogLogger _logger;
        
        public DisqordSerilogAdapter(SerilogLogger logger) {
            this._logger = logger;
        }

        public void Log(object sender, MessageLoggedEventArgs e) {
            this._logger.ForContext("SourceContext", sender.GetType().Name)
                .Write(LoggingHelper.From(e.Severity), e.Exception, e.Message);
        }

        public event EventHandler<MessageLoggedEventArgs> MessageLogged;
    }
}