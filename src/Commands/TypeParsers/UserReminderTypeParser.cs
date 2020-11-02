using Qmmands;
using System;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon
{
    public class UserReminderTypeParser : EspeonTypeParser<UserReminder> {
        private static readonly TimeSpanParser TimeSpanParser = new TimeSpanParser();

        public override ValueTask<TypeParserResult<UserReminder>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            if (TimeSpanParser.TryParseIn(value, out var timeSpan)) {
                var reminder = new UserReminder(
                    context.Channel.Id,
                    context.Member.Id,
                    context.Message.Id,
                    DateTimeOffset.Now + timeSpan,
                    value);
                return TypeParserResult<UserReminder>.Successful(reminder);
            }

            return new EspeonTypeParserFailedResult<UserReminder>(REMINDER_PARSER_NO_TIMESPAN);
        }
    }
}