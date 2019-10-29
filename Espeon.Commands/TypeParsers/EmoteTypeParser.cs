using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers {
	public class EmoteTypeParser : EspeonTypeParser<CachedGuildEmoji> {
		public override ValueTask<TypeParserResult<CachedGuildEmoji>> ParseAsync(Parameter parameter, string value,
			EspeonContext context, IServiceProvider provider) {
			if (context.Guild.Emojis.FirstOrDefault(x => x.ToString() == value) is {} emoji) {
				return TypeParserResult<CachedGuildEmoji>.Successful(emoji);
			}

			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return TypeParserResult<CachedGuildEmoji>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
		}
	}
}