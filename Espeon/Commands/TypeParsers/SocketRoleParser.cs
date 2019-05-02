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

            SocketRole role;

            var id = ParseId(value);

            if (id != 0)
            {
                role = context.Guild.Roles.FirstOrDefault(x => x.Id == id);
                if (role != null) return TypeParserResult<SocketRole>.Successful(role);
            }
            
            var response = provider.GetService<ResponseService>();
            var user = context.Invoker;
            
            var roles = context.Guild.Roles.Where(x =>
                string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase)).ToArray();

            switch (roles.Length)
            {
                case 0:
                    return TypeParserResult<SocketRole>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
                case 1:
                    return TypeParserResult<SocketRole>.Successful(roles[0]);
                default:
                    role = roles.FirstOrDefault(x => x.Name == value);
                    if (role == null)
                        // Casino, add this response
                        return TypeParserResult<SocketRole>.Unsuccessful(response.GetResponse(this, user.ResponsePack,
                            1));

                    return TypeParserResult<SocketRole>.Successful(role);
            }
        }
        
        private ulong ParseId(string value)
        {
            return value.Length > 3 && value[0] == '<' && value[1] == '@' && value[2] == '&' && value[^1] == '>' &&
                   ulong.TryParse(value[3..^1], out var id) || ulong.TryParse(value, out id)
                ? id
                : 0;
        }
    }
}
