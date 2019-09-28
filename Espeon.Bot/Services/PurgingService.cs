using Casino.DependencyInjection;
using Discord.WebSocket;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Espeon.Bot.Services
{
    public class PurgingService : BaseService<InitialiseArgs>, IPurgingService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IEventsService _events;
        [Inject] private readonly IServiceProvider _services;

        public PurgingService(IServiceProvider services) : base(services)
        {
            _client.LeftGuild += guild => _events.RegisterEvent(() => LeftGuildAsync(guild));
            _client.UserLeft += user => _events.RegisterEvent(() => UserLeftAsync(user));
            _client.ChannelDestroyed += channel => _events.RegisterEvent(() => ChannelDestroyedAsync(channel));
            _client.RoleDeleted += role => _events.RegisterEvent(() => RoleDeletedAsync(role));
            _client.RoleUpdated += (before, after) => _events.RegisterEvent(() => RoleUpdatedAsync(before, after));
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

            var removed = guild.Admins.Remove(user.Id) ||
                          guild.Moderators.Remove(user.Id) ||
                          guild.RestrictedUsers.Remove(user.Id) ||
                          guild.Warnings.RemoveAll(x => x.TargetUser == user.Id) > 0;

            if (removed)
            {
                guildStore.Update(guild);
                await guildStore.SaveChangesAsync();
            }

            if (user.MutualGuilds.Count == 1)
            {
                using var userStore = _services.GetService<UserStore>();

                await userStore.RemoveUserAsync(user);
                userStore.Update(user);

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
            {
                guildStore.Update(guild);
                await guildStore.SaveChangesAsync();
            }
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

            guildStore.Update(guild);

            await guildStore.SaveChangesAsync();
        }
    }
}
