using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Commands.Contexts;
using Umbreon.Commands.ModuleBases;
using Umbreon.Commands.Preconditions;
using Umbreon.Core;

namespace Umbreon.Commands.Modules
{
    [Name("Server Settings")]
    [Summary("Change the server specific settings for the bot")]
    [RequireRole(SpecialRole.Admin, Group = "RequireAdmin")]
    [RequireOwner(Group = "RequireAdmin")]
    [RequireGuildOwner(Group = "RequireAdmin")]
    public class ServerSettings : ServerSettingsBase<UmbreonContext>
    {
        // TODO Welcome channel, welcome message, MOTDChannel, MOTDMessage

        [Command("AddPrefix")]
        [Name("Add Prefix")]
        [Summary("Add a command prefix for the server")]
        [Usage("addprefix um!")]
        public async Task AddPrefix(
            [Name("New Prefix")]
            [Summary("The new prefix that you want to add")]
            [Remainder] string newPrefix)
        {
            var guild = await CurrentGuild;
            guild.Prefixes.Add(newPrefix);
            await SendMessageAsync("Prefix has been added");
            Database.UpdateObject("guilds", guild);
        }

        [Command("RemovePrefix")]
        [Name("Remove Prefix")]
        [Summary("Remove a command prefix for the server")]
        [Usage("removeprefix um!")]
        public async Task RemovePrefix(
            [Name("The Prefix")]
            [Summary("The prefix that you want to remove")]
            [Remainder] string newPrefix)
        {
            var guild = await CurrentGuild;
            if (guild.Prefixes.Count == 1)
            {
                await SendMessageAsync("This is the last prefix for the server you cannot remove it");
                return;
            }
            guild.Prefixes.Remove(newPrefix);
            await SendMessageAsync("Prefix has been removed");
            Database.UpdateObject("guilds", guild);
        }

        [Command("AdminRole")]
        [Name("Set Admin Role")]
        [Summary("Change which guild role will be designated as the admin role")]
        [Usage("adminrole admin")]
        public async Task SetAdmin(
            [Name("Admin Role")]
            [Summary("The role you want to make the admin role")]
            [Remainder] SocketRole adminRole)
        {
            var guild = await CurrentGuild;
            guild.AdminRole = adminRole.Id;
            await SendMessageAsync("Admin role has been set");
            Database.UpdateObject("guilds", guild);
        }

        [Command("ModRole")]
        [Name("Set Mod Role")]
        [Summary("Change which guild role will be designated as the mod role")]
        [Usage("modrole mod")]
        public async Task SetMod(
            [Name("Mod Role")]
            [Summary("The role you want to make the mod role")]
            [Remainder] SocketRole modRole)
        {
            var guild = await CurrentGuild;
            guild.ModRole = modRole.Id;
            await SendMessageAsync("Mod role has been set");
            Database.UpdateObject("guilds", guild);
        }

        [Command("mod")]
        [Name("Moderator")]
        [Summary("Promote a user to moderator")]
        [Usage("@Umbreon")]
        public async Task MakeMod(
            [Name("User")]
            [Summary("The user you want to promote")]
            [Remainder] SocketGuildUser user)
        {
            var guild = await CurrentGuild;
            if (Context.Guild.GetRole(guild.ModRole) is SocketRole modRole)
            {
                await user.AddRoleAsync(modRole);
                await SendMessageAsync("User has been made a moderator");
                return;
            }

            await SendMessageAsync("The mod role for this guild hasn't been set");
        }

        [Command("admin")]
        [Name("Admin")]
        [Summary("Promote a user to admin")]
        [Usage("admin @Umbreon")]
        [RequireGuildOwner(Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task MakeAdmin(
            [Name("User")]
            [Summary("The user you want to promote")]
            [Remainder] SocketGuildUser user)
        {
            var guild = await CurrentGuild;
            if (Context.Guild.GetRole(guild.AdminRole) is SocketRole adminRole)
            {
                await user.AddRoleAsync(adminRole);
                await SendMessageAsync("User has been made an administrator");
                return;
            }

            await SendMessageAsync("The admin role for this guild hasn't been set");
        }

        [Command("demod")]
        [Name("De-Moderator")]
        [Summary("Demote a user from moderator")]
        [Usage("demod @Umbreon")]
        public async Task RemoveMod(
            [Name("User")]
            [Summary("The user you want to demote")]
            [Remainder] SocketGuildUser user)
        {
            var guild = await CurrentGuild;
            if (Context.Guild.GetRole(guild.ModRole) is SocketRole modRole)
            {
                await user.RemoveRoleAsync(modRole);
                await SendMessageAsync("User has been demoted");
                return;
            }

            await SendMessageAsync("The mod role for this guild hasn't been set");
        }

        [Command("deadmin")]
        [Name("De-Admin")]
        [Summary("Demote a user from admin")]
        [Usage("deadmin @Umbreon")]
        [RequireGuildOwner(Group = "admin")]
        [RequireOwner(Group = "admin")]
        public async Task RemoveAdmin(
            [Name("User")]
            [Summary("The user you want to demote")]
            [Remainder] SocketGuildUser user)
        {
            var guild = await CurrentGuild;
            if (Context.Guild.GetRole(guild.AdminRole) is SocketRole adminRole)
            {
                await user.RemoveRoleAsync(adminRole);
                await SendMessageAsync("User has been demoted");
                return;
            }

            await SendMessageAsync("The admin role for this guild hasn't been set");
        }
    }
}
