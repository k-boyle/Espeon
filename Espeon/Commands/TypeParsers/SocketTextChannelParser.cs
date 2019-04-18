using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class SocketTextChannelParser : TypeParser<SocketTextChannel>
    {
        public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = (EspeonContext) ctx;

            var resp = new Dictionary<ResponsePack, string[]>
            {
                [ResponsePack.Default] = new []
                {
                    "This command must be used in a guild",
                    "No channel found matching the input"
                },
                [ResponsePack.owo] = new []
                {
                    "dis cwomand must be wused in a gwuild",
                    "no chwannel fwound"
                }
            };

            var p = context.Invoker.ResponsePack;

            if (context.Guild == null)
                return new TypeParserResult<SocketTextChannel>(resp[p][0]);

            var channels = context.Guild.TextChannels;

            SocketTextChannel channel = null;

            if (value.Length > 3 && value[0] == '<' && value[1] == '#' && value[^1] == '>' &&
                ulong.TryParse(value[2..^1], out var id) || ulong.TryParse(value, out id))
                channel = channels.FirstOrDefault(x => x.Id == id);

            channel ??= channels.FirstOrDefault(x => x.Name == value);

            return channel is null
                ? new TypeParserResult<SocketTextChannel>(resp[p][1]) 
                : new TypeParserResult<SocketTextChannel>(channel);
        }
    }
}
