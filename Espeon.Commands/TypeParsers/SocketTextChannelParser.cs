using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class SocketTextChannelParser : EspeonTypeParser<SocketTextChannel> {
		public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			ResponsePack p = context.Invoker.ResponsePack;
			var response = provider.GetService<IResponseService>();

			if (context.Guild == null) {
				return new TypeParserResult<SocketTextChannel>(response.GetResponse(this, p, 0));
			}

			IReadOnlyCollection<SocketTextChannel> channels = context.Guild.TextChannels;

			SocketTextChannel channel = null;

			if (value.Length > 3 && value[0] == '<' && value[1] == '#' && value[^1] == '>' &&
			    ulong.TryParse(value[2..^1], out ulong id) || ulong.TryParse(value, out id)) {
				channel = channels.FirstOrDefault(x => x.Id == id);
			}

			channel??=channels.FirstOrDefault(x => x.Name == value);

			return channel is null
				? new TypeParserResult<SocketTextChannel>(response.GetResponse(this, p, 1))
				: new TypeParserResult<SocketTextChannel>(channel);
		}
	}
}