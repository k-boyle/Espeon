using JetBrains.Annotations;
using Serilog;

namespace Espeon {
    public static class RiderExtensions {
        [SourceTemplate]
        public static void slog(this ILogger logger) {
            //$ this._logger = logger.ForContext("SourceContext", GetType().Name);
        }
    }
}