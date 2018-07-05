using Discord;
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
    [Group("mod")]
    [Name("Moderation")]
    [Summary("Moderation commands for the server")]
    [RequireRole(SpecialRole.Mod, Group = "RequireRole")]
    [RequireRole(SpecialRole.Admin, Group = "RequireRole")]
    [RequireGuildOwner(Group = "RequireRole")]
    public class Moderation : UmbreonBase<GuildCommandContext>
    {
        // TODO mute, warnings
        // TODO promo users, demote users, annoucement
        // TODO nickname

        [Command("kick")]
        [Name("Kick User")]
        [Summary("Kick a user from the server")]
        [Usage("mod kick Umbreon being too cwl 8)")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(
            [Name("User To Kick")]
            [Summary("The user you want to kick")]
            SocketGuildUser userToKick,
            [Name("Reason")]
            [Summary("The reason for kicking them")]
            [Remainder] string reason = null)
        {
            await userToKick.KickAsync(reason);
            await SendMessageAsync("User has been kicked");
        }

        [Command("ban")]
        [Name("Ban User")]
        [Summary("Ban a user from the server")]
        [Usage("mod ban Umbreon too cwl 4 scwl 8)")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(
            [Name("User To Ban")]
            [Summary("The user you want to ban")]
            SocketGuildUser userToBan,
            [Name("Reason")]
            [Summary("The reason for banning them")]
            [Remainder] string reason = null)
        {
            await Context.Guild.AddBanAsync(userToBan, 1, reason);
            await SendMessageAsync("User has been banned");
        }
    }
}
