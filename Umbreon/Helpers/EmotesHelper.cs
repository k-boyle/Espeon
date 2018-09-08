﻿using System;
using Discord;
using System.Collections.Generic;

namespace Umbreon.Helpers
{
    public static class EmotesHelper
    {
        public static Dictionary<string, Emote> Emotes = new Dictionary<string, Emote>(StringComparer.CurrentCultureIgnoreCase)
        {
            { "pokeball", Emote.Parse("<:pokeball:487332258290860041>") },
            { "greatball", Emote.Parse("<:greatball:487332755454164993>") },
            { "ultraball", Emote.Parse("<:ultraball:487333008601251850>") },
            { "rarecandy", Emote.Parse("<:candy:487333156329095168>") }
        };
    }
}