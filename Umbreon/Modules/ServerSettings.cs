using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

namespace Umbreon.Modules
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
            CurrentGuild.Prefixes.Add(newPrefix);
            await SendMessageAsync("Prefix has been added");
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
            if (CurrentGuild.Prefixes.Count == 1)
            {
                await SendMessageAsync("This is the last prefix for the server you cannot remove it");
                return;
            }
            CurrentGuild.Prefixes.Remove(newPrefix);
            await SendMessageAsync("Prefix has been removed");
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
            CurrentGuild.AdminRole = adminRole.Id;
            await SendMessageAsync("Admin role has been set");
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
            CurrentGuild.ModRole = modRole.Id;
            await SendMessageAsync("Mod role has been set");
        }

        [Command("Disable")]
        [Name("Disable Module")]
        [Summary("Disable one of the optional modules")]
        [Usage("disable Commands")]
        public async Task DisableModule(
            [Name("Module")]
            [Summary("The module code of the module you want to disable")]
            [Remainder] Module type)
        {
            if (!CurrentGuild.DisabledModules.Contains(type))
                CurrentGuild.DisabledModules.Add(type);
            await SendMessageAsync("Module has been disabled");
        }

        [Command("Enable")]
        [Name("Enable Module")]
        [Summary("Enable one of the optional modules")]
        [Usage("enable Commands")]
        public async Task EnableModule(
            [Name("Module")]
            [Summary("The module code of the module you want to enable")]
            [Remainder] Module type)
        {
            if (CurrentGuild.DisabledModules.Contains(type))
                CurrentGuild.DisabledModules.Remove(type);
            await SendMessageAsync("Module has been enabled");
        }

        [Command("CommandMatching")]
        [Name("Close Command Matching")]
        [Summary("Choose whether the bot will try match failed commands to the best match")]
        [Usage("commandmatching false")]
        public async Task SetMatching(
            [Name("Enabled")]
            [Summary("true of false")] bool enabled)
        {
            CurrentGuild.UnkownCommandResult = enabled;
            await SendMessageAsync("Close command matching has been changed");
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
            var modRole = Context.Guild.GetRole(CurrentGuild.ModRole);
            await user.AddRoleAsync(modRole);
            await SendMessageAsync("User has been made a moderator");
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
            var adminRole = Context.Guild.GetRole(CurrentGuild.AdminRole);
            await user.AddRoleAsync(adminRole);
            await SendMessageAsync("User has been made an admin");
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
            var modRole = Context.Guild.GetRole(CurrentGuild.ModRole);
            await user.RemoveRoleAsync(modRole);
            await SendMessageAsync("User has been demoted");
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
            var adminRole = Context.Guild.GetRole(CurrentGuild.AdminRole);
            await user.RemoveRoleAsync(adminRole);
            await SendMessageAsync("User has been demoted");
        }
    }
}
