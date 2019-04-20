using Discord.WebSocket;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class SocketTextChannelParser : TypeParser<SocketTextChannel>
    {
        public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext) ctx;

            var p = context.Invoker.ResponsePack;
            var response = provider.GetService<ResponseService>();

            if (context.Guild == null)
                return new TypeParserResult<SocketTextChannel>(response.GetResponse(this, p, 0));

            var channels = context.Guild.TextChannels;

            SocketTextChannel channel = null;

            if (value.Length > 3 && value[0] == '<' && value[1] == '#' && value[^1] == '>' &&
                ulong.TryParse(value[2..^1], out var id) || ulong.TryParse(value, out id))
                channel = channels.FirstOrDefault(x => x.Id == id);

            channel ??= channels.FirstOrDefault(x => x.Name == value);

            return channel is null
                ? new TypeParserResult<SocketTextChannel>(response.GetResponse(this, p, 1)) 
                : new TypeParserResult<SocketTextChannel>(channel);
        }
    }
}
