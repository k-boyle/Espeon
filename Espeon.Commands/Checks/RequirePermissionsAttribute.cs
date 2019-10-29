using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequirePermissionsAttribute : EspeonCheckBase {
		private readonly PermissionTarget _target;
		private readonly PermissionType _type;
		private readonly Permission _permissions;

		public RequirePermissionsAttribute(PermissionTarget target, PermissionType type, Permission permissions) {
			this._target = target;
			this._type = type;
			this._permissions = permissions;
		}

		public override ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider) {

			CachedMember user = this._target switch {
				PermissionTarget.User => context.Member,
				PermissionTarget.Bot  => context.Guild.CurrentMember,
				_                     => null
			};

			if (this._type == PermissionType.Guild && user.Permissions.Has(this._permissions) ||
			    this._type == PermissionType.Channel &&
			    user.GetPermissionsFor(context.Channel).Has(this._permissions)) {
				return CheckResult.Successful;
			}

			User u = context.Invoker;
			var response = provider.GetService<IResponseService>();

			string target = this._target == PermissionTarget.User ? "You" : "I";

			return CheckResult.Unsuccessful(response.GetResponse(this, u.ResponsePack, 0, target,
				this._permissions.Humanize()));
		}
	}
}