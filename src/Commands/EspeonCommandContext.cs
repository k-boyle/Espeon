using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using System;

namespace Espeon {
    public class EspeonCommandContext : DiscordCommandContext {
        public new EspeonBot Bot { get; }
        public new CachedMember Member { get; }
        public new CachedTextChannel Channel { get; }

        public EspeonCommandContext(EspeonBot bot, IPrefix prefix, CachedUserMessage message) 
                : base(bot, prefix, message) {
            if (!(message.Author is CachedMember member && message.Channel is CachedTextChannel channel)) {
                throw new InvalidOperationException("Bot should not be used in dms");
            }
            Bot = bot;
            Member = member;
            Channel = channel;
        }
    }
}