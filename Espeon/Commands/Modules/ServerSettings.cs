using Qmmands;
using System.Threading.Tasks;
using Espeon.Database.Entities;

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
        private Guild CurrentGuild { get; set; }

        [Command("addprefix")]
        [Name("Add Prefix")]
        public Task AddPrefixAsync([Remainder] string prefix)
        {
            if (CurrentGuild.Prefixes.Contains(prefix))
            {
                return SendMessageAsync("This prefix already exists for this guild");
            }

            CurrentGuild.Prefixes.Add(prefix);

            return Task.WhenAll(Context.Database.SaveChangesAsync(), SendMessageAsync("Prefix has been added"));
        }

        [Command("removeprefix")]
        [Name("Remove Prefix")]
        public Task RemovePrefixAsync([Remainder] string prefix)
        {
            if (!CurrentGuild.Prefixes.Contains(prefix))
            {
                return SendMessageAsync("This prefix doesn't exist for this guild");
            }

            CurrentGuild.Prefixes.Remove(prefix);

            return Task.WhenAll(Context.Database.SaveChangesAsync(), SendMessageAsync("Prefix has been removed"));
        }

        protected override async Task BeforeExecutedAsync(Command command)
        {
            CurrentGuild = await Context.Database.GetCurrentGuildAsync(Context);
        }
    }
}
