using Casino.DependencyInjection;
using Discord.WebSocket;
using Espeon.Core.Databases;
using Espeon.Core.Databases.GuildStore;
using Espeon.Core.Databases.UserStore;
using Espeon.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Services {
	public class PurgingService : BaseService<InitialiseArgs>, IPurgingService {
		[Inject] private readonly DiscordSocketClient _client;
		[Inject] private readonly IEventsService _events;
		[Inject] private readonly IServiceProvider _services;

		public PurgingService(IServiceProvider services) : base(services) {
			this._client.LeftGuild += guild => this._events.RegisterEvent(() => LeftGuildAsync(guild));
			this._client.UserLeft += user => this._events.RegisterEvent(() => UserLeftAsync(user));
			this._client.ChannelDestroyed +=
				channel => this._events.RegisterEvent(() => ChannelDestroyedAsync(channel));
			this._client.RoleDeleted += role => this._events.RegisterEvent(() => RoleDeletedAsync(role));
			this._client.RoleUpdated += (before, after) =>
				this._events.RegisterEvent(() => RoleUpdatedAsync(before, after));
		}

		private async Task LeftGuildAsync(SocketGuild guild) {
			using var guildStore = this._services.GetService<GuildStore>();
			await guildStore.RemoveGuildAsync(guild);
			await guildStore.SaveChangesAsync();
		}

		private async Task UserLeftAsync(SocketGuildUser user) {
			using var guildStore = this._services.GetService<GuildStore>();
			Guild guild = await guildStore.GetOrCreateGuildAsync(user.Guild, x => x.Warnings);

			bool removed = guild.Admins.Remove(user.Id) || guild.Moderators.Remove(user.Id) ||
			               guild.RestrictedUsers.Remove(user.Id) ||
			               guild.Warnings.RemoveAll(x => x.TargetUser == user.Id) > 0;

			if (removed) {
				guildStore.Update(guild);
				await guildStore.SaveChangesAsync();
			}

			if (user.MutualGuilds.Count == 1) {
				using var userStore = this._services.GetService<UserStore>();

				await userStore.RemoveUserAsync(user);
				userStore.Update(user);

				await userStore.SaveChangesAsync();
			}
		}

		private async Task ChannelDestroyedAsync(SocketChannel channel) {
			if (!(channel is SocketTextChannel textChannel)) {
				return;
			}

			using var guildStore = this._services.GetService<GuildStore>();
			Guild guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild);

			bool removed = guild.RestrictedChannels.Remove(channel.Id);

			if (removed) {
				await guildStore.SaveChangesAsync();
			}
		}

		private async Task RoleDeletedAsync(SocketRole role) {
			using var guildStore = this._services.GetService<GuildStore>();

			Guild guild = await guildStore.GetOrCreateGuildAsync(role.Guild);

			bool removed = guild.SelfAssigningRoles.Remove(role.Id);

			if (removed) {
				guildStore.Update(guild);
				await guildStore.SaveChangesAsync();
			}
		}

		private async Task RoleUpdatedAsync(SocketRole before, SocketRole after) {
			using var guildStore = this._services.GetService<GuildStore>();
			Guild guild = await guildStore.GetOrCreateGuildAsync(before.Guild);
			ulong id = before.Id;

			if (id != guild.DefaultRoleId && !guild.SelfAssigningRoles.Contains(id) && id != guild.NoReactions) {
				return;
			}

			if (after is null || before.Position == after.Position) {
				return;
			}

			SocketGuildUser currentUser = before.Guild.CurrentUser;

			if (after.Position <= currentUser.Hierarchy) {
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