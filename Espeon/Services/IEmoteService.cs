using Discord;
using System.Collections.Generic;

namespace Espeon.Services
{
    public interface IEmoteService
    {
        IDictionary<string, Emote> Collection { get; }
    }
}
