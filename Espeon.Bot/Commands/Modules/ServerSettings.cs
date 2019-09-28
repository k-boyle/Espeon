using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Bot.Commands
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
    public class ServerSettings : EspeonModuleBase
    {
        [Command("addprefix")]
        [Name("Add Prefix")]
        [Description("Add a new prefix for this guild")]
        public Task AddPrefixAsync(string prefix)
        {
            var currentGuild = Context.CurrentGuild;
            if (currentGuild.Prefixes.Contains(prefix))
                return SendNotOkAsync(0);

            currentGuild.Prefixes.Add(prefix);
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("removeprefix")]
        [Name("Remove Prefix")]
        [Description("Remove a prefix from the guild")]
        public Task RemovePrefixAsync(string prefix)
        {
            var currentGuild = Context.CurrentGuild;

            if (!currentGuild.Prefixes.Contains(prefix))
                return SendNotOkAsync(0);

            currentGuild.Prefixes.Remove(prefix);

            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("restrict")]
        [Name("Restrict Channel")]
        [Description("Restrict the bots access to a channel")]
        public Task RestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = Context.CurrentGuild;
            channel ??= Context.Channel;

            if (currentGuild.RestrictedChannels.Contains(channel.Id))
                return SendNotOkAsync(0);

            currentGuild.RestrictedChannels.Add(channel.Id);

            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("unrestrict")]
        [Name("Unrestrict Channel")]
        [Description("Unrestrict the bots access to a channel")]
        public Task UnrestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = Context.CurrentGuild;
            channel ??= Context.Channel;

            if (!currentGuild.RestrictedChannels.Contains(channel.Id))
                return SendNotOkAsync(0);

            currentGuild.RestrictedChannels.Remove(channel.Id);

            Context.GuildStore.Update(currentGuild);
            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
        }

        [Command("admin")]
        [Name("Admin User")]
        [RequireGuildOwner]
        [Description("Promote a user to bot admin")]
        public Task AdminUserAsync([Remainder] IGuildUser user)
        {
            var currentGuild = Context.CurrentGuild;

            if (currentGuild.Admins.Contains(user.Id))
                return SendNotOkAsync(1, user.GetDisplayName());

            currentGuild.Admins.Add(user.Id);

            if (currentGuild.Moderators.Contains(user.Id))
            {
                currentGuild.Moderators.Remove(user.Id);
            }

            Context.GuildStore.Update(currentGuild);
            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0, user.GetDisplayName()));
        }

        [Command("mod")]
        [Name("Moderate User")]
        [Description("Promote a user to bot moderator")]
        public Task ModUserAsync([Remainder] IGuildUser user)
        {
            var currentGuild = Context.CurrentGuild;

            if (currentGuild.Moderators.Contains(user.Id))
                return SendNotOkAsync(2, user.GetDisplayName());

            if (currentGuild.Admins.Contains(user.Id))
            {
                return SendOkAsync(0, user.GetDisplayName());
            }

            currentGuild.Moderators.Add(user.Id);

            Context.GuildStore.Update(currentGuild);
            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1, user.GetDisplayName()));
        }

        [Command("deadmin")]
        [Name("Demote Admin")]
        [RequireGuildOwner]
        [Description("Demote a user from bot admin")]
        public Task DemoteAdminAsync([Remainder] IGuildUser user)
        {
            if(user.Id == Context.Guild.OwnerId)
            {
                return SendNotOkAsync(0);
            }

            var currentGuild = Context.CurrentGuild;

            if (!currentGuild.Admins.Contains(user.Id))
                return SendNotOkAsync(2, user.GetDisplayName());

            currentGuild.Admins.Remove(user.Id);
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1, user.GetDisplayName()));
        }

        [Command("demod")]
        [Name("Demote Moderator")]
        [RequireGuildOwner]
        [Description("Demote a user from bot moderator")]
        public Task DemoteModeratorAsync([Remainder] IGuildUser user)
        {
            var currentGuild = Context.CurrentGuild;

            if (!currentGuild.Moderators.Contains(user.Id))
                return SendNotOkAsync(1, user.GetDisplayName());

            currentGuild.Moderators.Remove(user.Id);
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0, user.GetDisplayName()));
        }

        [Command("welcomechannel")]
        [Name("Set Welcome Channel")]
        [Description("Set the default channel for welcoming new members")]
        public Task SetWelcomeChannelAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.WelcomeChannelId = channel?.Id ?? 0;
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("welcomemessage")]
        [Name("Set Welcome Message")]
        [Description("Set the welcome message. {{guild}} will be replaced by your guilds name, " +
                     "and {{user}} will be replaced by the newly joined members name")]
        public Task SetWelcomeMessageAsync(
            [Remainder]
            [RequireSpecificLength(1900)]
            string message)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.WelcomeMessage = message;
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("defaultrole")]
        [Name("Set Default Role")]
        [Description("Set the role to be added to new members")]
        public Task SetDefaultRoleAsync(
            [Remainder]
            [RequirePositionHierarchy]
            SocketRole role = null)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.DefaultRoleId = role?.Id ?? 0;
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("warninglimit")]
        [Name("Set Warning Limit")]
        [Description("Set the limit to how many warnings a user can have before they're flagged up")]
        public Task SetWarningLimitAsync([RequireRange(0)] int limit)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.WarningLimit = limit;
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("noreactionrole")]
        [Name("Set No Reactions Role")]
        [Description("Set the role that stops people from reacting")]
        public Task SetNoReactionsRole(
            [Remainder]
            [RequirePositionHierarchy]
            SocketRole role = null)
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.NoReactions = role?.Id ?? 0;
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(0));
        }

        [Command("autoquote")]
        [Name("Toggle Auto-quoting")]
        [Description("Enables/disables the jump link to quote feature")]
        public Task ToggleAutoQuotingAsync()
        {
            var currentGuild = Context.CurrentGuild;
            currentGuild.AutoQuotes = !currentGuild.AutoQuotes;
            Context.GuildStore.Update(currentGuild);

            return Task.WhenAll(Context.GuildStore.SaveChangesAsync(),
                SendOkAsync(0, currentGuild.AutoQuotes ? "enabled" : "disabled"));
        }
    }
}
