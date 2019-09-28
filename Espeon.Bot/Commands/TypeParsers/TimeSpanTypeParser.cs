using Espeon.Commands;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
{
    public class TimeSpanTypeParser : EspeonTypeParser<TimeSpan>
    {
        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string value,
            EspeonContext context, IServiceProvider provider)
        {
            var matches = Utilities.TimeSpanRegex.Matches(value);

            if (matches.Count == 0)
            {
                var response = provider.GetService<IResponseService>();
                var user = context.Invoker;

                return TypeParserResult<TimeSpan>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
            }

            var result = new TimeSpan();
            bool weeks = false, days = false, hours = false, minutes = false, seconds = false;

            for (var m = 0; m < matches.Count; m++)
            {
                var match = matches[m];

                if (!uint.TryParse(match.Groups[1].Value, out var amount))
                    continue;

                switch (match.Groups[2].Value[0])
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

            return TypeParserResult<TimeSpan>.Successful(result);
        }

    }
}
