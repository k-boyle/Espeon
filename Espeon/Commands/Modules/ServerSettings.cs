﻿using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Enums;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    /*
     * Prefix
     * Restrict
     * Admin
     * Mod
     * Demote
     * Welcome Channel
     * Welcome Message
     * Default Role
     * Star Limit
     */

    //TODO force language pack
    [Name("Settings")]
    [RequireElevation(ElevationLevel.Admin)]
    public class ServerSettings : EspeonBase
    {
        [Command("addprefix")]
        [Name("Add Prefix")]
        public async Task AddPrefixAsync([Remainder] string prefix)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            if (currentGuild.Prefixes.Contains(prefix))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.Prefixes.Add(prefix);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("removeprefix")]
        [Name("Remove Prefix")]
        public async Task RemovePrefixAsync([Remainder] string prefix)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.Prefixes.Contains(prefix))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.Prefixes.Remove(prefix);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("restrict")]
        [Name("Restrict Channel")]
        public async Task RestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            channel ??= Context.Channel;

            if (currentGuild.RestrictedChannels.Contains(channel.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.RestrictedChannels.Add(channel.Id);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("unrestrict")]
        [Name("Unrestrict Channel")]
        public async Task UnrestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            channel ??= Context.Channel;

            if (!currentGuild.RestrictedChannels.Contains(channel.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.RestrictedChannels.Remove(channel.Id);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("admin")]
        [Name("Admin User")]
        [RequireGuildOwner]
        public async Task AdminUserAsync([Remainder] IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.Admins.Contains(user.Id))
            {
                currentGuild.Admins.Add(user.Id);

                if (currentGuild.Moderators.Contains(user.Id))
                {
                    currentGuild.Moderators.Remove(user.Id);
                }                

                await Context.GuildStore.SaveChangesAsync();

                await SendOkAsync(0, user.GetDisplayName());
                return;
            }

            await SendNotOkAsync(1, user.GetDisplayName());
        }

        [Command("mod")]
        [Name("Moderate User")]
        public async Task ModUserAsync([Remainder] IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.Moderators.Contains(user.Id))
            {
                if (currentGuild.Admins.Contains(user.Id))
                {
                    await SendOkAsync(0, user.GetDisplayName());
                    return;
                }

                currentGuild.Moderators.Add(user.Id);

                await Context.GuildStore.SaveChangesAsync();
                await SendOkAsync(1, user.GetDisplayName());
                return;
            }

            await SendOkAsync(2, user.GetDisplayName());
        }

        [Command("deadmin")]
        [Name("Demote Admin")]
        [RequireGuildOwner]
        public async Task DemoteAdminAsync([Remainder] IGuildUser user)
        {
            if(user.Id == Context.Guild.OwnerId)
            {
                await SendNotOkAsync(0);
                return;
            }

            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            if (currentGuild.Admins.Contains(user.Id))
            {
                currentGuild.Admins.Remove(user.Id);

                await Context.GuildStore.SaveChangesAsync();
                await SendOkAsync(1, user.GetDisplayName());
                return;
            }

            await SendNotOkAsync(2, user.GetDisplayName());
        }

        [Command("demod")]
        [Name("Demote Moderator")]
        [RequireGuildOwner]
        public async Task DemoteModeratorAsync([Remainder] IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            if (currentGuild.Moderators.Contains(user.Id))
            {
                currentGuild.Moderators.Remove(user.Id);

                await Context.GuildStore.SaveChangesAsync();
                await SendOkAsync(0, user.GetDisplayName());
                return;
            }

            await SendNotOkAsync(1, user.GetDisplayName());
        }

        [Command("welcomechannel")]
        [Name("Set Welcome Channel")]
        public async Task SetWelcomeChannelAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.WelcomeChannelId = channel?.Id ?? 0;
            
            await Context.GuildStore.SaveChangesAsync();

            await SendOkAsync(0);
        }

        [Command("welcomemessage")]
        [Name("Set Welcome Message")]
        public async Task SetWelcomeMessageAsync(
            [Remainder]
            [RequireSpecificLength(1900)]
            string message)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.WelcomeMessage = message;
            
            await Context.GuildStore.SaveChangesAsync();

            await SendOkAsync(0);
        }

        [Command("defaultrole")]
        [Name("Set Default Role")]
        public async Task SetDefaultRoleAsync([Remainder] SocketRole role = null)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.DefaultRoleId = role?.Id ?? 0;
            
            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(0);
        }

        [Command("warninglimit")]
        [Name("Set Warning Limit")]
        public async Task SetWarningLimitAsync([RequireRange(0)] int limit)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.WarningLimit = limit;

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(0);
        }

        [Command("noreactionrole")]
        [Name("Set No Reactions Role")]
        public async Task SetNoReactionsRole([Remainder] SocketRole role)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.NoReactions = role.Id;

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(0);
        }
    }
}