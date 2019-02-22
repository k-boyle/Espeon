using Discord;
using Espeon.Commands.Checks;
using Espeon.Databases.Entities;
using Humanizer;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionTarget = Espeon.Commands.Checks.PermissionTarget;

namespace Espeon.Commands.Modules
{
    /*
     * Kick
     * Ban
     * Warn
     * Revoke
     * View Warnings
     * Remove Reactions
     * Block
     * Blacklist
     */

    [Name("Moderation")]
    [RequireElevation(ElevationLevel.Mod)]
    public class Moderation : EspeonBase
    {
        [Command("Kick")]
        [Name("Kick User")]
        [RequirePermissions(PermissionTarget.Bot, GuildPermission.KickMembers)]
        public async Task KickUserAsync([RequireHierarchy] IGuildUser user, [Remainder] string reason = null)
        {
            await user.KickAsync(reason);
            await SendOkAsync(0, user.GetDisplayName());
        }

        [Command("Ban")]
        [Name("Ban User")]
        [RequirePermissions(PermissionTarget.Bot, GuildPermission.BanMembers)]
        public async Task BanUserAsync([RequireHierarchy] IGuildUser user, 
            [RequireRange(-1, 7)] int pruneDays = 0, [Remainder] string reason = null)
        {
            await user.BanAsync(pruneDays, reason);
            await SendOkAsync(0, user.GetDisplayName());
        }

        [Command("warn")]
        [Name("Warn User")]
        public async Task WarnUserAsync([RequireHierarchy] IGuildUser targetUser, 
            [RequireSpecificLength(200)]
            [Remainder]
            string reason)
        {
            var currentGuild = await Context.GuildStore
                .GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

            var currentCount = currentGuild.Warnings.Count(x => x.TargetUser == targetUser.Id) + 1;

            if(currentCount >= currentGuild.WarningLimit)
            {
                await SendNotOkAsync(0, targetUser.GetDisplayName(), currentCount);
            }

            currentGuild.Warnings.Add(new Warning
            {                
                TargetUser = targetUser.Id,
                Issuer = Context.User.Id,
                Reason = reason
            });

            await Context.GuildStore.SaveChangesAsync();

            await SendOkAsync(1, targetUser.GetDisplayName());
        }

        [Command("revoke")]
        [Name("Revoke Warning")]
        public async Task RevokeWarningAsync(string warningId)
        {
            var currentGuild = await Context.GuildStore
                .GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

            var warning = currentGuild.Warnings.FirstOrDefault(x => x.Id == warningId);

            if(warning is null)
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.Warnings.Remove(warning);

            await Context.GuildStore.SaveChangesAsync();

            await SendOkAsync(1);
        }

        [Command("warnings")]
        [Name("View Warnings")]
        public async Task ViewWarningsAsync([Remainder] IGuildUser targetUser)
        {
            var currentGuild = await Context.GuildStore
                .GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

            var foundWarnings = currentGuild.Warnings.Where(x => x.TargetUser == targetUser.Id).ToArray();

            if(foundWarnings.Length == 0)
            {
                await SendOkAsync(0);
                return;
            }

            var sb = new StringBuilder();

            foreach(var warning in foundWarnings)
            {
                sb.AppendLine($"**Id**: {warning.Id}, ");

                var issuer = Context.Guild.GetUser(warning.Issuer) as IGuildUser
                    ?? await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, warning.Issuer);

                sb.AppendLine($"**Issuer**: {issuer?.GetDisplayName() ?? "Not Found"}, ");

                sb.AppendLine($"**Issued On**: " +
                    $"{DateTimeOffset.FromUnixTimeMilliseconds(warning.IssuedOn).Humanize(culture: CultureInfo.InvariantCulture)}");

                sb.AppendLine($"**Reason**: {warning.Reason}");

                sb.AppendLine();
            }

            await SendOkAsync(1, sb.ToString());
        }

        [Command("noreactions")]
        [Name("Revoke Reactions")]
        [RequirePermissions(PermissionTarget.Bot, GuildPermission.ManageRoles)]
        public async Task RevokeReactionsAsync(
            [RequireHierarchy]
            [Remainder]
            IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            var role = Context.Guild.GetRole(currentGuild.NoReactions);

            if(role is null)
            {
                await SendNotOkAsync(0);

                return;
            }            

            await user.AddRoleAsync(role);
            await SendOkAsync(1);
        }

        [Command("restorereactions")]
        [Name("Restore Reactions")]
        [RequirePermissions(PermissionTarget.Bot, GuildPermission.ManageRoles)]
        public async Task RestoreReactionsAsync(
            [RequireHierarchy]
            [Remainder]
            IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            var role = Context.Guild.GetRole(currentGuild.NoReactions);

            if (role is null)
            {
                await SendNotOkAsync(0);

                return;
            }

            await user.RemoveRoleAsync(role);
            await SendOkAsync(1);
        }

        [Command("block")]
        [Name("Block User")]
        [RequirePermissions(PermissionTarget.Bot, ChannelPermission.ManageChannels)]
        public async Task BlockUserAsync(
            [RequireHierarchy]
            [Remainder]
            IGuildUser user)
        {
            await Context.Channel
                .AddPermissionOverwriteAsync(user, new OverwritePermissions(sendMessages: PermValue.Deny));

            await SendOkAsync(0);
        }

        [Command("blacklist")]
        [Name("Blacklist User")]
        public async Task BlacklistAsync(
            [RequireHierarchy]
            [Remainder]
            IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            
            if(currentGuild.RestrictedUsers.Contains(user.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.RestrictedUsers.Add(user.Id);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("unblacklist")]
        [Name("Unblacklist")]
        public async Task UnblacklistAsync(
            [RequireHierarchy]
            [Remainder]
            IGuildUser user)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);

            if (!currentGuild.RestrictedUsers.Contains(user.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            currentGuild.RestrictedUsers.Remove(user.Id);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }
    }
}
