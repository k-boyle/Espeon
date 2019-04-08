using Discord;
using Espeon.Databases;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class UserTupleParser : TypeParser<(IGuildUser, User)>
    {
        public override async ValueTask<TypeParserResult<(IGuildUser, User)>> ParseAsync(Parameter param, string value, CommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as EspeonContext;

            var commands = provider.GetService<CommandService>();

            var userParser = commands.GetSpecificTypeParser<IGuildUser, IGuildUserTypeParser>();

            var result = await userParser.ParseAsync(param, value, context, provider);

            if(result.IsSuccessful)
            {
                var dbUser = await context.UserStore.GetOrCreateUserAsync(result.Value);

                return new TypeParserResult<(IGuildUser, User)>((result.Value, dbUser));
            }

            return new TypeParserResult<(IGuildUser, User)>(result.Reason);
        }
    }
}
