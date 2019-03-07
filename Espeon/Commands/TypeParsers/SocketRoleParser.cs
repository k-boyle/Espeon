using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public sealed class SocketRoleParser : TypeParser<SocketRole>
    {
        public override Task<TypeParserResult<SocketRole>> ParseAsync(Parameter param, string value, ICommandContext ctx, IServiceProvider provider)
        {
            var context = ctx as EspeonContext;
            if (context.Guild == null)
                return Task.FromResult(new TypeParserResult<SocketRole>("This command must be used in a guild."));

            SocketRole role = null;

            if (value.Length > 3
                && value[0] == '<' 
                && value[1] == '@' 
                && value[2] == '&' 
                && value[^1] == '>' 
                && ulong.TryParse(value[3..^1], out var id)
                    || ulong.TryParse(value, out id))
                role = context.Guild.Roles.FirstOrDefault(x => x.Id == id);

            role ??= context.Guild.Roles
                .FirstOrDefault(x => string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            return role is null
                ? Task.FromResult(new TypeParserResult<SocketRole>("No role found matching the input."))
                : Task.FromResult(new TypeParserResult<SocketRole>(role));
        }
    }
}
