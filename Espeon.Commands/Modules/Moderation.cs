using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Humanizer;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands {
	/*
	* Kick
	* Ban
	* Warn
	* Revoke
	* View Warnings
	* Remove Reactions
	* Block
	* Blacklist
	*/

	[Name("Moderation")]
	[RequireElevation(ElevationLevel.Mod)]
	[Description("Commands for moderation of your guild")]
	public class Moderation : EspeonModuleBase {
		[Command("Kick")]
		[Name("Kick User")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Guild, Permission.KickMembers)]
		[Description("Kicks a user from the guild")]
		public Task KickUserAsync([RequireHierarchy] IMember user, [Remainder] string reason = null) {
			return Task.WhenAll(user.KickAsync(RestRequestOptions.FromReason(reason)),
				SendOkAsync(0, user.DisplayName));
		}

		[Command("Ban")]
		[Name("Ban User")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Guild, Permission.BanMembers)]
		[Description("Bans a user from your guild")]
		public Task BanUserAsync([RequireHierarchy] IMember user, [RequireRange(-1, 7)] int pruneDays = 0,
			[Remainder] string reason = null) {
			return Task.WhenAll(user.BanAsync(reason, pruneDays), SendOkAsync(0, user.DisplayName));
		}

		[Command("warn")]
		[Name("Warn User")]
		[Description("Adds a warning to the specified user")]
		public async Task WarnUserAsync([RequireHierarchy] IMember targetUser,
			[RequireSpecificLength(200)] [Remainder] string reason) {
			Guild currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

			int currentCount = currentGuild.Warnings.Count(x => x.TargetUser == targetUser.Id) + 1;

			if (currentCount >= currentGuild.WarningLimit) {
				await SendNotOkAsync(0, targetUser.DisplayName, currentCount);
			}

			currentGuild.Warnings.Add(new Warning {
				TargetUser = targetUser.Id,
				Issuer = Context.Member.Id,
				Reason = reason
			});

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1, targetUser.DisplayName));
		}

		[Command("revoke")]
		[Name("Revoke Warning")]
		[Description("Revokes the warning corresponding to the specified id")]
		public async Task RevokeWarningAsync(string warningId) {
			Guild currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

			Warning warning = currentGuild.Warnings.FirstOrDefault(x => x.Id == warningId);

			if (warning is null) {
				await SendNotOkAsync(0);
				return;
			}

			currentGuild.Warnings.Remove(warning);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}

		[Command("warnings")]
		[Name("View Warnings")]
		[Description("View a users warnings")]
		public async Task ViewWarningsAsync([Remainder] IMember targetUser) {
			Guild currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

			Warning[] foundWarnings = currentGuild.Warnings.Where(x => x.TargetUser == targetUser.Id).ToArray();

			if (foundWarnings.Length == 0) {
				await SendOkAsync(0);
				return;
			}

			var sb = new StringBuilder();

			foreach (Warning warning in foundWarnings) {
				sb.AppendLine($"**Id**: {warning.Id}, ");

				IMember issuer = Context.Guild.GetMember(warning.Issuer) as IMember ??
				                 await Context.Client.GetMemberAsync(Context.Guild.Id, warning.Issuer);

				sb.Append("**Issuer**: ").Append(issuer?.DisplayName ?? "Not Found").AppendLine(", ");

				sb.Append("**Issued On**: ").AppendLine(DateTimeOffset.FromUnixTimeMilliseconds(warning.IssuedOn)
					.Humanize(culture: CultureInfo.InvariantCulture));

				sb.Append("**Reason**: ").AppendLine(warning.Reason);

				sb.AppendLine();
			}

			await SendOkAsync(1, sb.ToString());
		}

		[Command("noreactions")]
		[Name("Revoke Reactions")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Guild, Permission.ManageRoles)]
		[Description("Adds the no reactions role to the specified user")]
		public async Task RevokeReactionsAsync([RequireHierarchy] [Remainder] IMember user) {
			Guild currentGuild = Context.CurrentGuild;

			CachedRole role = Context.Guild.GetRole(currentGuild.NoReactions);

			if (role is null) {
				await SendNotOkAsync(0);
				return;
			}

			await Task.WhenAll(user.GrantRoleAsync(role.Id, RestRequestOptions.FromReason("Reaction rights revoked")),
				SendOkAsync(1));
		}

		[Command("restorereactions")]
		[Name("Restore Reactions")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Guild, Permission.ManageRoles)]
		[Description("Removes the no reactions role from the specified user")]
		public async Task RestoreReactionsAsync([RequireHierarchy] [Remainder] IMember user) {
			Guild currentGuild = Context.CurrentGuild;

			CachedRole role = Context.Guild.GetRole(currentGuild.NoReactions);

			if (role is null) {
				await SendNotOkAsync(0);
				return;
			}

			await Task.WhenAll(user.RevokeRoleAsync(role.Id, RestRequestOptions.FromReason("Reaction rights restored")),
				SendOkAsync(1));
		}

		[Command("block")]
		[Name("Block User")]
		[RequirePermissions(PermissionTarget.Bot, PermissionType.Channel, Permission.ManageChannels)]
		[Description("Stops the specified user from talking in this channel")]
		public async Task BlockUserAsync([RequireHierarchy] [Remainder] IMember user) {
			await Channel.AddOrModifyOverwriteAsync(
				new LocalOverwrite(user,
					new Disqord.OverwritePermissions(ChannelPermissions.None, Permission.SendMessages)),
				RestRequestOptions.FromReason("User blocked from channel"));
			await SendOkAsync(0);
		}

		[Command("blacklist")]
		[Name("Blacklist User")]
		[Description("Blacklists a user from using the bot")]
		public async Task BlacklistAsync([RequireHierarchy] [Remainder] IMember user) {
			Guild currentGuild = Context.CurrentGuild;

			if (currentGuild.RestrictedUsers.Contains(user.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			currentGuild.RestrictedUsers.Add(user.Id);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}

		[Command("unblacklist")]
		[Name("Unblacklist")]
		[Description("Removes a user from the bots blacklist")]
		public async Task UnblacklistAsync([RequireHierarchy] [Remainder] IMember user) {
			Guild currentGuild = Context.CurrentGuild;

			if (!currentGuild.RestrictedUsers.Contains(user.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			currentGuild.RestrictedUsers.Remove(user.Id);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}
	}
}