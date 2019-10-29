using Disqord;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequirePositionHierarchy : EspeonParameterCheckBase {
		public override ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
			IServiceProvider provider) {
			var role = (CachedRole) argument;
			var response = provider.GetService<IResponseService>();

			if (role.Position <= context.Guild.CurrentMember.Hierarchy) {
				return CheckResult.Successful;
			}

			User user = context.Invoker;

			return CheckResult.Unsuccessful(response.GetResponse(this, user.ResponsePack, 0));
		}
	}
}