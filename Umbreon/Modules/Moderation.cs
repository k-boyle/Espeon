using Discord;
using Discord.Commands;
using Discord.Net.Helpers;
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
        // TODO add dm messages

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
            await userToKick.TrySendDMAsync($"You have been kicked from {Context.Guild.Name} {(reason is null ? "" : $"for; {reason}")}");
            await userToKick.KickAsync(reason);
            await SendMessageAsync("User has been kicked");
        }

        [Command("ban")]
        [Name("Ban User")]
        [Summary("Ban a user from the server")]
        [Usage("mod ban Umbreon 1 too cwl 4 scwl 8)")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(
            [Name("User To Ban")]
            [Summary("The user you want to ban")] SocketGuildUser userToBan,
            [Name("Prune Amount")]
            [Summary("The number of days of messages from the user you want to delete")]
            [OverrideTypeReader(typeof(BanLimitTypeReader))] int pruneAmount = 0,
            [Name("Reason")]
            [Summary("The reason for banning them")]
            [Remainder] string reason = null)
        {
            await userToBan.TrySendDMAsync($"You have been banned from {Context.Guild.Name} {(reason is null ? "" : $"for; {reason}")}");
            await Context.Guild.AddBanAsync(userToBan, pruneAmount, reason);
            await SendMessageAsync("User has been banned");
        }
    }
}
