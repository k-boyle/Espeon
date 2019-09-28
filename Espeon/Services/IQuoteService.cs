namespace Espeon.Services
{
    public interface IQuoteService
    {
        bool TryGetLastJumpMessage(ulong channelId, out ulong messageId);
    }
}
