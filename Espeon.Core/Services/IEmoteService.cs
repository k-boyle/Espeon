using Discord;
using System.Collections.Generic;

namespace Espeon.Core.Services {
	public interface IEmoteService {
		IDictionary<string, Emote> Collection { get; }
	}
}