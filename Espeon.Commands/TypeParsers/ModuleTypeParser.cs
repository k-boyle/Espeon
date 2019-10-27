using Espeon.Core;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class ModuleTypeParser : EspeonTypeParser<Module> {
		public override async ValueTask<TypeParserResult<Module>> ParseAsync(Parameter param, string value,
			EspeonContext context, IServiceProvider provider) {
			var commands = provider.GetService<CommandService>();

			IReadOnlyList<Module> modules = commands.GetAllModules();
			Module module = modules.SingleOrDefault(x =>
				string.Equals(x.Name, value, StringComparison.InvariantCultureIgnoreCase));

			ResponsePack p = context.Invoker.ResponsePack;
			var response = provider.GetService<IResponseService>();

			if (module is null) {
				bool isGuild = string.Equals(value, context.Guild.Name, StringComparison.InvariantCultureIgnoreCase);

				if (!isGuild) {
					return new TypeParserResult<Module>(response.GetResponse(this, p, 0, value));
				}

				module = modules.Single(x => x.Name == context.Guild.Id.ToString());
			}

			IResult result = await module.RunChecksAsync(context);

			return result.IsSuccessful
				? new TypeParserResult<Module>(module)
				: new TypeParserResult<Module>(response.GetResponse(this, p, 1));
		}
	}
}