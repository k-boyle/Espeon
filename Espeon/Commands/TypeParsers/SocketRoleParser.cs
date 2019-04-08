using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public sealed class SocketRoleParser : TypeParser<SocketRole>
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = ctx as EspeonContext;
            if (context.Guild == null)
                return new ValueTask<TypeParserResult<SocketRole>>(new TypeParserResult<SocketRole>("This command must be used in a guild."));

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

            return new ValueTask<TypeParserResult<SocketRole>>(role is null 
                ? new TypeParserResult<SocketRole>("No role found matching the input.") 
                : new TypeParserResult<SocketRole>(role));
        }
    }
}
