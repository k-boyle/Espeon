using Qmmands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Espeon.Commands
{
    public sealed class TimeSpanTypeParser : TypeParser<TimeSpan>
    {
        private const string Regex = @"(\d+)(w(?:eeks|eek?)?|d(?:ays|ay?)?|h(?:ours|rs|r?)|m(?:inutes|ins|in?)?|s(?:econds|econd|ecs|ec?)?)";
        private static readonly Regex TimeSpanRegex = new Regex(Regex, RegexOptions.Compiled);

        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext)ctx;

            var matches = TimeSpanRegex.Matches(value);
            if (matches.Count <= 0)
            {
                var response = provider.GetService<ResponseService>();
                var user = context.Invoker;

                return new TypeParserResult<TimeSpan>(response.GetResponse(this, user.ResponsePack, 0));
            }

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

            return new TypeParserResult<TimeSpan>(result);

        }
    }
}
