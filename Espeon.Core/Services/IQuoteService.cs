namespace Espeon.Core.Services {
	public interface IQuoteService {
		bool TryGetLastJumpMessage(ulong channelId, out ulong messageId);
	}
}