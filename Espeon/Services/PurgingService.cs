using Discord.WebSocket;
using Espeon.Attributes;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class PurgingService : BaseService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IServiceProvider _services;

        public override Task InitialiseAsync(UserStore userStore, GuildStore guildStore, CommandStore commandStore, IServiceProvider services)
        {
            _client.LeftGuild += LeftGuildAsync;
            _client.UserLeft += UserLeftAsync;
            _client.ChannelDestroyed += ChannelDestroyedAsync;
            _client.RoleDeleted += RoleDeletedAsync;
            _client.RoleUpdated += RoleUpdatedAsync;

            return Task.CompletedTask;
        }

        private async Task LeftGuildAsync(SocketGuild guild)
        {
            using var guildStore = _services.GetService<GuildStore>();
            await guildStore.RemoveGuildAsync(guild);
            await guildStore.SaveChangesAsync();
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(user.Guild, x => x.Warnings);

            var removed = false;

            if (guild.Admins.Remove(user.Id))
                removed = true;

            if (guild.Moderators.Remove(user.Id))
                removed = true;

            if (guild.RestrictedUsers.Remove(user.Id))
                removed = true;

            if (guild.Warnings.RemoveAll(x => x.TargetUser == user.Id) > 0)
                removed = true;

            if(removed)
                await guildStore.SaveChangesAsync();

            if (user.MutualGuilds.Count == 1)
            {
                using var userStore = _services.GetService<UserStore>();

                await userStore.RemoveUserAsync(user);
                await userStore.SaveChangesAsync();
            }
        }

        private async Task ChannelDestroyedAsync(SocketChannel channel)
        {
            if (!(channel is SocketTextChannel textChannel))
                return;

            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild);

            var removed = guild.RestrictedChannels.Remove(channel.Id);

            if (removed)
                await guildStore.SaveChangesAsync();
        }

        private async Task RoleDeletedAsync(SocketRole role)
        {
            using var guildStore = _services.GetService<GuildStore>();

            var guild = await guildStore.GetOrCreateGuildAsync(role.Guild);

            var removed = guild.SelfAssigningRoles.Remove(role.Id);

            if (removed)
                await guildStore.SaveChangesAsync();
        }

        private async Task RoleUpdatedAsync(SocketRole before, SocketRole after)
        {
            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(before.Guild);
            var id = before.Id;

            if (id != guild.DefaultRoleId && !guild.SelfAssigningRoles.Contains(id) && id != guild.NoReactions)
                return;

            if (after is null || before.Position == after.Position)
                return;

            var currentUser = before.Guild.CurrentUser;

            if (after.Position <= currentUser.Hierarchy)
                return;

            if (id == guild.DefaultRoleId)
                guild.DefaultRoleId = 0;
            else if (id == guild.NoReactions)
                guild.NoReactions = 0;
            else
                guild.SelfAssigningRoles.Remove(id);

            await guildStore.SaveChangesAsync();
        }
    }
}
