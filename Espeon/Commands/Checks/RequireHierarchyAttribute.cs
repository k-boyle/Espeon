using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	public class RequireHierarchyAttribute : EspeonParameterCheckBase {
		public override async ValueTask<CheckResult> CheckAsync(object argument, EspeonContext context,
			IServiceProvider provider) {
			RestApplication appInfo = await context.Client.GetApplicationInfoAsync();

			var targetUser = (IGuildUser) argument;

			Guild currentGuild = context.CurrentGuild;

			ElevationLevel executor = currentGuild.Admins.Contains(context.User.Id) ? ElevationLevel.Admin :
				currentGuild.Moderators.Contains(context.User.Id) ? ElevationLevel.Mod : ElevationLevel.None;

			ElevationLevel target = currentGuild.Admins.Contains(targetUser.Id) ? ElevationLevel.Admin :
				currentGuild.Moderators.Contains(targetUser.Id) ? ElevationLevel.Mod : ElevationLevel.None;

			if (context.Guild.CurrentUser is null) {
				throw new ThisWasQuahusFaultException();
			}

			User user = context.Invoker;
			ResponsePack p = user.ResponsePack;
			var response = provider.GetService<IResponseService>();

			if (targetUser.Id == context.Guild.OwnerId) {
				return CheckResult.Unsuccessful(response.GetResponse(this, p, 0));
			}

			if (target >= executor) {
				return CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
			}

			if (targetUser is SocketGuildUser socket) {
				if (context.Guild.CurrentUser.Hierarchy <= socket.Hierarchy) {
					return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));
				}

				return context.User.Hierarchy > socket.Hierarchy
					? CheckResult.Successful
					: CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
			}

			IEnumerable<SocketRole> roles = targetUser.RoleIds.Select(x => context.Guild.GetRole(x));
			SocketRole[] ordered = roles.OrderBy(x => x.Position).ToArray();

			if (context.Guild.CurrentUser.Hierarchy <= ordered[0].Position) {
				return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));
			}

			return context.User.Hierarchy > ordered[0].Position
				? CheckResult.Successful
				: CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
		}
	}
}