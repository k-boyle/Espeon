using Disqord;
using Disqord.Events;
using Espeon.Core.Database;
using Espeon.Core.Database.GuildStore;
using Espeon.Core.Database.UserStore;
using Espeon.Core.Services;
using Kommon.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class PurgingService : BaseService<InitialiseArgs>, IPurgingService {
		[Inject] private readonly DiscordClient _client;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly IServiceProvider _services;

		public PurgingService(IServiceProvider services) : base(services) {
			this._client.LeftGuild += args => this._events.RegisterEvent(() => LeftGuildAsync(args));
			this._client.MemberLeft += args => this._events.RegisterEvent(() => MemberLeftAsync(args));
			this._client.ChannelDeleted += args => this._events.RegisterEvent(() => ChannelDestroyedAsync(args));
			this._client.RoleDeleted += args => this._events.RegisterEvent(() => RoleDeletedAsync(args));
			this._client.RoleUpdated += args => this._events.RegisterEvent(() => RoleUpdatedAsync(args));
		}

		private async Task LeftGuildAsync(LeftGuildEventArgs args) {
			using var guildStore = this._services.GetService<GuildStore>();
			await guildStore.RemoveGuildAsync(args.Guild);
			await guildStore.SaveChangesAsync();
		}

		private async Task MemberLeftAsync(MemberLeftEventArgs args) {
			using var guildStore = this._services.GetService<GuildStore>();
			Guild guild = await guildStore.GetOrCreateGuildAsync(args.Guild, x => x.Warnings);

			bool removed = guild.Admins.Remove(args.User.Id) || guild.Moderators.Remove(args.User.Id) ||
			               guild.RestrictedUsers.Remove(args.User.Id) ||
			               guild.Warnings.RemoveAll(x => x.TargetUser == args.User.Id) > 0;

			if (removed) {
				guildStore.Update(guild);
				await guildStore.SaveChangesAsync();
			}

			if (this._client.Guilds.Count(x => x.Value.Members.Any(y => y.Value.Id == args.User.Id)) == 1) {
				using var userStore = this._services.GetService<UserStore>();

				await userStore.RemoveUserAsync(args.User);
				userStore.Update(args);

				await userStore.SaveChangesAsync();
			}
		}

		private async Task ChannelDestroyedAsync(ChannelDeletedEventArgs args) {
			if (!(args.Channel is CachedTextChannel textChannel)) {
				return;
			}

			using var guildStore = this._services.GetService<GuildStore>();
			Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild);

			bool removed = guild.RestrictedChannels.Remove(args.Channel.Id);

			if (removed) {
				await guildStore.SaveChangesAsync();
			}
		}

		private async Task RoleDeletedAsync(RoleDeletedEventArgs args) {
			using var guildStore = this._services.GetService<GuildStore>();

			Guild guild = await guildStore.GetOrCreateGuildAsync(args.Role.Guild);

			bool removed = guild.SelfAssigningRoles.Remove(args.Role.Id);

			if (removed) {
				guildStore.Update(guild);
				await guildStore.SaveChangesAsync();
			}
		}

		private async Task RoleUpdatedAsync(RoleUpdatedEventArgs args) {
			using var guildStore = this._services.GetService<GuildStore>();
			Guild guild = await guildStore.GetOrCreateGuildAsync(args.OldRole.Guild);
			ulong id = args.OldRole.Id;

			if (id != guild.DefaultRoleId && !guild.SelfAssigningRoles.Contains(id) && id != guild.NoReactions) {
				return;
			}
			
			if (args.NewRole is null || args.OldRole.Position == args.NewRole.Position) {
				return;
			}

			CachedMember currentUser = args.OldRole.Guild.CurrentMember;

			if (args.NewRole.Position <= currentUser.Hierarchy) {
				return;
			}

			if (id == guild.DefaultRoleId) {
				guild.DefaultRoleId = 0;
			} else if (id == guild.NoReactions) {
				guild.NoReactions = 0;
			} else {
				guild.SelfAssigningRoles.Remove(id);
			}

			guildStore.Update(guild);

			await guildStore.SaveChangesAsync();
		}
	}
}