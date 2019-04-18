using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public sealed class SocketRoleParser : TypeParser<SocketRole>
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext) ctx;

            SocketRole role = null;

            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[2] == '&' && value[^1]
                == '>' && ulong.TryParse(value[3..^1], out var id) || ulong.TryParse(value, out id))
                role = context.Guild.Roles.FirstOrDefault(x => x.Id == id);

            role ??= context.Guild.Roles
                .FirstOrDefault(x => string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            var resp = new Dictionary<ResponsePack, string>
            {
                [ResponsePack.Default] = "No role found matching the input",
                [ResponsePack.owo] = "no wole fwound"
            };

            return role is null ? new TypeParserResult<SocketRole>(resp[context.Invoker.ResponsePack]) 
                : new TypeParserResult<SocketRole>(role);
        }
    }
}
