using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Linq;

namespace Espeon {
    public class ClassNameEnricher : ILogEventEnricher {
        private const int Padding = 18;

        private readonly ConcurrentDictionary<string, LogEventProperty> _classNameCache;

        public ClassNameEnricher() {
            this._classNameCache = new ConcurrentDictionary<string, LogEventProperty>();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
            static LogEventProperty CreateClassNameProperty(string sourceContext) {
                var split = sourceContext!.Split('.');
                var sourceClass = split[^1];

                var sourceNamespace = split[..^1];
                var firstNamespace = sourceNamespace[0];

                if (firstNamespace != typeof(ClassNameEnricher).Namespace) {
                    var shortenedNamespace = string.Join('.', sourceNamespace.Select(name => name[0]));
                    sourceClass = string.Concat(shortenedNamespace, ".", sourceClass);
                }

                sourceClass = sourceClass.Length > Padding
                    ? string.Concat(sourceClass.Substring(0, Padding - 3), "...")
                    : sourceClass.PadRight(Padding, ' ');
                
                return new LogEventProperty("ClassName", new ScalarValue(sourceClass));
            }

            var sourceContext = (string) ((ScalarValue) logEvent.Properties["SourceContext"]).Value;
            var property = this._classNameCache.GetOrAdd(sourceContext, CreateClassNameProperty);

            logEvent.AddOrUpdateProperty(property);
        }
    }
}