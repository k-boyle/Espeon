using Disqord;
using Espeon.Core;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class CachedTextChannelParser : EspeonTypeParser<CachedTextChannel> {
		public override ValueTask<TypeParserResult<CachedTextChannel>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			ResponsePack p = context.Invoker.ResponsePack;
			var response = provider.GetService<IResponseService>();

			if (context.Guild == null) {
				return new TypeParserResult<CachedTextChannel>(response.GetResponse(this, p, 0));
			}

			IReadOnlyDictionary<Snowflake, CachedTextChannel> channels = context.Guild.TextChannels;

			CachedTextChannel channel = null;

			if (value.Length > 3 && value[0] == '<' && value[1] == '#' && value[^1] == '>' &&
			    ulong.TryParse(value[2..^1], out ulong id) || ulong.TryParse(value, out id)) {
				channel = channels.FirstOrDefault(x => x.Value.Id == id).Value;
			}

			channel ??= channels.FirstOrDefault(x => x.Value.Name == value).Value;

			return channel is null
				? new TypeParserResult<CachedTextChannel>(response.GetResponse(this, p, 1))
				: new TypeParserResult<CachedTextChannel>(channel);
		}
	}
}