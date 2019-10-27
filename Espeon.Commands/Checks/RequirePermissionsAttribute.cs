using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionTarget = Espeon.Core.PermissionTarget;

namespace Espeon.Commands {
	public class RequirePermissionsAttribute : EspeonCheckBase {
		private readonly PermissionTarget _target;
		private readonly GuildPermission[] _guildPerms;
		private readonly ChannelPermission[] _channelPerms;

		public RequirePermissionsAttribute(PermissionTarget target, params GuildPermission[] guildPerms) {
			this._target = target;
			this._guildPerms = guildPerms;
			this._channelPerms = new ChannelPermission[0];
		}

		public RequirePermissionsAttribute(PermissionTarget target, params ChannelPermission[] channelPerms) {
			this._target = target;
			this._channelPerms = channelPerms;
			this._guildPerms = new GuildPermission[0];
		}

		public override ValueTask<CheckResult> CheckAsync(EspeonContext context, IServiceProvider provider) {

			SocketGuildUser user = this._target switch {
				PermissionTarget.User => context.User,
				PermissionTarget.Bot  => context.Guild.CurrentUser,
				_                     => null
			};

			GuildPermission[] failedGuildPerms =
				this._guildPerms.Where(guildPerm => !user.GuildPermissions.Has(guildPerm)).ToArray();

			ChannelPermissions channelPerms = user.GetPermissions(context.Channel);

			ChannelPermission[] failedChannelPerms =
				this._channelPerms.Where(channelPerm => !channelPerms.Has(channelPerm)).ToArray();

			if (failedGuildPerms.Length == 0 && failedChannelPerms.Length == 0) {
				return CheckResult.Successful;
			}

			var sb = new StringBuilder();

			foreach (GuildPermission guildPerm in failedGuildPerms) {
				sb.AppendLine(guildPerm);
			}

			foreach (ChannelPermission channelPerm in failedChannelPerms) {
				sb.AppendLine(channelPerm);
			}

			User u = context.Invoker;
			var response = provider.GetService<IResponseService>();

			string target = this._target == PermissionTarget.User ? "You" : "I";

			return CheckResult.Unsuccessful(response.GetResponse(this, u.ResponsePack, 0, target, sb));
		}
	}
}