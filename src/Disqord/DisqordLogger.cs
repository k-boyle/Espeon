using Disqord.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using IDisqordLogger = Disqord.Logging.ILogger;
using IMSLogger = Microsoft.Extensions.Logging.ILogger;

namespace Espeon {
    public class DisqordLogger : IDisqordLogger {
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<Type, IMSLogger> _loggers;

        public DisqordLogger(IServiceProvider services) {
            this._services = services;
            this._loggers = new ConcurrentDictionary<Type, IMSLogger>();
        }

        public void Log(object sender, LogEventArgs args) {
            var logger = this._loggers.GetOrAdd(
                sender.GetType(),
                (senderType, services) => {
                    var loggerType = typeof(ILogger<>).MakeGenericType(senderType);
                    return (IMSLogger) services.GetService(loggerType);
                },
                this._services);
            logger.Log((LogLevel) args.Severity, args.Message);
        }
        
        public void Dispose() {
        }

        public event EventHandler<LogEventArgs> Logged;
    }
}