using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Modules.Contexts;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;
using Umbreon.TypeReaders;

namespace Umbreon.Modules
{
    [Group("set")]
    [Name("Server Settings")]
    [Summary("Change the server specific settings for the bot")]
    [RequireRole(SpecialRole.Admin, Group = "RequireAdmin")]
    [RequireOwner(Group = "RequireAdmin")]
    public class ServerSettings : ServerSettingsBase<GuildCommandContext>
    {
        // TODO Welcome channel, welcome message, MOTDChannel, MOTDMessage

        [Command("AddPrefix")]
        [Name("Add Prefix")]
        [Summary("Add a command prefix for the server")]
        [Usage("set addprefix um!")]
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
        [Usage("set removeprefix um!")]
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
        [Usage("set adminrole admin")]
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
        [Usage("set modrole mod")]
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
        [Usage("set disable Commands")]
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
        [Usage("set enable Commands")]
        public async Task EnableModule(
            [Name("Module")]
            [Summary("The module code of the module you want to enable")]
            [Remainder] Module type)
        {
            if (CurrentGuild.DisabledModules.Contains(type))
                CurrentGuild.DisabledModules.Remove(type);
            await SendMessageAsync("Module has been enabled");
        }

        [Command("Starboard")]
        [Name("Setup Starboard")]
        [Summary("Set the starboard channel for this guild")]
        [@Remarks("Leave blank to disable starboard")]
        [Usage("set starboard #starboard")]
        public async Task Starboard(
            [Name("Star Channel")]
            [Summary("The channel you want starboard to be, leave blank to disable starboard")]
            [Remainder] SocketTextChannel starChannel = null)
        {
            if (starChannel is null)
            {
                CurrentGuild.Starboard.Enabled = false;
                await SendMessageAsync("Starboard has been disabled");
                return;
            }
            
            CurrentGuild.Starboard.Enabled = true;
            CurrentGuild.Starboard.ChannelId = starChannel.Id;
            await SendMessageAsync("Starboard has been enabled");
        }

        [Command("Starlimit")]
        [Name("Star Limit")]
        [Summary("Set the star limit for starboard")]
        [Usage("set starlimit 3")]
        [RequireStarboard]
        public async Task StarLimit(
            [Name("Star Limit")]
            [Summary("How many stars are required")]
            [OverrideTypeReader(typeof(StarLimitTypeReader))] int starLimit)
        {
            CurrentGuild.Starboard.StarLimit = starLimit;
            await SendMessageAsync("Star limit has been updated");
        }

        [Command("CommandMatching")]
        [Name("Close Command Matching")]
        [Summary("Choose whether the bot will try match failed commands to the best match")]
        [@Remarks("This can lead to unexpected results")]
        [Usage("set commandmatching false")]
        public async Task SetMatching(
            [Name("Enabled")]
            [Summary("true of false")] bool enabled)
        {
            CurrentGuild.CloseCommandMatching = enabled;
            await SendMessageAsync("Close command matching has been changed");
        }
    }
}
