using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers {
	public class EmoteTypeParser : EspeonTypeParser<LocalCustomEmoji> {
		public override ValueTask<TypeParserResult<LocalCustomEmoji>> ParseAsync(Parameter parameter, string value,
			EspeonContext context, IServiceProvider provider) {
			if (LocalCustomEmoji.TryParse(value, out var emoji)) {
				return TypeParserResult<LocalCustomEmoji>.Successful(emoji);
			}

			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return TypeParserResult<LocalCustomEmoji>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
		}
	}
}