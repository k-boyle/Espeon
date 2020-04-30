using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Espeon {
    public class EspeonCommandContext : DiscordCommandContext {
        public IServiceScope ServiceScope { get; }
        public new EspeonBot Bot { get; }
        public new CachedMember Member { get; }
        public new CachedTextChannel Channel { get; }

        public EspeonCommandContext(IServiceScope scope, EspeonBot bot, IPrefix prefix, CachedUserMessage message) 
                : base(bot, prefix, message, scope.ServiceProvider) {
            if (!(message.Author is CachedMember member && message.Channel is CachedTextChannel channel)) {
                throw new InvalidOperationException("Bot should not be used in dms");
            }
            ServiceScope = scope;
            Bot = bot;
            Member = member;
            Channel = channel;
        }
    }
}