using Espeon.Commands;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class ReminderTypeParser : EspeonTypeParser<(string, TimeSpan)>
    {
        private readonly static RequireSpecificLengthAttribute _specificLengthAttribute;
        private readonly static TimeSpanTypeParser _timeSpanTypeParser;

        static ReminderTypeParser()
        {
            _specificLengthAttribute = new RequireSpecificLengthAttribute(0, 200);
            _timeSpanTypeParser = new TimeSpanTypeParser();
        }

        public override async ValueTask<TypeParserResult<(string, TimeSpan)>> ParseAsync(Parameter parameter, string value,
            EspeonContext context, IServiceProvider provider)
        {
            var lengthResult = await _specificLengthAttribute.CheckAsync(value, context, provider);

            if (!lengthResult.IsSuccessful)
                return TypeParserResult<(string, TimeSpan)>.Unsuccessful(lengthResult.Reason);

            var timeParserResult = await _timeSpanTypeParser.ParseAsync(parameter, value, context, provider);

            if (!timeParserResult.IsSuccessful)
                return TypeParserResult<(string, TimeSpan)>.Unsuccessful(timeParserResult.Reason);

            return TypeParserResult<(string, TimeSpan)>.Successful((Utilities.TimeSpanRegex.Replace(value, "").Trim(),
                timeParserResult.Value));
        }
    }
}
