using Discord;
using Espeon.Commands.Checks;
using Qmmands;
using System.Threading.Tasks;
using PermissionTarget = Espeon.Commands.Checks.PermissionTarget;

namespace Espeon.Commands.Modules
{
    /*
     * Kick
     * Ban
     * Warn
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
    }
}
