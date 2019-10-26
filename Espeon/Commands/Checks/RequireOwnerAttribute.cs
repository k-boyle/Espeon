using Discord.Rest;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequireOwnerAttribute : EspeonCheckBase {
		public override async ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider) {
			var response = provider.GetService<IResponseService>();

			RestApplication app = await context.Client.GetApplicationInfoAsync();

			if (app.Owner.Id == context.User.Id || context.Client.CurrentUser.Id == context.User.Id) {
				return CheckResult.Successful;
			}

			User user = context.Invoker;

			return CheckResult.Unsuccessful(Command is null
				? response.GetResponse(this, user.ResponsePack, 0, Module?.Name)
				: response.GetResponse(this, user.ResponsePack, 1, Command?.Name));
		}
	}
}