using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using System.Threading.Tasks;

namespace Espeon {
    public abstract class EspeonCommandModule : DiscordModuleBase<EspeonCommandContext> {
        public LocalisationService LocalisationService { get; set; }
        
        protected async Task<RestUserMessage> ReplyAsync(
                LocalisationStringKey stringKey,
                LocalMentions mentions = null,
                params object[] args) {
            var localisedString = await LocalisationService.GetResponseAsync(Context.Member.Id, Context.Guild.Id, stringKey, args);
            return await Context.Channel.SendMessageAsync(localisedString, mentions: mentions ?? LocalMentions.None);
        }
        
        protected async Task<RestUserMessage> ReplyAsync(LocalisationStringKey stringKey, params object[] args) {
            return await ReplyAsync(stringKey, LocalMentions.None, args);
        }
    }
}