using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class CommandTypeParser : EspeonTypeParser<Command> {
		public override ValueTask<TypeParserResult<Command>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			var commands = provider.GetService<CommandService>();
			Command command = commands.GetAllCommands().SingleOrDefault(x =>
				string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

			if (!(command is null)) {
				return new TypeParserResult<Command>(command);
			}

			var response = provider.GetService<IResponseService>();
			User user = context.Invoker;

			return new TypeParserResult<Command>(response.GetResponse(this, user.ResponsePack, 0));

		}
	}
}