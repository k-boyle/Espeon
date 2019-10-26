using Discord;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers {
	public class EmoteTypeParser : EspeonTypeParser<Emote> {
		public override ValueTask<TypeParserResult<Emote>> ParseAsync(Parameter parameter, string value,
			EspeonContext context, IServiceProvider provider) {
			if (Emote.TryParse(value, out Emote emote)) {
				return TypeParserResult<Emote>.Successful(emote);
			}

			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return TypeParserResult<Emote>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
		}
	}
}