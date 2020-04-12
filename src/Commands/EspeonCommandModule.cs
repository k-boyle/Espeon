using Disqord.Bot;
using Disqord.Rest;
using System.Threading.Tasks;

namespace Espeon {
    public abstract class EspeonCommandModule : DiscordModuleBase<EspeonCommandContext> {
        public LocalisationService LocalisationService { get; set; }
        
        protected async Task<RestUserMessage> SendLocalisedMessageAsync(string key) {
            var localisedString = await LocalisationService.GetResponseAsync(Context.Guild, Context.User, key);
            return await Context.Channel.SendMessageAsync(localisedString);
        }
    }
}