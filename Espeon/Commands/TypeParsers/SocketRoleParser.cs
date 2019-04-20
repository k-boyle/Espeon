using Discord.WebSocket;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
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
            var context = (EspeonContext) ctx;

            SocketRole role = null;

            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[2] == '&' && value[^1]
                == '>' && ulong.TryParse(value[3..^1], out var id) || ulong.TryParse(value, out id))
                role = context.Guild.Roles.FirstOrDefault(x => x.Id == id);

            role ??= context.Guild.Roles
                .FirstOrDefault(x => string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

            var response = provider.GetService<ResponseService>();
            var user = context.Invoker;

            return role is null 
                ? new TypeParserResult<SocketRole>(response.GetResponse(this, user.ResponsePack, 0)) 
                : new TypeParserResult<SocketRole>(role);
        }
    }
}
