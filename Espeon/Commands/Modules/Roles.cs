using Discord.WebSocket;
using Espeon.Commands.Checks;
using Qmmands;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    /*
     * Add
     * AddSelf
     * Remove
     * RemoveSelf
     * List
     */

    [Group("Roles")]
    [Name("Self Assigning Roles")]
    public class Roles : EspeonBase
    {
        [Command("add")]
        [Name("Add Role")]
        public async Task AddRoleAsync(
            [RequirePositionHierarchy]
            [Remainder]
            SocketRole role)
        {
            if (Context.User.Roles.Any(x => x.Id == role.Id))
            {
                await SendNotOkAsync(0);

                return;
            }

            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var roles = currentGuild.SelfAssigningRoles;

            if (roles.Contains(role.Id))
            {
                await Context.User.AddRoleAsync(role);
                await SendOkAsync(1);

                return;
            }

            await SendNotOkAsync(2);
        }

        [Command("addself")]
        [Name("Add SAR")]
        [RequireElevation(ElevationLevel.Mod)]
        public async Task AddSelfRoleAsync(
            [RequirePositionHierarchy]
            [Remainder]
            SocketRole role)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var roles = currentGuild.SelfAssigningRoles;

            if (roles.Contains(role.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            roles.Add(role.Id);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("remove")]
        [Name("Remove Role")]
        public async Task RemoveRoleAsync(
            [RequirePositionHierarchy]
            [Remainder]
            SocketRole role)
        {
            if(!Context.User.Roles.Any(x => x.Id == role.Id))
            {
                await SendNotOkAsync(0);

                return;
            }

            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var roles = currentGuild.SelfAssigningRoles;

            if(!roles.Contains(role.Id))
            {
                await SendNotOkAsync(1);
                return;
            }

            await Context.User.RemoveRoleAsync(role);
            await SendOkAsync(2);
        }

        [Command("removeself")]
        [Name("Remove SAR")]
        public async Task RemoveSelfAssigningRoleAsync([Remainder] SocketRole role)
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var roles = currentGuild.SelfAssigningRoles;

            if(!roles.Contains(role.Id))
            {
                await SendNotOkAsync(0);
                return;
            }

            roles.Remove(role.Id);

            await Context.GuildStore.SaveChangesAsync();
            await SendOkAsync(1);
        }

        [Command("list")]
        [Name("List Roles")]
        public async Task ListRolesAsync()
        {
            var currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild);
            var roles = currentGuild.SelfAssigningRoles
                .Select(x => Context.Guild.GetRole(x))
                .Where(x => !(x is null))
                .ToArray();

            await SendOkAsync(0, roles.Length > 0 ? string.Join('\n', roles.Select(x => x.Mention)) : "None");
        }
    }
}
