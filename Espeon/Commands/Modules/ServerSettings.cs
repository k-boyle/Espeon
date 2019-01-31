using Qmmands;
using System.Threading.Tasks;

namespace Espeon.Commands.Modules
{
    /*
     * Prefix
     * Restrict
     * Admin
     * Mod
     * Demote
     * Welcome Channel
     * Welcome Message
     * Default Role
     * Star Limit
     */

    [Name("Settings")]
    public class ServerSettings : EspeonBase
    {
        [Command("addprefix")]
        [Name("Add Prefix")]
        public async Task AddPrefixAsync([Remainder] string prefix)
        {
            var currentGuild = await Context.GetCurrentGuildAsync();
            if (currentGuild.Prefixes.Contains(prefix))
            {
                await SendMessageAsync("This prefix already exists for this guild");
                return;
            }

            currentGuild.Prefixes.Add(prefix);

            await Context.Database.SaveChangesAsync();
            await SendMessageAsync("Prefix has been added");
        }

        [Command("removeprefix")]
        [Name("Remove Prefix")]
        public async Task RemovePrefixAsync([Remainder] string prefix)
        {
            var currentGuild = await Context.GetCurrentGuildAsync();

            if (!currentGuild.Prefixes.Contains(prefix))
            {
                await SendMessageAsync("This prefix doesn't exist for this guild");
                return;
            }

            currentGuild.Prefixes.Remove(prefix);

            await Context.Database.SaveChangesAsync();
            await SendMessageAsync("Prefix has been removed");
        }
    }
}
