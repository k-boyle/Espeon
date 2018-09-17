using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Commands.ModuleBases;
using Espeon.Commands.Preconditions;
using Espeon.Commands.TypeReaders;
using Espeon.Core;

namespace Espeon.Commands.Modules
{
    [Name("Moderation")]
    [Summary("Moderation commands for the server")]
    [RequireRole(SpecialRole.Mod, Group = "RequireRole")]
    [RequireRole(SpecialRole.Admin, Group = "RequireRole")]
    [RequireGuildOwner(Group = "RequireRole")]
    public class Moderation : EspeonBase
    {
        // TODO mute, warnings
        // TODO nickname

        [Command("kick")]
        [Name("Kick User")]
        [Summary("Kick a user from the server")]
        [Usage("kick Espeon being too cwl 8)")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(
            [Name("User To Kick")]
            [Summary("The user you want to kick")]
            [RequireHierarchy] SocketGuildUser userToKick,
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
        [Usage("ban Espeon 1 too cwl 4 scwl 8)")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(
            [Name("User To Ban")]
            [Summary("The user you want to ban")]
            [RequireHierarchy] SocketGuildUser userToBan,
            [Name("Prune Amount")]
            [Summary("The number of days of messages from the user you want to delete")]
            [OverrideTypeReader(typeof(BanLimitTypeReader))] int pruneAmount = 0,
            [Name("Reason")]
            [Summary("The reason for banning them")]
            [Remainder] string reason = null)
        {
            await Context.Guild.AddBanAsync(userToBan, pruneAmount, reason);
            await SendMessageAsync("User has been banned");
        }
    }
}
