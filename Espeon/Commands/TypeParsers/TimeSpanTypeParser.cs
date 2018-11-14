using Qmmands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public sealed class TimeSpanTypeParser : TypeParser<TimeSpan>
    {
        private static readonly Regex TimeSpanRegex = new Regex(@"(\d+)(w(?:eeks?)?|d(?:ays?)?|h(?:ours?)?|m(?:inutes?)?|s(?:econds?)?)", RegexOptions.Compiled);

        public override Task<TypeParserResult<TimeSpan>> ParseAsync(string value, ICommandContext context, IServiceProvider provider)
        {
            var matches = TimeSpanRegex.Matches(value);
            if (matches.Count <= 0) return Task.FromResult(new TypeParserResult<TimeSpan>("Failed to parse time span"));

            var result = new TimeSpan();
            bool weeks = false, days = false, hours = false, minutes = false, seconds = false;

            for (var m = 0; m < matches.Count; m++)
            {
                var match = matches[m];

                if (!uint.TryParse(match.Groups[1].Value, out var amount))
                    continue;

                var character = match.Groups[2].Value[0];

                switch (character)
                {
                    case 'w':
                        if (!weeks)
                        {
                            result = result.Add(TimeSpan.FromDays(amount * 7));
                            weeks = true;
                        }
                        continue;

                    case 'd':
                        if (!days)
                        {
                            result = result.Add(TimeSpan.FromDays(amount));
                            days = true;
                        }
                        continue;

                    case 'h':
                        if (!hours)
                        {
                            result = result.Add(TimeSpan.FromHours(amount));
                            hours = true;
                        }
                        continue;

                    case 'm':
                        if (!minutes)
                        {
                            result = result.Add(TimeSpan.FromMinutes(amount));
                            minutes = true;
                        }
                        continue;

                    case 's':
                        if (!seconds)
                        {
                            result = result.Add(TimeSpan.FromSeconds(amount));
                            seconds = true;
                        }
                        continue;
                }
            }

            return Task.FromResult(result > TimeSpan.FromSeconds(10)
                ? new TypeParserResult<TimeSpan>(result)
                : new TypeParserResult<TimeSpan>("Time span must be greater than 10seconds"));

        }
    }
}
