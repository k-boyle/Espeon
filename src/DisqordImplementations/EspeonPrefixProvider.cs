using Disqord;
using Disqord.Bot.Prefixes;
using Espeon.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.DisqordImplementations {
    public class EspeonPrefixProvider : IPrefixProvider {
        private readonly PrefixService _prefixService;
        
        public EspeonPrefixProvider(PrefixService prefixService) {
            this._prefixService = prefixService;
        }
        
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message) {
            if (message.Channel is CachedTextChannel channel) {
                return this._prefixService.GetPrefixesAsync(channel.Guild);
            }
            
            throw new InvalidOperationException("Bot should not be listening to dm's");
        }
    }
}