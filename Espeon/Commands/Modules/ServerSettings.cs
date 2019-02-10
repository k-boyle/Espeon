using Discord.WebSocket;
using Espeon.Commands.Checks;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
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
    public class ServerSettings : EspeonBase
    {
        [Command("addprefix")]
        [Name("Add Prefix")]
        public async Task AddPrefixAsync([Remainder] string prefix)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            if (currentGuild.Prefixes.Contains(prefix))
            {
                await SendMessageAsync("This prefix already exists for this guild");
                return;
            }

            currentGuild.Prefixes.Add(prefix);

            await Context.Database.SaveChangesAsync();
            await SendMessageAsync("Prefix has been added");
        }

        [Command("removeprefix")]
        [Name("Remove Prefix")]
        public async Task RemovePrefixAsync([Remainder] string prefix)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.Prefixes.Contains(prefix))
            {
                await SendMessageAsync("This prefix doesn't exist for this guild");
                return;
            }

            currentGuild.Prefixes.Remove(prefix);

            await Context.Database.SaveChangesAsync();
            await SendMessageAsync("Prefix has been removed");
        }

        [Command("restrict")]
        [Name("Restrict Channel")]
        public async Task RestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            channel ??= Context.Channel;

            if (currentGuild.RestrictedChannels.Contains(channel.Id))
            {
                await SendNotOkAsync("The bot is already restricted in this channel");
                return;
            }

            currentGuild.RestrictedChannels.Add(channel.Id);

            await Context.Database.SaveChangesAsync();
            await SendOkAsync("The bot has been restricted from this channel");
        }

        [Command("unrestrict")]
        [Name("Unrestrict Channel")]
        public async Task UnrestrictChannelAccessAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            channel ??= Context.Channel;

            if (!currentGuild.RestrictedChannels.Contains(channel.Id))
            {
                await SendNotOkAsync("The bot is not restricted in this channel");
                return;
            }

            currentGuild.RestrictedChannels.Remove(channel.Id);

            await Context.Database.SaveChangesAsync();
            await SendOkAsync("The bot has been unrestricted from this channel");
        }

        [Command("admin")]
        [Name("Admin User")]
        [RequireGuildOwner]
        public async Task AdminUserAsync([Remainder] SocketGuildUser user)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.Admins.Contains(user.Id))
            {
                currentGuild.Admins.Add(user.Id);

                if (currentGuild.Moderators.Contains(user.Id))
                {
                    currentGuild.Moderators.Remove(user.Id);
                }
                

                await Context.Database.SaveChangesAsync();

                await SendOkAsync($"{user.GetDisplayName()} has been promoted to an admin");
                return;
            }

            await SendNotOkAsync($"{user.GetDisplayName()} is already an admin");
        }

        [Command("mod")]
        [Name("Moderate User")]
        public async Task ModUserAsync([Remainder] SocketGuildUser user)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.Moderators.Contains(user.Id))
            {
                if (currentGuild.Admins.Contains(user.Id))
                {
                    await SendOkAsync($"{user.GetDisplayName()} is already an admin");
                    return;
                }

                currentGuild.Moderators.Add(user.Id);

                await Context.Database.SaveChangesAsync();
                await SendOkAsync($"{user.GetDisplayName()} has been promoted to a moderator");
                return;
            }

            await SendOkAsync($"{user.GetDisplayName()} is already a moderator");
        }

        [Command("deadmin")]
        [Name("Demote Admin")]
        [RequireGuildOwner]
        public async Task DemoteAdminAsync([Remainder] SocketGuildUser user)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);

            if (currentGuild.Admins.Contains(user.Id))
            {
                currentGuild.Admins.Remove(user.Id);

                await Context.Database.SaveChangesAsync();
                await SendOkAsync($"{user.GetDisplayName()} has been demoted");
                return;
            }

            await SendNotOkAsync($"{user.GetDisplayName()} isn't an admin in this guild");
        }

        [Command("demod")]
        [Name("Demote Moderator")]
        [RequireGuildOwner]
        public async Task DemoteModeratorAsync([Remainder] SocketGuildUser user)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);

            if (currentGuild.Moderators.Contains(user.Id))
            {
                currentGuild.Moderators.Remove(user.Id);

                await Context.Database.SaveChangesAsync();
                await SendOkAsync($"{user.GetDisplayName()} has been demoted");
                return;
            }

            await SendNotOkAsync($"{user.GetDisplayName()} isn't a moderator in this guild");
        }

        [Command("welcomechannel")]
        [Name("Set Welcome Channel")]
        public async Task SetWelcomeChannelAsync([Remainder] SocketTextChannel channel = null)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.WelcomeChannelId = channel?.Id ?? 0;
            
            await Context.Database.SaveChangesAsync();

            await SendOkAsync("Welcome channel has been set");
        }

        [Command("welcomemessage")]
        [Name("Set Welcome Message")]
        public async Task SetWelcomeMessageAsync(
            [Remainder]
            [ParameterLength(1900)]
            string message)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.WelcomeMessage = message;
            
            await Context.Database.SaveChangesAsync();

            await SendOkAsync("Welcome message has been set");
        }

        [Command("defaultrole")]
        [Name("Set Default Role")]
        public async Task SetDefaultRoleAsync([Remainder] SocketRole role = null)
        {
            var currentGuild = await Context.Database.GetOrCreateGuildAsync(Context.Guild);
            currentGuild.DefaultRoleId = role?.Id ?? 0;
            
            await Context.Database.SaveChangesAsync();
            await SendOkAsync("Default role has been updated");
        }
    }
}
