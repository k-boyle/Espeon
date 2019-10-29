using Disqord;

namespace Espeon.Core.Services {
	public interface IEmoteService {
		CachedGuildEmoji this[string index] { get; }
	}
}