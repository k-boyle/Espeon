using Discord;
using Espeon.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class UserTupleParser : TypeParser<(IGuildUser, User)>
    {
        public override async Task<TypeParserResult<(IGuildUser, User)>> ParseAsync(string value, ICommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            var commands = provider.GetService<CommandService>();

            var userParser = commands.GetSpecificTypeParser<IGuildUser, IGuildUserTypeParser>();

            var result = await userParser.ParseAsync(value, context, provider);

            if(result.IsSuccessful)
            {
                var dbUser = await context.Database.GetOrCreateUserAsync(result.Value);

                return new TypeParserResult<(IGuildUser, User)>((result.Value, dbUser));
            }

            return new TypeParserResult<(IGuildUser, User)>(result.Reason);
        }
    }
}
