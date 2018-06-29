using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Umbreon.Attributes;
using Umbreon.Core;
using Umbreon.Modules.ModuleBases;
using Umbreon.Preconditions;

namespace Umbreon.Modules
{
    [Group("set")]
    [Name("Server Settings")]
    [Summary("Change the server specific settings for the bot")]
    [RequireRole(SpecialRole.Admin, Group = "RequireAdmin")]
    [RequireOwner(Group = "RequireAdmin")]
    public class ServerSettings : ServerSettingsBase<ICommandContext>
    {
        [Command("Prefix")]
        [Name("Server Prefix")]
        [Summary("Change the command prefix for the server")]
        [Usage("set prefix um!")]
        public async Task SetPrefix(
            [Name("New Prefix")]
            [Summary("The new prefix that you want")]
            [Remainder] string newPrefix)
        {
            CurrentGuild.Prefix = newPrefix;
            await SendMessageAsync("Prefix has been changed");
        }

        [Command("AdminRole")]
        [Name("Set Admin Role")]
        [Summary("Change which guild role will be designated as the admin role")]
        public async Task SetAdmin(
            [Name("Admin Role")]
            [Summary("The role you want to make the admin role")]
            [Remainder] SocketRole adminRole)
        {
            CurrentGuild.AdminRole = adminRole.Id;
            await SendMessageAsync("Admin role has been set");
        }
    }
}
