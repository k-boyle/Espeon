using Qmmands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Espeon.LocalisationStringKey;

namespace Espeon {
    //TODO natural language parsing
    public class UserReminderTypeParser : EspeonTypeParser<UserReminder> {
        private const string Regex = @"(\d+)\s?(w(?:eeks|eek?)?|d(?:ays|ay?)?|h(?:ours|rs|r?)|m(?:inutes|ins|in?)?|s(?:econds|econd|ecs|ec?)?)";

        private static readonly Regex TimeSpanRegex = new Regex(Regex, RegexOptions.Compiled);

        public override ValueTask<TypeParserResult<UserReminder>> ParseAsync(
                Parameter parameter,
                string value,
                EspeonCommandContext context) {
            var matches = TimeSpanRegex.Matches(value);
            if (matches.Count == 0) {
                return new EspeonTypeParserFailedResult<UserReminder>(REMINDER_PARSER_NO_TIMESPAN);
            }
            
            var timeSpan = new TimeSpan();
            bool weeks = false, days = false, hours = false, minutes = false, seconds = false;

            for (var m = 0; m < matches.Count; m++) {
                var match = matches[m];

                if (!uint.TryParse(match.Groups[1].Value, out var amount)) {
                    continue;
                }

                switch (match.Groups[2].Value[0]) {
                    case 'w':
                        if (!weeks) {
                            timeSpan = timeSpan.Add(TimeSpan.FromDays(amount * 7));
                            weeks = true;
                        }

                        continue;

                    case 'd':
                        if (!days) {
                            timeSpan = timeSpan.Add(TimeSpan.FromDays(amount));
                            days = true;
                        }

                        continue;

                    case 'h':
                        if (!hours) {
                            timeSpan = timeSpan.Add(TimeSpan.FromHours(amount));
                            hours = true;
                        }

                        continue;

                    case 'm':
                        if (!minutes) {
                            timeSpan = timeSpan.Add(TimeSpan.FromMinutes(amount));
                            minutes = true;
                        }

                        continue;

                    case 's':
                        if (!seconds) {
                            timeSpan = timeSpan.Add(TimeSpan.FromSeconds(amount));
                            seconds = true;
                        }

                        continue;
                }
            }
            
            if (timeSpan <= TimeSpan.Zero) {
                return new EspeonTypeParserFailedResult<UserReminder>(REMINDER_PARSER_IN_THE_PAST);
            }

            var reminder = new UserReminder(
                context.Channel.Id,
                context.Member.Id,
                context.Message.Id,
                DateTimeOffset.Now + timeSpan,
                TimeSpanRegex.Replace(value, ""));
            return TypeParserResult<UserReminder>.Successful(reminder);
        }
    }
}