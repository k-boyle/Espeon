using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using System;

namespace Espeon {
    public class EspeonCommandContext : DiscordCommandContext {
        public EspeonBot Bot { get; }
        public CachedMember Member { get; }
        public CachedGuild Guild { get; }
        public CachedTextChannel Channel { get; }
        public CachedMessage Message { get; }

        public EspeonCommandContext(EspeonBot bot, IPrefix prefix, CachedUserMessage message) 
                : base(bot, prefix, message) {
            if (!(message.Author is CachedMember member && message.Channel is CachedTextChannel channel)) {
                throw new InvalidOperationException("Bot should not be used in dms");
            }
            Bot = bot;
            Member = member;
            Guild = member.Guild;
            Channel = channel;
            Message = message;
        }
    }
}