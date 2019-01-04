using Discord;
using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Interactive.Criteria;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Espeon.Interactive.Callbacks
{
    public class DeleteCallback : IReactionCallback
    {
        public EspeonContext Context { get; }
        public IUserMessage Message { get; }
        public ICriterion<SocketReaction> Criterion { get; }

        private readonly IEmote _deleteEmote;
        public IEnumerable<IEmote> Reactions => new[] { _deleteEmote };

        public DeleteCallback(EspeonContext context, IUserMessage message, IEmote deleteEmote, ICriterion<SocketReaction> criterion = null)
        {
            Context = context;
            Message = message;
            
            Criterion = criterion ?? new ReactionFromSourceUser(context.User.Id);

            _deleteEmote = deleteEmote;
        }
        
        public Task InitialiseAsync()
            => Task.CompletedTask;

        public Task HandleTimeoutAsync()
            => Message.DeleteAsync();

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            if (!reaction.Emote.Equals(_deleteEmote))
                return false;

            await Message.DeleteAsync();
            return true;
        }
    }
}
