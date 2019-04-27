using Casino.Common.Discord.Net;
using Discord;
using Discord.WebSocket;
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

    [Name("Settings")]
    [RequireElevation(ElevationLevel.Admin)]
    [Description("Control the settings for your guild")]
    public class ServerSettings : EspeonBase
    {
        [Command("addprefix")]
        [Name("Add Prefix")]
        [Description("Add a new prefix for this guild")]
        public async Task AddPrefixAsync([Remainder] string prefix)
        {
            var currentGuild = Context.CurrentGuild;
            if (currentGuild.Prefixes.Contains(prefix))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.Prefixes.Add(prefix);
            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("removeprefix")]
        [Name("Remove Prefix")]
        [Description("Remove a prefix from the guild")]
        public async Task RemovePrefixAsync([Remainder] string prefix)
        {
            var currentGuild = Context.CurrentGuild;

            if (!currentGuild.Prefixes.Contains(prefix))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.Prefixes.Remove(prefix);

            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("restrict")]
        [Name("Restrict Channel")]
        [Description("Restrict the bots access to a channel")]
        public async Task RestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = Context.CurrentGuild;
            channel ??= Context.Channel;

            if (currentGuild.RestrictedChannels.Contains(channel.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.RestrictedChannels.Add(channel.Id);

            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("unrestrict")]
        [Name("Unrestrict Channel")]
        [Description("Unrestrict the bots access to a channel")]
        public async Task UnrestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = Context.CurrentGuild;
            channel ??= Context.Channel;

            if (!currentGuild.RestrictedChannels.Contains(channel.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.RestrictedChannels.Remove(channel.Id);

            Context.GuildStore.Update(currentGuild);
            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("admin")]
        [Name("Admin User")]
        [RequireGuildOwner]
        [Description("Promote a user to bot admin")]
        public async Task AdminUserAsync([Remainder] IGuildUser user)
        {
            var currentGuild = Context.CurrentGuild;

            if (!currentGuild.Admins.Contains(user.Id))
            {
                currentGuild.Admins.Add(user.Id);

                if (currentGuild.Moderators.Contains(user.Id))
                {
                    currentGuild.Moderators.Remove(user.Id);
                }                
                
                Context.GuildStore.Update(currentGuild);
                await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0, user.GetDisplayName()));
                return;
            }

            await SendNotOkAsync(1, user.GetDisplayName());
        }

        [Command("mod")]
        [Name("Moderate User")]
        [Description("Promote a user to bot moderator")]
        public async Task ModUserAsync([Remainder] IGuildUser user)
        {
            var currentGuild = Context.CurrentGuild;

            if (!currentGuild.Moderators.Contains(user.Id))
            {
                if (currentGuild.Admins.Contains(user.Id))
                {
                    await SendOkAsync(0, user.GetDisplayName());
                    return;
                }

                currentGuild.Moderators.Add(user.Id);

                Context.GuildStore.Update(currentGuild);
                await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1, user.GetDisplayName()));
                return;
            }

            await SendNotOkAsync(2, user.GetDisplayName());
        }

        [Command("deadmin")]
        [Name("Demote Admin")]
        [RequireGuildOwner]
        [Description("Demote a user from bot admin")]
        public async Task DemoteAdminAsync([Remainder] IGuildUser user)
        {
            if(user.Id == Context.Guild.OwnerId)
            {
                await SendNotOkAsync(0);
                return;
            }

            var currentGuild = Context.CurrentGuild;

            if (currentGuild.Admins.Contains(user.Id))
            {
                currentGuild.Admins.Remove(user.Id);
                Context.GuildStore.Update(currentGuild);

                await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1, user.GetDisplayName()));
                return;
            }

            await SendNotOkAsync(2, user.GetDisplayName());
        }

        [Command("demod")]
        [Name("Demote Moderator")]
        [RequireGuildOwner]
        [Description("Demote a user from bot moderator")]
        public async Task DemoteModeratorAsync([Remainder] IGuildUser user)
        {
            var currentGuild = Context.CurrentGuild;

            if (currentGuild.Moderators.Contains(user.Id))
            {
                currentGuild.Moderators.Remove(user.Id);
                Context.GuildStore.Update(currentGuild);

                await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0, user.GetDisplayName()));
                return;
            }

            await SendNotOkAsync(1, user.GetDisplayName());
        }

        [Command("welcomechannel")]
        [Name("Set Welcome Channel")]
        [Description("Set the default channel for welcoming new members")]
        public async Task SetWelcomeChannelAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.WelcomeChannelId = channel?.Id ?? 0;
            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("welcomemessage")]
        [Name("Set Welcome Message")]
        [Description("Set the welcome message. {{guild}} will be replaced by your guilds name, " +
                     "and {{user}} will be replaced by the newly joined members name")]
        public async Task SetWelcomeMessageAsync(
            [Remainder]
            [RequireSpecificLength(1900)]
            string message)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.WelcomeMessage = message;
            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("defaultrole")]
        [Name("Set Default Role")]
        [Description("Set the role to be added to new members")]
        public async Task SetDefaultRoleAsync(
            [Remainder]
            [RequirePositionHierarchy]
            SocketRole role = null)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.DefaultRoleId = role?.Id ?? 0;
            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("warninglimit")]
        [Name("Set Warning Limit")]
        [Description("Set the limit to how many warnings a user can have before they're flagged up")]
        public async Task SetWarningLimitAsync([RequireRange(0)] int limit)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.WarningLimit = limit;
            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("noreactionrole")]
        [Name("Set No Reactions Role")]
        [Description("Set the role that stops people from reacting")]
        public async Task SetNoReactionsRole(
            [Remainder]
            [RequirePositionHierarchy]
            SocketRole role = null)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.NoReactions = role?.Id ?? 0;
            Context.GuildStore.Update(currentGuild);

            await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }
    }
}
