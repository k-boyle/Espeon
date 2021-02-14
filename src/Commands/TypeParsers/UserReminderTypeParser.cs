using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;
using static Espeon.LocalisationStringKey;

namespace Espeon
{
    public class UserReminderTypeParser : EspeonTypeParser<UserReminder> {
        private static readonly TimeSpanParser TimeSpanParser = new(
            new Dictionary<string, TimeUnit>(StringComparer.InvariantCultureIgnoreCase) {
                ["s"] = TimeUnit.SECOND,
                ["sec"] = TimeUnit.SECOND,
                ["secs"] = TimeUnit.SECOND,
                ["second"] = TimeUnit.SECOND,
                ["seconds"] = TimeUnit.SECOND,
                
                ["m"] = TimeUnit.MINUTE,
                ["min"] = TimeUnit.MINUTE,
                ["minute"] = TimeUnit.MINUTE,
                ["minutes"] = TimeUnit.MINUTE,
                
                ["h"] = TimeUnit.HOUR,
                ["hr"] = TimeUnit.HOUR,
                ["hrs"] = TimeUnit.HOUR,
                ["hour"] = TimeUnit.HOUR,
                ["hours"] = TimeUnit.HOUR,
                
                ["d"] = TimeUnit.DAY,
                ["day"] = TimeUnit.DAY,
                ["days"] = TimeUnit.DAY,
                
                ["w"] = TimeUnit.WEEK,
                ["wk"] = TimeUnit.WEEK,
                ["wks"] = TimeUnit.WEEK,
                ["week"] = TimeUnit.WEEK,
                ["weeks"] = TimeUnit.WEEK,
                
                ["mth"] = TimeUnit.MONTH,
                ["mths"] = TimeUnit.MONTH,
                ["month"] = TimeUnit.MONTH,
                ["months"] = TimeUnit.MONTH,
                
                ["y"] = TimeUnit.YEAR,
                ["yr"] = TimeUnit.YEAR,
                ["yrs"] = TimeUnit.YEAR,
                ["year"] = TimeUnit.YEAR,
                ["years"] = TimeUnit.YEAR
            });

        public override ValueTask<TypeParserResult<UserReminder>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            if (!TimeSpanParser.TryParseIn(value, out var timeSpan)) {
                return new EspeonTypeParserFailedResult<UserReminder>(REMINDER_PARSER_NO_TIMESPAN);
            }

            var reminder = new UserReminder(
                context.Channel.Id,
                context.Member.Id,
                context.Message.Id,
                DateTimeOffset.Now + timeSpan,
                value);
            return TypeParserResult<UserReminder>.Successful(reminder);
        }
    }
}