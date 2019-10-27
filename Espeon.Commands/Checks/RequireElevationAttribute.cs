using Espeon.Core;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequireElevationAttribute : RequireGuildOwnerAttribute {
		private readonly ElevationLevel _level;

		public RequireElevationAttribute(ElevationLevel level) {
			this._level = level;
		}

		public override async ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider) {
			CheckResult result = await base.CheckAsync(context, provider);

			if (result.IsSuccessful) {
				return CheckResult.Successful;
			}

			var response = provider.GetService<IResponseService>();

			Guild currentGuild = context.CurrentGuild;

			ResponsePack p = context.Invoker.ResponsePack;

			return this._level switch {
				ElevationLevel.Mod => (currentGuild.Moderators.Contains(context.User.Id) ||
				                       currentGuild.Admins.Contains(context.User.Id)
					? CheckResult.Successful
					: CheckResult.Unsuccessful(response.GetResponse(this, p, 0))),
				ElevationLevel.Admin => (currentGuild.Admins.Contains(context.User.Id)
					? CheckResult.Successful
					: CheckResult.Unsuccessful(response.GetResponse(this, p, 1))),
				_ => CheckResult.Unsuccessful(response.GetResponse(this, p, 2))
			};
		}
	}
}