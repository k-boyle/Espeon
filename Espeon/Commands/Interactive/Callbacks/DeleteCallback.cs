using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    public class DeleteCallback : IReactionCallback
    {
        public EspeonContext Context { get; }

        public bool RunOnGatewayThread => true;

        public IUserMessage Message { get; }
        public ICriterion<SocketReaction> Criterion { get; }

        private readonly IEmote _deleteEmote;
        public IEnumerable<IEmote> Reactions => new[] { _deleteEmote };

        private bool _isDeleted;

        private readonly SemaphoreSlim _deleteSemaphore;

        public DeleteCallback(EspeonContext context, IUserMessage message, IEmote deleteEmote, ICriterion<SocketReaction> criterion = null)
        {
            Context = context;
            Message = message;
            
            Criterion = criterion ?? new ReactionFromSourceUser(context.User.Id);

            _deleteEmote = deleteEmote;

            _deleteSemaphore = new SemaphoreSlim(1);
        }
        
        public Task InitialiseAsync()
            => Task.CompletedTask;

        public Task HandleTimeoutAsync() 
            => _isDeleted ? Task.CompletedTask : Message.DeleteAsync();

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            await _deleteSemaphore.WaitAsync();

            if (!reaction.Emote.Equals(_deleteEmote))
            {
                _deleteSemaphore.Release();
                return false;
            }

            await Message.DeleteAsync();
            _isDeleted = true;

            _deleteSemaphore.Release();
            return true;
        }
    }
}
