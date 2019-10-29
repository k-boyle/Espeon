using Disqord;
using Disqord.Rest;
using Espeon.Core;
using Espeon.Core.Database;
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
			RestApplication appInfo = await context.Client.CurrentApplication.GetOrDownloadAsync();

			var targetUser = (IMember) argument;

			Guild currentGuild = context.CurrentGuild;

			ElevationLevel executor = currentGuild.Admins.Contains(context.Member.Id) ? ElevationLevel.Admin :
				currentGuild.Moderators.Contains(context.Member.Id) ? ElevationLevel.Mod : ElevationLevel.None;

			ElevationLevel target = currentGuild.Admins.Contains(targetUser.Id) ? ElevationLevel.Admin :
				currentGuild.Moderators.Contains(targetUser.Id) ? ElevationLevel.Mod : ElevationLevel.None;

			if (context.Guild.CurrentMember is null) {
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

			if (targetUser is CachedMember socket) {
				if (context.Guild.CurrentMember.Hierarchy <= socket.Hierarchy) {
					return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));
				}

				return context.Member.Hierarchy > socket.Hierarchy
					? CheckResult.Successful
					: CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
			}

			IEnumerable<CachedRole> roles = targetUser.RoleIds.Select(x => context.Guild.GetRole(x));
			CachedRole[] ordered = roles.OrderBy(x => x.Position).ToArray();

			if (context.Guild.CurrentMember.Hierarchy <= ordered[0].Position) {
				return CheckResult.Unsuccessful(response.GetResponse(this, p, 1));
			}

			return context.Member.Hierarchy > ordered[0].Position
				? CheckResult.Successful
				: CheckResult.Unsuccessful(response.GetResponse(this, p, 2));
		}
	}
}