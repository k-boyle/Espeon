using System.Collections.Concurrent;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Espeon.Logging
{
    public class ClassNameEnricher : ILogEventEnricher
    {
        private const int Padding = 18;

        private readonly ConcurrentDictionary<string, LogEventProperty> _properyByClassName;

        public ClassNameEnricher()
        {
            this._properyByClassName = new ConcurrentDictionary<string, LogEventProperty>();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {

            var sourceContext = (string) ((ScalarValue) logEvent.Properties["SourceContext"]).Value;
            var property = this._properyByClassName.GetOrAdd(
                sourceContext,
                static source =>
                {
                    var split = source.Split('.');
                    var sourceClass = split[^1];

                    var sourceNamespace = split[..^1];
                    var firstNamespace = sourceNamespace[0];

                    if (firstNamespace != typeof(ClassNameEnricher).Namespace)
                    {
                        var shortenedNamespace = string.Join('.', sourceNamespace.Select(name => name[0]));
                        sourceClass = $"{shortenedNamespace}.{sourceClass}";
                    }

                    sourceClass = sourceClass.Length > Padding
                        ? $"{sourceClass.Substring(0, Padding - 3)}..."
                        : sourceClass.PadRight(Padding, ' ');

                    return new LogEventProperty("ClassName", new ScalarValue(sourceClass));
                }
            );

            logEvent.AddOrUpdateProperty(property);
        }
    }
}