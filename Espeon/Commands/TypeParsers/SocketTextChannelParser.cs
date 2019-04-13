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
            var context = ctx as EspeonContext;
            if (context.Guild == null)
                return new ValueTask<TypeParserResult<SocketTextChannel>>(new TypeParserResult<SocketTextChannel>("This command must be used in a guild."));

            IEnumerable<SocketTextChannel> channels = context.Guild.TextChannels;

            SocketTextChannel channel = null;

            if (value.Length > 3 
                && value[0] == '<' 
                && value[1] == '#' 
                && value[^1] == '>' 
                && ulong.TryParse(value[2..^1], out var id)
                    || ulong.TryParse(value, out id))
                channel = channels.FirstOrDefault(x => x.Id == id);

            channel ??= channels.FirstOrDefault(x => x.Name == value);

            return new ValueTask<TypeParserResult<SocketTextChannel>>(channel is null 
                ? new TypeParserResult<SocketTextChannel>("No channel found matching the input.") 
                : new TypeParserResult<SocketTextChannel>(channel));
        }
    }
}
