using Discord.WebSocket;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequirePositionHierarchy : EspeonParameterCheckBase {
		public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
			IServiceProvider provider) {
			var role = (SocketRole) argument;
			var response = provider.GetService<IResponseService>();

			if (role.Position <= context.Guild.CurrentUser.Hierarchy) {
				return CheckResult.Successful;
			}

			User user = context.Invoker;

			return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
		}
	}
}