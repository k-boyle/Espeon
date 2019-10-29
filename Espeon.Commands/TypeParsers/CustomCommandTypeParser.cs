using Espeon.Core.Database;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class CustomCommandTypeParser : EspeonTypeParser<CustomCommand> {
		public override async ValueTask<TypeParserResult<CustomCommand>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			var service = provider.GetService<ICustomCommandsService>();
			ImmutableArray<CustomCommand> commands = await service.GetCommandsAsync(context.GuildStore, context.Guild);

			CustomCommand found = commands.FirstOrDefault(x =>
				string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

			if (!(found is null)) {
				return TypeParserResult<CustomCommand>.Successful(found);
			}

			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return TypeParserResult<CustomCommand>.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
		}
	}
}