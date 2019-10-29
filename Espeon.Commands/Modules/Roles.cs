

using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Qmmands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands {
	/*
	* Add
	* AddSelf
	* Remove
	* RemoveSelf
	* List
	*/

	[Group("Roles")]
	[Name("Self Assigning Roles")]
	[Description("Manage your own roles")]
	public class Roles : EspeonModuleBase {
		[Command("add")]
		[Name("Add Role")]
		[Description("Adds a role from the available self assigning roles")]
		public async Task AddRoleAsync([RequirePositionHierarchy] [Remainder] CachedRole role) {
			if (Context.Member.Roles.Any(x => x.Value.Id == role.Id)) {
				await SendNotOkAsync(0);

				return;
			}

			Guild currentGuild = Context.CurrentGuild;
			ICollection<ulong> roles = currentGuild.SelfAssigningRoles;

			if (roles.Contains(role.Id)) {
				await Task.WhenAll(
					Context.Member.GrantRoleAsync(role.Id, RestRequestOptions.FromReason("Self assigning role")),
					SendOkAsync(1));
				return;
			}

			await SendNotOkAsync(2);
		}

		[Command("addself")]
		[Name("Add SAR")]
		[Description("Adds a new role to the available self assigning roles")]
		[RequireElevation(ElevationLevel.Mod)]
		public async Task AddSelfRoleAsync([RequirePositionHierarchy] [Remainder] CachedRole role) {
			Guild currentGuild = Context.CurrentGuild;
			ICollection<ulong> roles = currentGuild.SelfAssigningRoles;

			if (roles.Contains(role.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			roles.Add(role.Id);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}

		[Command("remove")]
		[Name("Remove Role")]
		[Description("Removes one of your roles if that role is in the self assigning roles collection")]
		public async Task RemoveRoleAsync([RequirePositionHierarchy] [Remainder] CachedRole role) {
			if (Context.Member.Roles.All(x => x.Value.Id != role.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			Guild currentGuild = Context.CurrentGuild;
			ICollection<ulong> roles = currentGuild.SelfAssigningRoles;

			if (!roles.Contains(role.Id)) {
				await SendNotOkAsync(1);
				return;
			}

			await Task.WhenAll(
				Context.Member.RevokeRoleAsync(role.Id, RestRequestOptions.FromReason("Self assigning role")),
				SendOkAsync(2));
		}

		[Command("removeself")]
		[Name("Remove SAR")]
		[Description("Removes a role from the available self assinging roles")]
		public async Task RemoveSelfAssigningRoleAsync([Remainder] CachedRole role) {
			Guild currentGuild = Context.CurrentGuild;
			ICollection<ulong> roles = currentGuild.SelfAssigningRoles;

			if (!roles.Contains(role.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			roles.Remove(role.Id);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}

		[Command("list")]
		[Name("List Roles")]
		[Description("Gets all of the available self assigning roles")]
		public async Task ListRolesAsync() {
			Guild currentGuild = Context.CurrentGuild;
			CachedRole[] roles = currentGuild.SelfAssigningRoles.Select(x => Context.Guild.GetRole(x))
				.Where(x => !(x is null)).ToArray();

			await SendOkAsync(0, roles.Length > 0 ? string.Join('\n', roles.Select(x => x.Mention)) : "None");
		}
	}
}