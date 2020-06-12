using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Qmmands;
using System.Threading.Tasks;

namespace Espeon {
    [Name("Settings")]
    [Description("Configure Espeon for this guild")]
    [BotOwnerOnly]
    public class SettingsModule : EspeonCommandModule {
        public PrefixService PrefixService { get; set; }
        
        [Name("View Prefixes")]
        [Description("List all of the prefixes for this guild")]
        [Command("prefixes")]
        public async Task ListPrefixesAsync() {
            var prefixes = await PrefixService.GetPrefixesAsync(Context.Guild);
            await ReplyAsync(string.Join(", ", prefixes));
        }
        
        [Name("Add Prefix")]
        [Description("Adds a new prefix for this guild")]
        [Command("addprefix")]
        public async Task AddPrefixAsync([Remainder] string prefix) {
            await PrefixService.TryAddPrefixAsync(Context.Guild, new StringPrefix(prefix));
            await ReplyAsync("gucci");
        }
        
        [Name("Remove Prefix")]
        [Description("Removes a new prefix for this guild")]
        [Command("removeprefix")]
        public async Task RemovePrefixAsync([Remainder] string prefix) {
            await PrefixService.TryRemovePrefixAsync(Context.Guild, new StringPrefix(prefix));
            await ReplyAsync("gucci");
        }
    }
}