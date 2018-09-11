using Discord;
using System;
using System.Collections.Generic;

namespace Umbreon.Helpers
{
    public static class EmotesHelper
    {
        public static readonly Dictionary<string, Emote> Emotes = new Dictionary<string, Emote>(StringComparer.CurrentCultureIgnoreCase)
        {
            { "pokeball", Emote.Parse("<:pokeball:487332258290860041>") },
            { "greatball", Emote.Parse("<:greatball:487332755454164993>") },
            { "ultraball", Emote.Parse("<:ultraball:487333008601251850>") },
            { "masterball", Emote.Parse("<:masterball:488077901632372736>") },
            { "rarecandy", Emote.Parse("<:candy:487333156329095168>") },
            { "umbreon", Emote.Parse("<:umbreon:488857497080168459>") }
        };
    }
}
